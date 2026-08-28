using EtiquetaAndro.Core.Templates;

namespace EtiquetaAndro.Core.Sheets;

/// <summary>
/// Which reference artwork a sheet is based on — the two references differ
/// in the table's exact static wording/casing ("STYLE:"/"RAL NO:" vs.
/// "Style#"/"Po No:"), which the renderer picks per kind rather than the
/// caller having to repeat it.
/// </summary>
public enum SheetKind
{
    Hangtag,
    CartonAndShipping,
}

/// <summary>
/// The two cells of the approval-form table that actually vary — everything
/// else in the table (Brand/RAL or Po No/dates) is always blank, per the
/// reference artwork, and baked directly into the sheet renderer.
/// </summary>
public sealed record ApprovalTableData(string StyleValue, string ProdMethodValue);

/// <summary>
/// A dimension-callout arrow: a shaft with a triangular arrowhead at each
/// end and a centered mm/inch label, matching the reference artwork's
/// "70 MM" / "38 MM" style callouts. Horizontal vs. vertical is inferred
/// from which delta between the two points is larger.
/// </summary>
public sealed record DimensionArrow(double X0, double Y0, double X1, double Y1, string Label);

/// <summary>
/// One already-designed <see cref="LabelTemplate"/> instance placed at a
/// fixed offset on a bigger sheet — e.g. the two GTIN panels or the single
/// carrier panel on a CARTON &amp; SHIPPING approval sheet. Reuses the
/// existing template/data/renderer machinery unchanged; the sheet renderer
/// just translates the canvas before drawing it.
/// </summary>
public sealed record PanelPlacement(LabelTemplate Template, object Data, double OffsetXMm, double OffsetYMm);

/// <summary>
/// One page of an approval sheet: the letterhead, the Brand/Style/Prod
/// Method/Dates table, the "PO="/color line, one or more dimension arrows,
/// and the actual label artwork placed at fixed offsets.
/// </summary>
public sealed record ComposedSheet(
    SheetKind Kind,
    double WidthMm,
    double HeightMm,
    string PoNumber,
    string ColorValue,
    ApprovalTableData Table,
    IReadOnlyList<PanelPlacement> Panels,
    IReadOnlyList<DimensionArrow> Arrows);
