using Antlr4.Runtime;
using TCGCollect.Core;

namespace TCGCollect.DataStore
{
    internal class LuceneCardSearchHelper
    {
        public static IEnumerable<Card> Filter(IQueryable<Card> cards, string filter)
        {
            if (cards is null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            if (String.IsNullOrEmpty(filter))
            {
                return cards;
            }

            var inputStream = new AntlrInputStream(filter);
            var lexer = new LuceneLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new LuceneParser(commonTokenStream);

            LuceneToExpressionVisitor visitor = new();
            var filterExpression = visitor.BuildPredicate(parser.query());

            return cards.Where(filterExpression);
        }
    }
}
