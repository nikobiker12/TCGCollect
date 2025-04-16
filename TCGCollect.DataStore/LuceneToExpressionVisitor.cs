using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using TCGCollect.DataStore;

namespace TCGCollect.Core
{
    internal class LuceneToExpressionVisitor : LuceneParserBaseVisitor<Expression>
    {
        private readonly ParameterExpression _parameter;

        public LuceneToExpressionVisitor()
        {
            // Create a parameter for the Card object
            _parameter = Expression.Parameter(typeof(Card), "card");
        }

        public override Expression VisitQuery([NotNull] LuceneParser.QueryContext context)
        {
            // Combine all clauses in the query using AND logic
            var expressions = context.disjQuery().Select(Visit).ToList();
            return expressions.Aggregate(Expression.AndAlso);
        }

        public override Expression VisitDisjQuery([NotNull] LuceneParser.DisjQueryContext context)
        {
            // Combine all conjunctive queries using OR logic
            var expressions = context.conjQuery().Select(Visit).ToList();
            return expressions.Aggregate(Expression.OrElse);
        }

        public override Expression VisitConjQuery([NotNull] LuceneParser.ConjQueryContext context)
        {
            // Combine all modClauses using AND logic
            var expressions = context.modClause().Select(Visit).ToList();
            return expressions.Aggregate(Expression.AndAlso);
        }

        public override Expression VisitModClause([NotNull] LuceneParser.ModClauseContext context)
        {
            // Handle modifiers (+, -, NOT) and clauses
            var clauseExpression = Visit(context.clause());
            if (context.modifier() != null)
            {
                var modifier = context.modifier().GetText();
                if (modifier == "-")
                {
                    // Negate the clause
                    return Expression.Not(clauseExpression);
                }
                else if (modifier.ToLower() == "not")
                {
                    // Negate the clause
                    return Expression.Not(clauseExpression);
                }
            }
            return clauseExpression;
        }

        public override Expression VisitTerm([NotNull] LuceneParser.TermContext context)
        {
            //// Handle boosting (e.g., term^2)
            //if (context.CARAT() != null && context.NUMBER() != null)
            //{
            //    var term = context.TERM()?.GetText() ?? context.NUMBER()?.GetText();
            //    var boost = double.Parse(context.NUMBER().GetText());
            //    // Implement boosting logic here (e.g., adjust scoring)
            //    throw new NotImplementedException("Boosting is not yet implemented.");
            //}

            //// Handle regular expressions (e.g., /regex/)
            //if (context.REGEXPTERM() != null)
            //{
            //    var regex = context.REGEXPTERM().GetText().Trim('/');
            //    // Implement regex matching logic here
            //    throw new NotImplementedException("Regular expression matching is not yet implemented.");
            //}

            //// Handle numeric terms
            //if (context.NUMBER() != null)
            //{
            //    var number = context.NUMBER().GetText();
            //    return Expression.Constant(Convert.ToInt32(number));
            //}

            // Handle plain terms
            if (context.TERM() != null)
            {
                var term = context.TERM().GetText();
                return Expression.Constant(term);
            }

            // Handle quoted terms (e.g., "exact phrase")
            if (context.quotedTerm() != null)
            {
                var quotedTerm = context.quotedTerm().QUOTED().GetText().Trim('"');
                return Expression.Constant(quotedTerm);
            }

            throw new InvalidOperationException("Unsupported term type.");
        }

        private enum OperatorType
        {
            Equals,
            Like
        }

        public override Expression VisitFieldExpr([NotNull] LuceneParser.FieldExprContext context)
        {
            // Visit each field and value pair
            var field = context.fieldName()?.GetText();
            var value = Visit(context.term());

            OperatorType operatorType = OperatorType.Equals;
            if (context.TILDE() != null)
            {
                // Handle the like operator
                operatorType = OperatorType.Like;
            }

            string cardProperty = MapCardFieldToProperty(field);
            if (!string.IsNullOrEmpty(cardProperty))
            {
                // Build the LINQ expression for the clause
                var property = Expression.Property(_parameter, cardProperty);
                return BuildStringExpression(property, value, operatorType);
            }

            string cardPartProperty = MapCardPartFieldToProperty(field);
            if (!string.IsNullOrEmpty(cardPartProperty))
            {
                // Access the Faces property of the Card
                var facesProperty = Expression.Property(_parameter, nameof(Card.Faces));

                // Access the Parts property of each CardFace
                var faceParameter = Expression.Parameter(typeof(CardFace), "face");
                var partsProperty = Expression.Property(faceParameter, nameof(CardFace.Parts));
                
                // Access the specific property of each CardPart
                var partParameter = Expression.Parameter(typeof(CardPart), "part");
                var partProperty = Expression.Property(partParameter, cardPartProperty);

                // Create expressions to check if any part's property matches any value
                var partPredicate = BuildStringExpression(partProperty, value, operatorType);

                // Use the Any method to check if any part satisfies the predicate
                var anyPartMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(CardPart));
                var anyPartExpression = Expression.Call(anyPartMethod, partsProperty, Expression.Lambda(partPredicate, partParameter));

                // Use the Any method to check if any face satisfies the part predicate
                var anyFaceMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(CardFace));
                var anyFaceExpression = Expression.Call(anyFaceMethod, facesProperty, Expression.Lambda(anyPartExpression, faceParameter));

                return anyFaceExpression;
            }

            throw new InvalidOperationException($"Unknown field: {field}");
        }

        private Expression BuildStringExpression(MemberExpression memberExpression, Expression value, OperatorType operatorType)
        {
            return operatorType switch
            {
                OperatorType.Equals => Expression.Call(
                    typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string), typeof(StringComparison) })!,
                    memberExpression,
                    value,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase)),
                OperatorType.Like => Expression.Call(
                    memberExpression,
                    typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) })!,
                    value,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase)),
                _ => throw new InvalidOperationException($"Unsupported operator type: {operatorType}")
            };
        }

        public override Expression VisitGroupingExpr([NotNull] LuceneParser.GroupingExprContext context)
        {
            // Handle grouped expressions (e.g., parentheses)
            return Visit(context.query());
        }

        public Expression<Func<Card, bool>> BuildPredicate(LuceneParser.QueryContext context)
        {
            // Visit the query and construct the predicate
            var body = Visit(context);
            return Expression.Lambda<Func<Card, bool>>(body, _parameter);
        }

        private string MapCardFieldToProperty(string field)
        {
            // Map the Lucene field to the Card property
            return field?.ToLower() switch
            {
                "g" => nameof(Card.Game),
                "game" => nameof(Card.Game),
                "r" => nameof(Card.Rarity),
                "rarity" => nameof(Card.Rarity),
                "s" => nameof(Card.SetName),
                "set" => nameof(Card.SetName),
                "number" => nameof(Card.Number),
                "language" => nameof(Card.Language),
                "foil" => nameof(Card.IsFoil),
                _ => string.Empty
            };
        }

        private string MapCardPartFieldToProperty(string field)
        {
            // Map the Lucene field to the CardPart property
            return field?.ToLower() switch
            {
                "n" => nameof(CardPart.Name),
                "name" => nameof(CardPart.Name),
                "text" => nameof(CardPart.Text),
                "t" => nameof(CardPart.Type),
                "type" => nameof(CardPart.Type),
                "artist" => nameof(CardPart.Artist),
                _ => string.Empty
            };
        }


    }
}
