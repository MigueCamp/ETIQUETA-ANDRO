using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Sheets;

namespace EtiquetaAndro.Core.Tests;

public class HangtagSheetComposerTests
{
    private static HangtagLabelData Label(string color, string size, string barcode) =>
        new(PoNumber: "134876", Style: "NE725", Color: color, Size: size, Gender: "MEN'S", Barcode: barcode);

    [Fact]
    public void Compose_GroupsLabelsByStyleAndColor_OneSheetPerGroup()
    {
        var labels = new List<HangtagLabelData>
        {
            Label("BURGUNDY HEATHER", "S/P", "1"),
            Label("BURGUNDY HEATHER", "M/M", "2"),
            Label("BLACK", "S/P", "3"),
        };

        var sheets = HangtagSheetComposer.Compose(labels);

        Assert.Equal(2, sheets.Count);
        Assert.Contains(sheets, s => s.ColorValue == "BURGUNDY HEATHER" && s.Panels.Count == 2);
        Assert.Contains(sheets, s => s.ColorValue == "BLACK" && s.Panels.Count == 1);
    }

    [Fact]
    public void Compose_WhenAColorGroupExceedsOnePagesGridCapacity_Paginates()
    {
        var labels = Enumerable.Range(0, 13)
            .Select(i => Label("BURGUNDY HEATHER", $"SIZE{i}", i.ToString()))
            .ToList();

        var sheets = HangtagSheetComposer.Compose(labels);

        Assert.Equal(2, sheets.Count);
        Assert.Equal(12, sheets[0].Panels.Count);
        Assert.Single(sheets[1].Panels);
        // Panel placements within one page must not overlap (each page is its
        // own coordinate space, so reusing the grid origin across pages is fine).
        foreach (var sheet in sheets)
        {
            var offsets = sheet.Panels.Select(p => (p.OffsetXMm, p.OffsetYMm)).ToList();
            Assert.Equal(offsets.Count, offsets.Distinct().Count());
        }
    }

    [Fact]
    public void Compose_FillsTheApprovalTableFromTheGroupsOwnData()
    {
        var sheets = HangtagSheetComposer.Compose([Label("BLACK", "S/P", "1")]);

        var sheet = Assert.Single(sheets);
        Assert.Equal(SheetKind.Hangtag, sheet.Kind);
        Assert.Equal("134876", sheet.PoNumber);
        Assert.Equal("NE725", sheet.Table.StyleValue);
        Assert.Equal("HANGTAG STICKER", sheet.Table.ProdMethodValue);
        Assert.Equal(2, sheet.Arrows.Count);
    }
}
