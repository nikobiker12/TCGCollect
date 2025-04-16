using System.Text.Json;
using TCGCollect.Core;
using TCGCollect.DataStore;

namespace TCGCollect.UnitTests
{
    public class CardSearchUnitTests
    {
        [Fact]
        public void g_colon_filter()
        {
            string filter = "g:OP";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(4, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Equal("OP", card.Game));
        }

        [Fact]
        public void game_colon_filter()
        {
            string filter = "game:OP";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(4, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Equal("OP", card.Game));
        }

        [Fact]
        public void r_colon_filter()
        {
            string filter = "r:Common";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(4, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Equal("Common", card.Rarity));
        }

        [Fact]
        public void rarity_colon_filter()
        {
            string filter = "rarity:Common";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(4, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Equal("Common", card.Rarity));
        }

        [Fact]
        public void s_colon_filter()
        {
            string filter = "s:HNT";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(2, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Equal("HNT", card.SetName));
        }

        [Fact]
        public void set_colon_filter()
        {
            string filter = "set:HNT";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(2, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Equal("HNT", card.SetName));
        }

        [Fact]
        public void t_colon_filter()
        {
            string filter = "t:Creature";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.All(filteredCards, card => CardPartAssert.AnyParts(card, p => p.Type == "Creature"));
        }

        [Fact]
        public void type_colon_filter()
        {
            string filter = "type:Creature";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.All(filteredCards, card => CardPartAssert.AnyParts(card, p => p.Type == "Creature"));
        }

        [Fact]
        public void n_colon_filter()
        {
            string filter = "n:Part1";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(2, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name.Contains("Part1")));
        }

        [Fact]
        public void name_colon_filter()
        {
            string filter = "name:Part1";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(2, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name == "Part1"));
        }


        [Fact]
        public void n_colon_filter_Then_NotFound()
        {
            string filter = "n:PartUnk";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.Empty(filteredCards);
        }
 
        [Fact]
        public void name_like_filter()
        {
            string filter = "name~NameA";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(2, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name.Contains("NameA")));
        }

        [Fact]
        public void lower_case_name_like_filter()
        {
            string filter = "name~name";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Equal(4, filteredCards.Length);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name.Contains("Name")));
        }

        [Fact]
        public void lower_case_name_equal_filter()
        {
            string filter = "name=namec";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Single(filteredCards);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name == "NameC"));
        }

        [Fact]
        public void and_conjonction_set_name_filter()
        {
            string filter = "set:OP10 AND name=namec";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Single(filteredCards);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name == "NameC"));
        }

        [Fact]
        public void implicit_conjonction_set_name_filter()
        {
            string filter = "set:OP10 name:namec";

            var filteredCards = LuceneCardSearchHelper.Filter(GetCards().AsQueryable(), filter).ToArray();
            Assert.NotEmpty(filteredCards);
            Assert.Single(filteredCards);
            Assert.All(filteredCards, card => Assert.Contains(card.Faces.SelectMany(f => f.Parts), p => p.Name == "NameC"));
        }

        [Fact]
        public void SerializeTest()
        {
            // Arrange
            var cards = GetCards();
            var directoryPath = @"c:\tcgcollect-data";
            var filePath = Path.Combine(directoryPath, "cards.json");

            // Ensure directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Serialize cards to JSON
            var json = JsonSerializer.Serialize(cards, new JsonSerializerOptions { WriteIndented = true });

            // Write JSON to file
            File.WriteAllText(filePath, json);

            // Assert file exists
            Assert.True(File.Exists(filePath));
        }

        public List<Card> GetCards()
        {
            return new List<Card>()
            {
               new Card
               {
                   Id = "1",
                   Game = "FAB",
                   ReferenceId = "HNT-104",
                   Language = "English",
                   SetName = "HNT",
                   Number = "104",
                   Rarity = "Common",
                   IsFoil = false,
                   FoilType = null,
                   Faces = new[]
                   {
                       new CardFace
                       {
                           Layout = "Normal",
                           Parts = new[]
                           {
                               new CardPart
                               {
                                   Name = "Part1",
                                   PrintedName = "Printed Part1",
                                   Text = "Some text for Part1",
                                   PrintedText = "Printed text for Part1",
                                   Type = "Monster",
                                   PrintedType = "Printed Monster",
                                   SubTypes = "Subtype1",
                                   PrintedSubTypes = "Printed Subtype1",
                                   CardColors = new[] { "Red", "Blue" },
                                   Cost = 3,
                                   Artist = "Artist1",
                                   ImageSrc = "Image1.png"
                               }
                           }
                       }
                   }
               },
               new Card
               {
                   Id = "2",
                   ReferenceId = "HNT-106",
                   Language = "English",
                   Game = "FAB",
                   SetName = "HNT",
                   Number = "106",
                   Rarity = "Common",
                   IsFoil = false,
                   FoilType = null,
                   Faces = new[]
                   {
                       new CardFace
                       {
                           Layout = "Normal",
                           Parts = new[]
                           {
                               new CardPart
                               {
                                   Name = "Part1",
                                   PrintedName = "Printed Part1",
                                   Text = "Some text for Part1",
                                   PrintedText = "Printed text for Part1",
                                   Type = "Monster",
                                   PrintedType = "Printed Monster",
                                   SubTypes = "Subtype1",
                                   PrintedSubTypes = "Printed Subtype1",
                                   CardColors = new[] { "Red", "Blue" },
                                   Cost = 3,
                                   Artist = "Artist1",
                                   ImageSrc = "Image1.png"
                               }
                           }
                       }
                   }
               },
               new Card
               {
                   Id = "2",
                   ReferenceId = "OP10-005",       
                   Game = "OP",
                   Language = "English",
                   SetName = "OP10",
                   Rarity = "Rare",
                   IsFoil = true,
                   FoilType = "Holo",
                   Faces = new[]
                   {
                       new CardFace
                       {
                           Layout = "Split",
                           Parts = new[]
                           {
                               new CardPart
                               {
                                   Name = "NameA",
                                   PrintedName = "Printed PartA",
                                   Text = "Some text for PartA",
                                   PrintedText = "Printed text for PartA",
                                   Type = "Warrior",
                                   PrintedType = "Printed Warrior",
                                   SubTypes = "SubtypeA",
                                   PrintedSubTypes = "Printed SubtypeA",
                                   CardColors = new[] { "Black" },
                                   Cost = 4,
                                   Artist = "ArtistA",
                                   ImageSrc = "ImageA.png"
                               },
                               new CardPart
                               {
                                   Name = "NameB",
                                   PrintedName = "Printed PartB",
                                   Text = "Some text for PartB",
                                   PrintedText = "Printed text for PartB",
                                   Type = "Artifact",
                                   PrintedType = "Printed Artifact",
                                   SubTypes = "SubtypeB",
                                   PrintedSubTypes = "Printed SubtypeB",
                                   CardColors = new[] { "White" },
                                   Cost = 1,
                                   Artist = "ArtistB",
                                   ImageSrc = "ImageB.png"
                               }
                           }
                       }
                   }
               },
               new Card
               {
                   Id = "2",
                   ReferenceId = "OP10-005",
                   Game = "OP",
                   Language = "English",
                   SetName = "OP10",
                   Rarity = "Rare",
                   IsFoil = true,
                   FoilType = "Holo",
                   Faces = new[]
                   {
                       new CardFace
                       {
                           Layout = "Split",
                           Parts = new[]
                           {
                               new CardPart
                               {
                                   Name = "NameAA",
                                   PrintedName = "Printed PartA",
                                   Text = "Some text for PartA",
                                   PrintedText = "Printed text for PartA",
                                   Type = "Warrior",
                                   PrintedType = "Printed Warrior",
                                   SubTypes = "SubtypeA",
                                   PrintedSubTypes = "Printed SubtypeA",
                                   CardColors = new[] { "Black" },
                                   Cost = 4,
                                   Artist = "ArtistA",
                                   ImageSrc = "ImageA.png"
                               },
                               new CardPart
                               {
                                   Name = "NameAB",
                                   PrintedName = "Printed PartB",
                                   Text = "Some text for PartB",
                                   PrintedText = "Printed text for PartB",
                                   Type = "Artifact",
                                   PrintedType = "Printed Artifact",
                                   SubTypes = "SubtypeB",
                                   PrintedSubTypes = "Printed SubtypeB",
                                   CardColors = new[] { "White" },
                                   Cost = 1,
                                   Artist = "ArtistB",
                                   ImageSrc = "ImageB.png"
                               }
                           }
                       }
                   }
               },
               new Card
               {
                   Id = "5",
                   Game = "OP",
                   ReferenceId = "OP10-105",
                   Language = "English",
                   SetName = "OP10",
                   Number = "104",
                   Rarity = "Common",
                   IsFoil = false,
                   FoilType = null,
                   Faces = new[]
                   {
                       new CardFace
                       {
                           Layout = "Normal",
                           Parts = new[]
                           {
                               new CardPart
                               {
                                   Name = "NameC",
                                   PrintedName = "Printed Part1",
                                   Text = "Some text for Part1",
                                   PrintedText = "Printed text for Part1",
                                   Type = "Monster",
                                   PrintedType = "Printed Monster",
                                   SubTypes = "Subtype1",
                                   PrintedSubTypes = "Printed Subtype1",
                                   CardColors = new[] { "Red", "Blue" },
                                   Cost = 3,
                                   Artist = "Artist1",
                                   ImageSrc = "Image1.png"
                               }
                           }
                       }
                   }
               },
               new Card
               {
                   Id = "6",
                   Game = "OP",
                   ReferenceId = "OP10-105",
                   Language = "English",
                   SetName = "OP10",
                   Number = "104",
                   Rarity = "Common",
                   IsFoil = false,
                   FoilType = null,
                   Faces = new[]
                   {
                       new CardFace
                       {
                           Layout = "Normal",
                           Parts = new[]
                           {
                               new CardPart
                               {
                                   Name = "NameD",
                                   PrintedName = "Printed Part1",
                                   Text = "Some text for Part1",
                                   PrintedText = "Printed text for Part1",
                                   Type = "Creature",
                                   PrintedType = "Printed Monster",
                                   SubTypes = "Subtype1",
                                   PrintedSubTypes = "Printed Subtype1",
                                   CardColors = new[] { "Red", "Blue" },
                                   Cost = 3,
                                   Artist = "Artist1",
                                   ImageSrc = "Image1.png"
                               }
                           }
                       }
                   }
               },

          };
        }
    }

    public static class CardPartAssert
    {
        public static void AnyParts(Card card, Predicate<CardPart> filter)
        {
            Assert.Contains(card.Faces.SelectMany(f => f.Parts), filter);
        }
    }
}