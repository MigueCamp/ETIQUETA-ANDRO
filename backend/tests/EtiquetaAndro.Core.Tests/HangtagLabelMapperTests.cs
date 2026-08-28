using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Tests;

public class HangtagLabelMapperTests
{
    private static IReadOnlyList<ZplLabelBlock> ParseFixture(string fileName)
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));
        return ZplParser.ParseLabels(zpl);
    }

    [Fact]
    public void Map_ProducesExpectedDataForFirstSku()
    {
        var blocks = ParseFixture("hangtag-bolsa-plastico.prn");

        var data = HangtagLabelMapper.Map(blocks[0]);

        Assert.Equal("134857", data.PoNumber);
        Assert.Equal("M115T", data.Style);
        Assert.Equal("BLACK", data.Color);
        Assert.Equal("LT/GT/GA", data.Size);
        Assert.Equal("MEN'S", data.Gender); // normalized from the PRN's "MEN´S" (U+00B4) to a plain apostrophe
        Assert.Equal("00774582786859", data.Barcode);
    }

    [Fact]
    public void Map_TracksColorAndSizeAcrossAllSkusInTheBatch()
    {
        var blocks = ParseFixture("hangtag-bolsa-plastico.prn");

        var data = blocks.Select(HangtagLabelMapper.Map).ToList();

        Assert.Equal(35, data.Count);
        // PO and style are constant for the whole PO in this sample; colour
        // and size are what actually varies from SKU to SKU.
        Assert.All(data, d => Assert.Equal("134857", d.PoNumber));
        Assert.All(data, d => Assert.Equal("M115T", d.Style));
        Assert.Contains(data, d => d.Color == "WHITE" && d.Size == "LT/GT/GA");
        Assert.Contains(data, d => d.Color == "SAFETY YELLOW" && d.Size == "4XLT/4TGT/4EGA");
        Assert.Equal(7, data.Select(d => d.Color).Distinct().Count());
        Assert.Equal(35, data.Select(d => d.Barcode).Distinct().Count());
    }

    [Fact]
    public void Detect_RecognizesHangtagTemplate()
    {
        var blocks = ParseFixture("hangtag-bolsa-plastico.prn");

        Assert.Equal(LabelTemplateKind.HangtagSticker, ZplTemplateDetector.Detect(blocks[0]));
    }
}
