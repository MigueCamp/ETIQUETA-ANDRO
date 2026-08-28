using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Sheets;

namespace EtiquetaAndro.Core.Tests;

public class SheetRendererTests
{
    private static HangtagLabelData Hangtag(string barcode) =>
        new(PoNumber: "134876", Style: "NE725", Color: "BURGUNDY HEATHER", Size: "S/P", Gender: "MEN'S", Barcode: barcode);

    [Fact]
    public void RenderPdf_ForAHangtagApprovalSheet_ProducesOnePagePerComposedSheet()
    {
        var sheets = HangtagSheetComposer.Compose([Hangtag("1"), Hangtag("2")]);
        // Both share the same (Style, Color) so they land on one composed sheet.
        Assert.Single(sheets);

        var pdfBytes = SheetRenderer.RenderPdf(sheets);

        Assert.True(pdfBytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);
    }

    [Fact]
    public void RenderPdf_ForACartonApprovalSheet_ProducesOnePagePerCarton()
    {
        var gtin = new CartonGtinLabelData(
            Style: "NE725", ColorCode: "53", ColorDescription: "BURGUNDY HEATHER", Quantity: "12",
            CartonNumber: "1", Size: "M", Manufacturer: "07822", Country: "BANGLADESH",
            Barcode: "1", PoNumber: "134876");
        var carrier = new CarrierLabelData(Barcode: "1", Carrier: "BY SEA", ShipToCompany: "S&S ACTIVEWEAR LLC");

        var composition = CartonSheetComposer.Compose([gtin], [carrier]);
        var pdfBytes = SheetRenderer.RenderPdf(composition.Sheets);

        Assert.True(pdfBytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);
    }

    [Fact]
    public void RenderSvg_ForAHangtagApprovalSheet_ProducesWellFormedMarkup()
    {
        var sheets = HangtagSheetComposer.Compose([Hangtag("1")]);

        var svg = SvgSheetRenderer.RenderSvg(sheets[0]);

        Assert.StartsWith("<svg", svg);
        Assert.EndsWith("</svg>", svg);
        Assert.Contains("<text", svg);
    }
}
