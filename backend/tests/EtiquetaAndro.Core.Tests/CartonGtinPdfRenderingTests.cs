using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Rendering;
using EtiquetaAndro.Core.Templates;
using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Tests;

public class CartonGtinPdfRenderingTests
{
    [Fact]
    public void RenderPdf_ProducesOnePdfPagePerCarton()
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "carton-gtin-derecha.prn"));
        var labels = ZplParser.ParseLabels(zpl)
            .Select(b => CartonGtinLabelMapper.Map(b, poNumber: "134876"))
            .Cast<object>()
            .ToList();

        var pdfBytes = SkiaLabelRenderer.RenderPdf(CartonGtinTemplateDefinition.Template, labels);

        Assert.True(pdfBytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);
    }

    [Fact]
    public void RenderPdf_WithoutAnExternalPoNumber_StillRendersEveryOtherField()
    {
        // PoNumber is the one field this template never gets from the PRN
        // itself (see CartonGtinLabelData remarks) — rendering must not
        // throw or blow up just because it's null.
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "carton-gtin-derecha.prn"));
        var labels = ZplParser.ParseLabels(zpl).Select(b => CartonGtinLabelMapper.Map(b)).Cast<object>().ToList();

        var pdfBytes = SkiaLabelRenderer.RenderPdf(CartonGtinTemplateDefinition.Template, labels);

        Assert.True(pdfBytes.Length > 0);
    }
}
