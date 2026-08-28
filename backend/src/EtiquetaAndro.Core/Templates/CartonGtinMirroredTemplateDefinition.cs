using EtiquetaAndro.Core.Labels;

namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// The CARTON &amp; SHIPPING GTIN panel's *other* fixed layout: the reference
/// artwork prints two copies of this panel side by side on one carton (one
/// for each side of the box), and the second one is not a simple left-right
/// flip of the first — it is an independently laid out mirror image where
/// every column is reordered (MANUFACTURER/REVISION/COUNTRY/STYLE instead of
/// STYLE/COUNTRY/REVISION/MANUFACTURER) but the text itself still reads
/// normally, left to right.
/// </summary>
/// <remarks>
/// Coordinates were measured the same way as <see cref="CartonGtinTemplateDefinition"/>,
/// against the *left-hand* panel in
/// "STYLE# NE725 PO= 134876 CARTON &amp; SHIPPING STICKER LAYOUT FOR USA.pdf"
/// (outer box 74.7,163.9 to 468.0,414.0pt), converted to millimetres and
/// re-based to that panel's own top-left corner. Only used when composing
/// the approval sheet (see <see cref="Sheets.CartonSheetComposer"/>) — the
/// standalone per-carton production PDF only ever uses the one canonical
/// <see cref="CartonGtinTemplateDefinition"/> layout, per the existing
/// project decision to keep that output design-fixed regardless of which
/// raw PRN layout variant produced the data.
/// </remarks>
public static class CartonGtinMirroredTemplateDefinition
{
    public static readonly LabelTemplate Template = new()
    {
        WidthMm = 138.75,
        HeightMm = 88.22,

        Borders =
        [
            new RoundedBorder(X: 0, Y: 0, Width: 138.75, Height: 88.22, CornerRadiusMm: 0, StrokeWidthMm: 0.25),
        ],

        Rules =
        [
            new Rule(X: 98.15, Y: 0, Width: 0.25, Height: 88.22), // vertical divider: rest vs. STYLE column
            new Rule(X: 35.88, Y: 0, Width: 0.25, Height: 13.5), // header row: MANUFACTURER vs. REVISION
            new Rule(X: 53.33, Y: 0, Width: 0.25, Height: 13.5), // header row: REVISION vs. COUNTRY
            new Rule(X: 0, Y: 13.5, Width: 138.75, Height: 0.25), // below header row
            new Rule(X: 0, Y: 25.7, Width: 138.75, Height: 0.25), // below color code/description row
            new Rule(X: 98.15, Y: 39.4, Width: 40.6, Height: 0.25), // below SIZE row (right column only)
            new Rule(X: 98.15, Y: 49.3, Width: 40.6, Height: 0.25), // below QUANTITY row (right column only)
            new Rule(X: 98.15, Y: 65.15, Width: 40.6, Height: 0.25), // below PO NUMBER row (right column only)
        ],

        StaticTexts =
        [
            new StaticText(X: 1.26, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "MANUFACTURER"),
            new StaticText(X: 37.91, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "REVISION"),
            new StaticText(X: 55.90, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "COUNTRY"),
            new StaticText(X: 104.14, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "STYLE"),
            // Same size/baseline as the row's other values — see the
            // canonical panel's GTIN remarks. COUNTRY's own X (below) is
            // shifted right to make room, same as MANUFACTURER there.
            new StaticText(X: 38.42, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold, Text: "GTIN"),
            new StaticText(X: 2.61, BaselineY: 18.06, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "COLOR DESCRIPTION"),
            new StaticText(X: 105.62, BaselineY: 18.06, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "COLOR CODE"),
            new StaticText(X: 99.90, BaselineY: 30.51, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "SIZE"),
            new StaticText(X: 99.64, BaselineY: 43.71, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "QUANTITY"),
            new StaticText(X: 131.05, BaselineY: 48.66, FontSizePt: 7.9, Weight: FontWeight.Regular, Text: "EA"),
            new StaticText(X: 99.90, BaselineY: 53.62, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "PO NUMBER"),
            new StaticText(X: 99.72, BaselineY: 73.43, FontSizePt: 9.4, Weight: FontWeight.Regular, Text: "CARTON"),
            new StaticText(X: 122.58, BaselineY: 83.63, FontSizePt: 7.9, Weight: FontWeight.Regular, Text: "OF"),
        ],

        // Same vertical rule as the canonical (non-mirrored) panel — see
        // CartonGtinTemplateDefinition.DynamicTexts remarks — so both panels
        // in a sheet line up row for row instead of drifting independently:
        // BaselineY = label's BaselineY + 1.0mm + that value's cap-height.
        DynamicTexts =
        [
            new DynamicText(X: 104.80, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Style, MaxWidthMm: 31.78),
            // X shifted right 2mm past where "GTIN" now ends at this size —
            // see the canonical panel's MANUFACTURER remarks.
            new DynamicText(X: 58.12, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Country, MaxWidthMm: 42.45),
            new DynamicText(X: 1.85, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Manufacturer, MaxWidthMm: 35.53),
            new DynamicText(X: 120.46, BaselineY: 23.01, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).ColorCode, MaxWidthMm: 14.76),
            new DynamicText(X: 23.27, BaselineY: 23.01, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).ColorDescription, MaxWidthMm: 70.07),
            new DynamicText(X: 119.20, BaselineY: 35.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Size, MaxWidthMm: 16.79),
            new DynamicText(X: 120.89, BaselineY: 48.66, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Quantity, MaxWidthMm: 15.18),
            new DynamicText(X: 110.64, BaselineY: 58.57, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).PoNumber ?? string.Empty, MaxWidthMm: 24.83),
            new DynamicText(X: 110.64, BaselineY: 78.38, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).CartonNumber, MaxWidthMm: 24.83),
            // Sits a fixed 3.56mm above the barcode's own top edge (Y: 51.73 below), matching the canonical panel's gap.
            new DynamicText(X: 25.81, BaselineY: 48.17, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Barcode, MaxWidthMm: 69.22),
        ],

        Barcodes =
        [
            new BarcodeArea(X: 8.53, Y: 51.73, Width: 73.24, Height: 21.59,
                ValueSelector: d => ((CartonGtinLabelData)d).Barcode),
        ],
    };
}
