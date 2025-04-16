namespace TCGCollect.Core
{
    public class Card
    {
        public string Id { get; set; } = string.Empty;

        public string Game { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty; // e.g. "card", "token", "emblem", "art", "don", ...

        public string ReferenceId { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;
 
        public string SetName { get; set; } = string.Empty;

        public string Rarity { get; set; } = string.Empty;

        public bool IsFoil { get; set; } = false;

        public string? FoilType { get; set; }

        public string? Number { get; set; }

        public CardFace[] Faces { get; set; } = Array.Empty<CardFace>();
    }

    public class CardFace
    {
        public CardPart[] Parts { get; set; } = Array.Empty<CardPart>();

        public string Layout { get; set; } = string.Empty;
    }

    public class CardPart
    {
        public string Name { get; set; } = string.Empty;
        public string PrintedName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string PrintedText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PrintedType { get; set; } = string.Empty;
        public string SubTypes { get; set; } = string.Empty;
        public string PrintedSubTypes { get; set; } = string.Empty;
        public string[] CardColors { get; set; } = Array.Empty<string>();
        public int? Cost { get; set; } = null;

        #region Illustration Specific
        /// <summary>
        /// The name of the artist who illustrated the card.
        /// </summary>
        public string Artist { get; set; } = string.Empty;

        /// <summary>
        /// The name of the set the card belongs to.
        /// </summary>
        public string ImageSrc { get; set; } = string.Empty;
        #endregion
    }

    public class OnePieceCardPart : CardPart
    {
        public string AttackTypes { get; set; } = string.Empty;
        public string? Power { get; set; } = string.Empty;
        public string? Counter { get; set; } = string.Empty;
    }

    public class StarWarsUnlimitedCardPart : CardPart
    {
        public int? Attack { get; set; } = null;
        public int? Defense { get; set; } = null;
    }
}
