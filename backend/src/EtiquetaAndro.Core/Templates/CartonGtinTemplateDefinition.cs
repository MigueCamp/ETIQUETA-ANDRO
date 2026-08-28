using EtiquetaAndro.Core.Labels;

namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// The fixed CARTON &amp; SHIPPING GTIN-panel design: a bordered box with a
/// STYLE / COUNTRY / REVISION / MANUFACTURER header row, a big
/// COLOR CODE / COLOR DESCRIPTION row, then SIZE / QUANTITY / PO NUMBER /
/// CARTON rows down the left column, and a Code128 barcode.
/// </summary>
/// <remarks>
/// Coordinates were measured the same way as <see cref="HangtagTemplateDefinition"/>:
/// with PyMuPDF against the vector text/paths of the right-hand GTIN panel in
/// "STYLE# NE725 PO= 134876 CARTON &amp; SHIPPING STICKER LAYOUT FOR USA.pdf"
/// (the panel whose header row reads "STYLE | COUNTRY | REVISION |
/// MANUFACTURER" left to right), then converted from points to millimetres.
///
/// Two things in the reference PDF are proof-sheet artefacts rather than
/// real production data and are reproduced here as best judgement rather
/// than measurement:
/// <list type="bullet">
/// <item>The "GTIN" text between COUNTRY and MANUFACTURER is not a value —
/// it never has a number under it in the reference, and the real PRN
/// data has no matching field either. It is kept as a static label.</item>
/// <item>The PO NUMBER value position <i>is</i> measured from the reference
/// (which shows "134876" there for proofing) — but real PRNs never carry a
/// PO number in this template (see <see cref="CartonGtinLabelData.PoNumber"/>),
/// so it prints blank unless the caller supplies one.</item>
/// <item>The CARTON row's value position has no equivalent in the reference
/// PDF (it prints blank there too) even though real PRNs do carry a carton
/// number. Its position below is an estimate, placed in the same value
/// column as the other rows, evenly between the "CARTON" and "OF" labels.</item>
/// </list>
/// </remarks>
public static class CartonGtinTemplateDefinition
{
    public static readonly LabelTemplate Template = new()
    {
        WidthMm = 140.89,
        HeightMm = 88.05,

        Borders =
        [
            new RoundedBorder(X: 0, Y: 0, Width: 140.89, Height: 88.05, CornerRadiusMm: 0, StrokeWidthMm: 0.25),
        ],

        Rules =
        [
            new Rule(X: 39.62, Y: 0, Width: 0.25, Height: 88.05), // vertical divider: STYLE column vs. the rest
            new Rule(X: 82.38, Y: 0, Width: 0.25, Height: 13.29), // header row: COUNTRY vs. REVISION
            new Rule(X: 99.40, Y: 0, Width: 0.25, Height: 13.29), // header row: REVISION vs. MANUFACTURER
            new Rule(X: 0, Y: 13.29, Width: 140.89, Height: 0.25), // below header row
            new Rule(X: 0, Y: 25.48, Width: 140.89, Height: 0.25), // below color code/description row
            new Rule(X: 0, Y: 39.20, Width: 39.62, Height: 0.25), // below SIZE row (left column only)
            new Rule(X: 0, Y: 49.78, Width: 39.62, Height: 0.25), // below QUANTITY row (left column only)
            new Rule(X: 0, Y: 65.02, Width: 39.62, Height: 0.25), // below PO NUMBER row (left column only)
        ],

        // Label BaselineY values here are copied from
        // CartonGtinMirroredTemplateDefinition's own labels row for row
        // (STYLE/COUNTRY/MANUFACTURER, COLOR CODE/DESCRIPTION, SIZE,
        // QUANTITY, PO NUMBER, CARTON, OF) rather than kept at this panel's
        // own independently-measured values — the two panels were measured
        // off two separate physical panels in the reference PDF, and the
        // small (~1.3-2mm) per-row differences that came out of that made
        // otherwise-identical values sit at visibly different depths in
        // their row between the two panels. Sharing one set keeps every row
        // lined up the same way in both.
        StaticTexts =
        [
            new StaticText(X: 2.79, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "STYLE"),
            new StaticText(X: 44.77, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "COUNTRY"),
            new StaticText(X: 83.64, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "REVISION"),
            new StaticText(X: 100.77, BaselineY: 4.51, FontSizePt: 8.9, Weight: FontWeight.Regular, Text: "MANUFACTURER"),
            // Same size/baseline as its row's other values (see DynamicTexts
            // remarks below) so it reads at the same visual height as
            // COUNTRY/MANUFACTURER/STYLE rather than looking undersized.
            // MANUFACTURER's own X (below) is shifted right to make room —
            // at this size "GTIN" runs wider than its own REVISION column.
            new StaticText(X: 85.34, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold, Text: "GTIN"),
            new StaticText(X: 2.20, BaselineY: 18.06, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "COLOR CODE"),
            new StaticText(X: 43.31, BaselineY: 18.06, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "COLOR DESCRIPTION"),
            new StaticText(X: 2.12, BaselineY: 30.51, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "SIZE"),
            new StaticText(X: 2.12, BaselineY: 43.71, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "QUANTITY"),
            new StaticText(X: 32.17, BaselineY: 48.66, FontSizePt: 7.9, Weight: FontWeight.Regular, Text: "EA"),
            new StaticText(X: 2.21, BaselineY: 53.62, FontSizePt: 9.1, Weight: FontWeight.Regular, Text: "PO NUMBER"),
            new StaticText(X: 2.29, BaselineY: 73.43, FontSizePt: 9.4, Weight: FontWeight.Regular, Text: "CARTON"),
            new StaticText(X: 22.69, BaselineY: 83.63, FontSizePt: 7.9, Weight: FontWeight.Regular, Text: "OF"),
        ],

        // MaxWidthMm keeps each value from running into its neighbour — see
        // the DynamicText.MaxWidthMm remarks. It matters here specifically
        // because the system Arial Bold substituted for the reference
        // artwork's embedded font renders noticeably wider (measured ~15-22%
        // wider for the sample data) than the original design's metrics.
        //
        // Every row follows the same vertical rule so values sit in a
        // consistent spot relative to their own label instead of drifting
        // row to row: BaselineY = label's BaselineY + 1.0mm + that value's
        // own cap-height (FontSizePt * 0.3528mm/pt * 0.7 cap-height ratio).
        DynamicTexts =
        [
            new DynamicText(X: 5.84, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Style, MaxWidthMm: 31.78),
            new DynamicText(X: 40.89, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Country, MaxWidthMm: 42.45),
            // X shifted right from the column's own right-align formula (2mm
            // past where "GTIN" now ends at this size) and MaxWidthMm trimmed
            // to match, so an unusually long code still clears the right border.
            new DynamicText(X: 105.04, BaselineY: 9.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Manufacturer, MaxWidthMm: 32.82),
            new DynamicText(X: 22.86, BaselineY: 23.01, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).ColorCode, MaxWidthMm: 14.76),
            new DynamicText(X: 67.82, BaselineY: 23.01, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).ColorDescription, MaxWidthMm: 70.07),
            new DynamicText(X: 20.83, BaselineY: 35.46, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Size, MaxWidthMm: 16.79),
            new DynamicText(X: 22.44, BaselineY: 48.66, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Quantity, MaxWidthMm: 15.18),
            // Blank unless the caller supplied a PO number externally — see remarks.
            new DynamicText(X: 12.79, BaselineY: 58.57, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).PoNumber ?? string.Empty, MaxWidthMm: 24.83),
            new DynamicText(X: 12.79, BaselineY: 78.38, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).CartonNumber, MaxWidthMm: 24.83),
            // Sits a fixed 3.56mm above the barcode's own top edge (Y: 51.48 below), not tied to a label row.
            new DynamicText(X: 68.67, BaselineY: 47.92, FontSizePt: 16.0, Weight: FontWeight.Bold,
                ValueSelector: d => ((CartonGtinLabelData)d).Barcode, MaxWidthMm: 69.22),
        ],

        Barcodes =
        [
            new BarcodeArea(X: 56.98, Y: 51.48, Width: 73.24, Height: 21.59,
                ValueSelector: d => ((CartonGtinLabelData)d).Barcode),
        ],
    };
}
