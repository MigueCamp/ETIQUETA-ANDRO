using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Rendering;
using EtiquetaAndro.Core.Templates;
using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Tests;

public class SvgLabelRendererTests
{
    [Fact]
    public void RenderSvg_ForHangtag_ProducesAWellFormedSvgWithRealData()
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "hangtag-bolsa-plastico.prn"));
        var label = HangtagLabelMapper.Map(ZplParser.ParseLabels(zpl)[0]);

        var svg = SvgLabelRenderer.RenderSvg(HangtagTemplateDefinition.Template, label);

        Assert.StartsWith("<svg", svg);
        Assert.EndsWith("</svg>", svg);
        Assert.Contains("width=\"70mm\"", svg);
        Assert.Contains("height=\"38mm\"", svg);
        Assert.Contains(">134857<", svg); // PO value actually made it into the markup
        Assert.Contains(">BLACK<", svg);
        Assert.Contains("<rect", svg); // at least the border/barcode bars
    }

    [Fact]
    public void RenderSvg_ForCartonGtin_EscapesTextAndOmitsNullPoNumber()
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "carton-gtin-derecha.prn"));
        var label = CartonGtinLabelMapper.Map(ZplParser.ParseLabels(zpl)[0]); // no PO number supplied

        var svg = SvgLabelRenderer.RenderSvg(CartonGtinTemplateDefinition.Template, label);

        Assert.Contains(">WHITE<", svg);
        Assert.DoesNotContain("null", svg);
    }

    [Fact]
    public void RenderSvg_ForCarrierPanel_RotatesTextAndOmitsMissingFields()
    {
        var label = new CarrierLabelData(Barcode: "00774582787153", Carrier: "BY SEA");

        var svg = SvgLabelRenderer.RenderSvg(CarrierTemplateDefinition.Template, label);

        Assert.Contains(">BY SEA<", svg);
        Assert.Contains("rotate(-90", svg);
        Assert.DoesNotContain("null", svg);
    }
}
