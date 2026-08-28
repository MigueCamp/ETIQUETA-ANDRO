using EtiquetaAndro.Core.Labels;

namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// The fixed HANGTAG STICKER design: 70mm x 38mm, four labelled fields
/// ("P/O:", "STYLE:", "COLOR:", "SIZE:"), a gender tag, and a Code128
/// barcode with its human-readable value printed above it.
/// </summary>
/// <remarks>
/// Every coordinate below was measured directly from the vector text and
/// path data in the reference file
/// "STYLE# NE725 PO= 134876 HANGTAG STICKER LAYOUT FOR USA.pdf" (first cell
/// of the 8-up proof sheet) using PyMuPDF, then converted from PDF points to
/// millimetres (1pt = 25.4/72 mm) — not eyeballed from the rendered image.
/// </remarks>
public static class HangtagTemplateDefinition
{
    public static readonly LabelTemplate Template = new()
    {
        WidthMm = 70,
        HeightMm = 38,

        Borders =
        [
            new RoundedBorder(X: 0, Y: 0, Width: 70, Height: 38, CornerRadiusMm: 1.95, StrokeWidthMm: 0.35),
        ],

        Rules =
        [
            new Rule(X: 3.89, Y: 5.67, Width: 7.20, Height: 0.34), // under "P/O:"
            new Rule(X: 3.89, Y: 10.41, Width: 12.61, Height: 0.34), // under "STYLE:"
            new Rule(X: 3.89, Y: 15.24, Width: 13.88, Height: 0.34), // under "COLOR:"
            new Rule(X: 3.89, Y: 20.74, Width: 8.97, Height: 0.34), // under "SIZE:"
        ],

        StaticTexts =
        [
            new StaticText(X: 3.89, BaselineY: 4.17, FontSizePt: 10.1, Weight: FontWeight.Bold, Text: "P/O:"),
            new StaticText(X: 3.89, BaselineY: 8.91, FontSizePt: 10.1, Weight: FontWeight.Bold, Text: "STYLE:"),
            new StaticText(X: 3.89, BaselineY: 13.99, FontSizePt: 10.1, Weight: FontWeight.Bold, Text: "COLOR:"),
            new StaticText(X: 3.89, BaselineY: 19.32, FontSizePt: 10.1, Weight: FontWeight.Bold, Text: "SIZE:"),
        ],

        DynamicTexts =
        [
            new DynamicText(X: 23.45, BaselineY: 3.86, FontSizePt: 10, Weight: FontWeight.Regular,
                ValueSelector: d => ((HangtagLabelData)d).PoNumber),
            new DynamicText(X: 23.45, BaselineY: 8.60, FontSizePt: 10, Weight: FontWeight.Regular,
                ValueSelector: d => ((HangtagLabelData)d).Style),
            new DynamicText(X: 23.45, BaselineY: 13.68, FontSizePt: 10, Weight: FontWeight.Regular,
                ValueSelector: d => ((HangtagLabelData)d).Color),
            new DynamicText(X: 23.45, BaselineY: 19.02, FontSizePt: 10, Weight: FontWeight.Regular,
                ValueSelector: d => ((HangtagLabelData)d).Size),
            new DynamicText(X: 53.25, BaselineY: 3.86, FontSizePt: 10, Weight: FontWeight.Regular,
                ValueSelector: d => ((HangtagLabelData)d).Gender),
            new DynamicText(X: 12.70, BaselineY: 24.18, FontSizePt: 10, Weight: FontWeight.Regular,
                ValueSelector: d => ((HangtagLabelData)d).Barcode),
        ],

        Barcodes =
        [
            new BarcodeArea(X: 3.89, Y: 25.82, Width: 44.78, Height: 8.21,
                ValueSelector: d => ((HangtagLabelData)d).Barcode),
        ],
    };
}
