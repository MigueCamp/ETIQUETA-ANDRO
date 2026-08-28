using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Templates;

namespace EtiquetaAndro.Core.Sheets;

/// <summary>Result of matching GTIN and Carrier data into approval sheets.</summary>
public sealed record CartonSheetComposition(IReadOnlyList<ComposedSheet> Sheets, int UnmatchedGtinCount);

/// <summary>
/// Builds one CARTON &amp; SHIPPING approval sheet per physical carton — the
/// mirrored GTIN panel pair plus the carrier panel below, matched by
/// barcode, exactly as the reference artwork lays out one page per size.
/// </summary>
public static class CartonSheetComposer
{
    private const double SheetWidthMm = 330.2;

    // See HangtagSheetComposer's matching constant — the reference canvas
    // reserves this much space above the table for the printer's own
    // letterhead, which this sheet no longer draws; the page is shortened
    // by the same amount and everything below it shifted up to match
    // SheetGeometry.CartonGeometry's own shift.
    private const double RemovedLetterheadShiftMm = 17.64;
    private const double SheetHeightMm = 266.7 - RemovedLetterheadShiftMm;

    private const double LeftGtinOffsetX = 26.35;
    private const double RightGtinOffsetX = 179.34;
    private const double GtinOffsetY = 57.82 - RemovedLetterheadShiftMm;

    private const double CarrierOffsetX = 108.42;
    private const double CarrierOffsetY = 164.34 - RemovedLetterheadShiftMm;

    public static CartonSheetComposition Compose(
        IReadOnlyList<CartonGtinLabelData> gtinLabels,
        IReadOnlyList<CarrierLabelData> carrierLabels)
    {
        var carrierByBarcode = carrierLabels
            .GroupBy(c => c.Barcode)
            .ToDictionary(g => g.Key, g => g.First());

        var sheets = new List<ComposedSheet>();
        var unmatched = 0;

        foreach (var gtin in gtinLabels)
        {
            if (!carrierByBarcode.TryGetValue(gtin.Barcode, out var carrier))
            {
                unmatched++;
                continue;
            }

            sheets.Add(ComposeSheet(gtin, carrier));
        }

        return new CartonSheetComposition(sheets, unmatched);
    }

    private static ComposedSheet ComposeSheet(CartonGtinLabelData gtin, CarrierLabelData carrier)
    {
        var panels = new List<PanelPlacement>
        {
            new(CartonGtinMirroredTemplateDefinition.Template, gtin, LeftGtinOffsetX, GtinOffsetY),
            new(CartonGtinTemplateDefinition.Template, gtin, RightGtinOffsetX, GtinOffsetY),
            new(CarrierTemplateDefinition.Template, carrier, CarrierOffsetX, CarrierOffsetY),
        };

        var s = RemovedLetterheadShiftMm;
        var arrows = new List<DimensionArrow>
        {
            new(30.91, 52.57 - s, 311.04, 52.57 - s, "12\""),
            new(21.66, 61.57 - s, 21.66, 141.46 - s, "4\""),
            // The reference's own label for this specific arrow could not be
            // recovered from the PDF's text layer (extraction gap, not a
            // measurement) — "6\"" is inferred from the panel's own ~5.5in
            // width rounding to the nearest common nominal label size, the
            // same way the other three arrows round their actual mm span.
            new(114.66, 158.0 - s, 244.3, 158.0 - s, "6\""),
            new(102.2, 168.81 - s, 102.2, 248.7 - s, "4\""),
        };

        return new ComposedSheet(
            Kind: SheetKind.CartonAndShipping,
            WidthMm: SheetWidthMm,
            HeightMm: SheetHeightMm,
            PoNumber: gtin.PoNumber ?? string.Empty,
            ColorValue: gtin.ColorDescription,
            Table: new ApprovalTableData(StyleValue: gtin.Style, ProdMethodValue: "CARTON & SHIPPING"),
            Panels: panels,
            Arrows: arrows);
    }
}
