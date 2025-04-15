namespace TCGCollect.Core
{
    public class Card
    {
        public string Id { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Layout { get; set; } = string.Empty;

        public CardFace[] Faces { get; set; } = Array.Empty<CardFace>();

        public string SetName { get; set; } = string.Empty;

        public string Rarity { get; set; } = string.Empty;

        public bool IsFoil { get; set; } = false;

        public string FoilType { get; set;} = string.Empty;
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
        public string SubTypes { get; set; } = string.Empty;
        public string PrintedSubTypes { get; set; } = string.Empty;
        public string PrintedType { get; set; } = string.Empty;
        public string[] CardColors { get; set; } = Array.Empty<string>();
    }

    public class OnePieceCardPart
    {
        public string[] AttackTypes { get; set; } = Array.Empty<string>();

        public string? Power { get; set; } = string.Empty;

        public string? Counter { get; set; } = string.Empty;
    }
}
