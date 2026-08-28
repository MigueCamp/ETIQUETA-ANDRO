using EtiquetaAndro.Core.Labels;

namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// The fixed CARTON &amp; SHIPPING carrier/ship-to panel design: a bordered
/// box with CARRIER, FROM, TO and INTERNAL USE fields — every piece of text
/// on it is rotated -90° from how a normal page reads.
/// </summary>
/// <remarks>
/// Coordinates were measured the same way as the other two templates: with
/// PyMuPDF against the vector text/paths of the bottom panel in
/// "STYLE# NE725 PO= 134876 CARTON &amp; SHIPPING STICKER LAYOUT FOR USA.pdf",
/// then converted from points to millimetres.
///
/// Every element here uses <c>RotationDegrees: -90</c>. Each rotated text
/// line's anchor is the bottom-right (max-X, max-Y) corner of its measured
/// bounding box: text is drawn as if horizontal starting at that point, then
/// rotated -90° about it. Under that rotation, the text's own width ends up
/// extending in -Y (so it reads bottom-to-top, matching the reference) and
/// its ascenders end up extending in -X — using the bbox's *left* edge as
/// the anchor instead (an earlier mistake here) left no room for ascenders
/// and clipped them off the edge of the label. The barcode uses its bbox's
/// top-left (min-X, min-Y) corner instead, since it has no baseline/ascender
/// asymmetry to account for. Multi-word lines (e.g. the ship-to
/// company/street/city lines) were reassembled from PyMuPDF's per-word
/// boxes, which the reference exports as several words sharing one X (one
/// rotated line) at different Y — ordered by descending Y to get natural
/// reading order.
/// </remarks>
public static class CarrierTemplateDefinition
{
    private const double Rotation = -90;

    public static readonly LabelTemplate Template = new()
    {
        WidthMm = 140.05,
        HeightMm = 91.41,

        Borders =
        [
            new RoundedBorder(X: 0, Y: 0, Width: 140.05, Height: 91.41, CornerRadiusMm: 0, StrokeWidthMm: 0.25),
        ],

        // Measured the same way as the borders/text above (PyMuPDF against
        // the reference PDF, relative to this panel's own top-left corner)
        // — this panel had none of its own grid lines implemented until
        // they were found to be missing from the rendered approval sheet.
        Rules =
        [
            new Rule(X: 15.50, Y: 0, Width: 0.25, Height: 91.44), // INTERNAL vs. CARRIER/FROM column
            new Rule(X: 44.80, Y: 0, Width: 0.25, Height: 91.44), // CARRIER/FROM vs. TO/INTERNAL USE column
            new Rule(X: 78.35, Y: 0, Width: 0.25, Height: 91.44), // TO/INTERNAL USE vs. 2nd INTERNAL USE column
            new Rule(X: 91.90, Y: 0, Width: 0.25, Height: 91.44), // thin decorative double-divider before the barcode
            new Rule(X: 95.95, Y: 0, Width: 0.25, Height: 91.44),
            new Rule(X: 15.50, Y: 38.30, Width: 62.85, Height: 0.25), // CARRIER row vs. TO row
        ],

        StaticTexts =
        [
            new StaticText(X: 6.73, BaselineY: 89.92, FontSizePt: 12.3, Weight: FontWeight.Bold, Text: "INTERNAL", RotationDegrees: Rotation),
            new StaticText(X: 21.38, BaselineY: 34.37, FontSizePt: 12.5, Weight: FontWeight.Bold, Text: "CARRIER", RotationDegrees: Rotation),
            new StaticText(X: 21.38, BaselineY: 89.92, FontSizePt: 12.5, Weight: FontWeight.Bold, Text: "FROM", RotationDegrees: Rotation),
            new StaticText(X: 52.15, BaselineY: 35.81, FontSizePt: 11.6, Weight: FontWeight.Bold, Text: "INTERNAL USE", RotationDegrees: Rotation),
            new StaticText(X: 51.39, BaselineY: 89.41, FontSizePt: 11.9, Weight: FontWeight.Bold, Text: "TO", RotationDegrees: Rotation),
            new StaticText(X: 84.37, BaselineY: 88.73, FontSizePt: 12.5, Weight: FontWeight.Bold, Text: "INTERNAL USE", RotationDegrees: Rotation),
        ],

        // MaxWidthMm here means the same thing it does for the other two
        // templates, just along a rotated axis: for -90°-rotated text the
        // run direction (the axis the text grows along, and the one
        // textLength/ScaleX compress) is -Y from the anchor, not -X. Each
        // budget below is that anchor's distance up to the nearest row
        // boundary — either the label's own top edge (Y=0) for the top
        // "CARRIER" row, or the horizontal divider between the two rows
        // (measured at ~38.2mm from the top in the reference PDF) for the
        // bottom "TO" row — minus a small safety margin. Without this,
        // system-substituted Arial Bold overflows past the label edge for
        // realistic values (e.g. "EXPEDITED AIR FREIGHT" as carrier, or a
        // long ship-to company name), since none of these fields have a
        // length fixed by the design the way the hangtag/GTIN panels do.
        DynamicTexts =
        [
            new DynamicText(X: 39.50, BaselineY: 35.73, FontSizePt: 27.5, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).Carrier ?? string.Empty, MaxWidthMm: 33, RotationDegrees: Rotation),
            new DynamicText(X: 39.62, BaselineY: 77.98, FontSizePt: 28.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).ManufacturerCode ?? string.Empty, MaxWidthMm: 36, RotationDegrees: Rotation),
            new DynamicText(X: 69.45, BaselineY: 28.02, FontSizePt: 29.9, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).InternalCode ?? string.Empty, MaxWidthMm: 26, RotationDegrees: Rotation),
            new DynamicText(X: 57.66, BaselineY: 89.41, FontSizePt: 12.5, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).ShipToCompany ?? string.Empty, MaxWidthMm: 47, RotationDegrees: Rotation),
            new DynamicText(X: 62.91, BaselineY: 89.41, FontSizePt: 11.5, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).ShipToStreet ?? string.Empty, MaxWidthMm: 47, RotationDegrees: Rotation),
            new DynamicText(X: 69.26, BaselineY: 89.41, FontSizePt: 11.9, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).ShipToCityState ?? string.Empty, MaxWidthMm: 47, RotationDegrees: Rotation),
            // Narrower than the other ship-to fields on purpose: a 5-digit
            // ZIP (or ZIP+4) is much shorter than a street/company line, and
            // reusing the wider budget just stretched it to fill the space.
            new DynamicText(X: 76.20, BaselineY: 89.41, FontSizePt: 13.2, Weight: FontWeight.Bold,
                ValueSelector: d => ((CarrierLabelData)d).ShipToZip ?? string.Empty, MaxWidthMm: 26, RotationDegrees: Rotation),
            // X sits 3.56mm clear of the barcode's own left edge (X: 110.37
            // below) — the same gap the two GTIN panels use between their
            // barcode number and their barcode, rather than the near-zero
            // clearance a literal reference measurement gave here.
            // MaxWidthMm is a safety net (this is the only barcode-adjacent
            // text in the three templates that lacked one) — the anchor sits
            // 64.60mm from the panel's top edge, so an unusually long barcode
            // value could otherwise run off the label with nothing to catch it.
            new DynamicText(X: 106.81, BaselineY: 64.60, FontSizePt: 13.9, Weight: FontWeight.Regular,
                ValueSelector: d => ((CarrierLabelData)d).Barcode, MaxWidthMm: 55, RotationDegrees: Rotation),
        ],

        Barcodes =
        [
            // Width/Height are the barcode's own (unrotated) length and bar
            // height — see BarcodeArea.RotationDegrees remarks.
            new BarcodeArea(X: 110.37, Y: 69.54, Width: 55.79, Height: 22.86,
                ValueSelector: d => ((CarrierLabelData)d).Barcode, RotationDegrees: Rotation),
        ],
    };
}
