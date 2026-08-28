using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Tests;

public class ZplParserTests
{
    private static string ReadFixture(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    [Fact]
    public void ParseLabels_SkipsThePrinterInitBlock_AndReturnsOneEntryPerPhysicalLabel()
    {
        var zpl = ReadFixture("hangtag-bolsa-plastico.prn");

        var labels = ZplParser.ParseLabels(zpl);

        // 35 SKUs (7 colours x 5 sizes) in this file; the leading ^XA..^XZ
        // printer config block has no ^PW and must not be counted as a label.
        Assert.Equal(35, labels.Count);
        Assert.All(labels, l => Assert.Equal(304, l.PrintWidthDots));
        Assert.All(labels, l => Assert.Equal(559, l.LabelLengthDots));
    }

    [Fact]
    public void ParseLabels_ExtractsBarcodeValue_WithoutCode128ControlPrefix()
    {
        var zpl = ReadFixture("hangtag-bolsa-plastico.prn");

        var first = ZplParser.ParseLabels(zpl)[0];

        Assert.Equal("00774582786859", first.Barcode);
    }

    [Fact]
    public void ParseLabels_ExtractsPlainTextFieldsByPosition()
    {
        var zpl = ReadFixture("hangtag-bolsa-plastico.prn");

        var first = ZplParser.ParseLabels(zpl)[0];

        Assert.Equal("134857", first.TryGetValue(40, 431));
        Assert.Equal("M115T", first.TryGetValue(75, 440));
        Assert.Equal("BLACK", first.TryGetValue(115, 431));
        Assert.Equal("LT/GT/GA", first.TryGetValue(155, 431));
    }
}
