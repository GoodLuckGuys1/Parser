using Newtonsoft.Json;

namespace OzonLive;

public class Atom
{
    [JsonProperty("type")] public string? Type { get; set; }

    [JsonProperty("price")] public Price? Price { get; set; }
    
    [JsonProperty("priceWithTitle")] public PriceWithTitle? PriceWithTitle { get; set; }

    [JsonProperty("labelList")] public LabelList? LabelList { get; set; }

    [JsonProperty("textAtom")] public TextAtom? TextAtom { get; set; }
}

public class Icon
{
    [JsonProperty("image")] public string? Image { get; set; }

    [JsonProperty("tintColor")] public string? TintColor { get; set; }
}

public class Item
{
    [JsonProperty("icon")] public Icon? Icon { get; set; }

    [JsonProperty("title")] public string? Title { get; set; }

    [JsonProperty("titleColor")] public string? TitleColor { get; set; }
}

public class LabelList
{
    [JsonProperty("items")] public List<Item>? Items { get; set; }

    [JsonProperty("textStyle")] public string? TextStyle { get; set; }

    [JsonProperty("maxLines")] public int? MaxLines { get; set; }

    [JsonProperty("align")] public string? Align { get; set; }

    [JsonProperty("testInfo")] public TestInfo? TestInfo { get; set; }
}

public class PriceWithTitle
{
    [JsonProperty("price")] public string? PriceItem { get; set; }

    [JsonProperty("priceColor")] public string? PriceColor { get; set; }

    [JsonProperty("originalPrice")] public string? OriginalPrice { get; set; }

    [JsonProperty("originalPriceColor")] public string? OriginalPriceColor { get; set; }

    [JsonProperty("theme")] public string? Theme { get; set; }

    [JsonProperty("strikethroughColor")] public string? StrikethroughColor { get; set; }
}

public class Price
{
    [JsonProperty("price")] public string? PriceItem { get; set; }

    [JsonProperty("priceColor")] public string? PriceColor { get; set; }

    [JsonProperty("originalPrice")] public string? OriginalPrice { get; set; }

    [JsonProperty("originalPriceColor")] public string? OriginalPriceColor { get; set; }

    [JsonProperty("theme")] public string? Theme { get; set; }

    [JsonProperty("strikethroughColor")] public string? StrikethroughColor { get; set; }
}

public class Root
{
    [JsonProperty("type")] public string? Type { get; set; }

    [JsonProperty("id")] public string? Id { get; set; }

    [JsonProperty("atom")] public Atom? Atom { get; set; }
}

public class TestInfo
{
    [JsonProperty("automatizationId")] public string? AutomatizationId { get; set; }
}

public class TextAtom
{
    [JsonProperty("text")] public string? Text { get; set; }

    [JsonProperty("textStyle")] public string? TextStyle { get; set; }

    [JsonProperty("maxLines")] public int? MaxLines { get; set; }

    [JsonProperty("testInfo")] public TestInfo? TestInfo { get; set; }
}