namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// A complete, fixed label design: physical size in millimetres plus every
/// static and dynamic element on it. This is the single source of truth a
/// renderer walks to produce either a print-ready PDF or an on-screen
/// preview — there is no other place layout/position decisions are made.
/// </summary>
public sealed class LabelTemplate
{
    public required double WidthMm { get; init; }
    public required double HeightMm { get; init; }
    public required IReadOnlyList<RoundedBorder> Borders { get; init; }
    public required IReadOnlyList<Rule> Rules { get; init; }
    public required IReadOnlyList<StaticText> StaticTexts { get; init; }
    public required IReadOnlyList<DynamicText> DynamicTexts { get; init; }
    public required IReadOnlyList<BarcodeArea> Barcodes { get; init; }
}
