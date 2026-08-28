using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Rendering;
using EtiquetaAndro.Core.Templates;

namespace EtiquetaAndro.Core.Tests;

public class CarrierPdfRenderingTests
{
    [Fact]
    public void RenderPdf_WithSampleShipToData_ProducesAValidPdf()
    {
        var label = new CarrierLabelData(
            Barcode: "00774582787153",
            Carrier: "BY SEA",
            ShipToCompany: "S&S ACTIVEWEAR LLC",
            ShipToStreet: "154 CAMPANELLI DRIVE",
            ShipToCityState: "MIDDLEBORO, MA, US",
            ShipToZip: "02346",
            ManufacturerCode: "07822",
            InternalCode: "RP");

        var pdfBytes = SkiaLabelRenderer.RenderPdf(CarrierTemplateDefinition.Template, [label]);

        Assert.True(pdfBytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);
    }

    [Fact]
    public void RenderPdf_WithNoShipToDataSupplied_StillRendersJustTheBarcode()
    {
        // Every field but the barcode is null until the caller supplies a
        // shipment header — rendering must not throw just because they're
        // missing (see CarrierLabelData remarks).
        var label = new CarrierLabelData(Barcode: "00774582787153");

        var pdfBytes = SkiaLabelRenderer.RenderPdf(CarrierTemplateDefinition.Template, [label]);

        Assert.True(pdfBytes.Length > 0);
    }
}
