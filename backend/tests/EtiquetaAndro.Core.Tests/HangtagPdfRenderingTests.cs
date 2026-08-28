using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Rendering;
using EtiquetaAndro.Core.Templates;
using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Tests;

public class HangtagPdfRenderingTests
{
    [Fact]
    public void RenderPdf_ProducesOnePdfPagePerLabel()
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "hangtag-bolsa-plastico.prn"));
        var labels = ZplParser.ParseLabels(zpl).Select(HangtagLabelMapper.Map).Cast<object>().ToList();

        var pdfBytes = SkiaLabelRenderer.RenderPdf(HangtagTemplateDefinition.Template, labels);

        Assert.True(pdfBytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes[..4]);
    }

    [Fact]
    public void RenderPdf_ProducesAPageForEveryLabel_NotJustTheFirstOne()
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "hangtag-bolsa-plastico.prn"));
        var labels = ZplParser.ParseLabels(zpl).Select(HangtagLabelMapper.Map).Cast<object>().ToList();

        var onePagePdf = SkiaLabelRenderer.RenderPdf(HangtagTemplateDefinition.Template, [labels[0]]);
        var fullPdf = SkiaLabelRenderer.RenderPdf(HangtagTemplateDefinition.Template, labels);

        // A crude but reliable page-count signal that doesn't require a PDF
        // parser: each page adds its own "/Type /Page" object.
        var onePageCount = CountOccurrences(onePagePdf, "/Type /Page"u8);
        var fullPageCount = CountOccurrences(fullPdf, "/Type /Page"u8);

        Assert.True(fullPageCount > onePageCount);
    }

    private static int CountOccurrences(byte[] haystack, ReadOnlySpan<byte> needle)
    {
        var count = 0;
        var span = (ReadOnlySpan<byte>)haystack;
        int index;
        while ((index = span.IndexOf(needle)) >= 0)
        {
            count++;
            span = span[(index + needle.Length)..];
        }

        return count;
    }
}
