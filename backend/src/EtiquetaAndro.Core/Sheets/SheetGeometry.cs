namespace EtiquetaAndro.Core.Sheets;

/// <summary>
/// The Brand/Style/Prod-Method/Dates table's own geometry — outer box, the
/// two internal dividers, and where each of the six cells' text baselines
/// sit. Measured with PyMuPDF against each reference PDF's letterhead table.
/// </summary>
public sealed record TableGeometry(
    double X, double Y, double Width, double Height,
    double ColDivider1X, double ColDivider2X, double RowDividerY,
    double Col1X, double Col2X, double Col3X,
    double Row1BaselineY, double Row2BaselineY,
    double FontSizePt);

/// <summary>
/// Everything about an approval sheet's chrome that is fixed per
/// <see cref="SheetKind"/> (as opposed to per-shipment data): the table, and
/// the "PO="/color line's position and font size.
/// </summary>
public sealed record SheetGeometry(
    TableGeometry Table,
    double PoX, double PoBaselineY, double PoFontSizePt,
    double ColorX, double ColorBaselineY, double ColorFontSizePt)
{
    /// <summary>
    /// Measured from "STYLE# NE725 PO= 134876 HANGTAG STICKER LAYOUT FOR
    /// USA.pdf" (907.09x783.08pt sheet), then shifted up 23.56mm — the
    /// reference reserves that much space above the table for the printer's
    /// own letterhead, which this sheet no longer draws (see
    /// <see cref="HangtagSheetComposer"/>'s matching shift for the grid and
    /// arrows below it) — leaving a plain 6mm top margin instead.
    /// </summary>
    private static readonly SheetGeometry HangtagGeometry = new(
        Table: new TableGeometry(
            X: 3.35, Y: 6.0, Width: 313.13, Height: 13.30,
            ColDivider1X: 103.4, ColDivider2X: 203.02, RowDividerY: 12.3,
            Col1X: 6.8, Col2X: 105.35, Col3X: 207.4,
            Row1BaselineY: 11.34, Row2BaselineY: 18.34,
            FontSizePt: 11),
        PoX: 16.44, PoBaselineY: 31.76, PoFontSizePt: 16,
        ColorX: 124.07, ColorBaselineY: 32.95, ColorFontSizePt: 14);

    /// <summary>
    /// Measured from "STYLE# NE725 PO= 134876 CARTON &amp; SHIPPING LAYOUT
    /// FOR USA.pdf" (936x756pt sheet), then shifted up 17.64mm — same
    /// letterhead-removal adjustment as <see cref="HangtagGeometry"/>, see
    /// <see cref="CartonSheetComposer"/> for the matching panel/arrow shift.
    /// </summary>
    private static readonly SheetGeometry CartonGeometry = new(
        Table: new TableGeometry(
            X: 4.77, Y: 6.0, Width: 321.2, Height: 10.87,
            ColDivider1X: 107.6, ColDivider2X: 209.8, RowDividerY: 11.16,
            Col1X: 8.3, Col2X: 114.2, Col3X: 214.12,
            Row1BaselineY: 10.86, Row2BaselineY: 16.16,
            FontSizePt: 9),
        PoX: 39.61, PoBaselineY: 28.64, PoFontSizePt: 16,
        ColorX: 141.48, ColorBaselineY: 26.46, ColorFontSizePt: 13);

    public static SheetGeometry For(SheetKind kind) => kind switch
    {
        SheetKind.Hangtag => HangtagGeometry,
        SheetKind.CartonAndShipping => CartonGeometry,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}

/// <summary>
/// The table/PO-line's exact static wording, which differs slightly in
/// casing/punctuation between the two reference PDFs (e.g. "RAL NO:" vs.
/// "Po No:", "COLOUR-" vs. "COLOR#").
/// </summary>
public sealed record SheetWording(
    string BrandLabel, string SecondaryLabel, string StyleLabel, string ProdMethodLabel, string ColorLinePrefix)
{
    private static readonly SheetWording Hangtag = new(
        BrandLabel: "BRAND:", SecondaryLabel: "RAL NO:", StyleLabel: "STYLE:",
        ProdMethodLabel: "PROD METHOD:", ColorLinePrefix: "COLOUR-");

    private static readonly SheetWording Carton = new(
        BrandLabel: "Brand:", SecondaryLabel: "Po No:", StyleLabel: "Style#",
        ProdMethodLabel: "Prod Method:", ColorLinePrefix: "COLOR#");

    public static SheetWording For(SheetKind kind) => kind switch
    {
        SheetKind.Hangtag => Hangtag,
        SheetKind.CartonAndShipping => Carton,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}
