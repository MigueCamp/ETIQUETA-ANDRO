using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Sheets;

namespace EtiquetaAndro.Core.Tests;

public class CartonSheetComposerTests
{
    private static CartonGtinLabelData Gtin(string barcode) => new(
        Style: "NE725", ColorCode: "53", ColorDescription: "BURGUNDY HEATHER", Quantity: "12",
        CartonNumber: "1", Size: "M", Manufacturer: "07822", Country: "BANGLADESH",
        Barcode: barcode, PoNumber: "134876");

    private static CarrierLabelData Carrier(string barcode) => new(Barcode: barcode, Carrier: "BY SEA");

    [Fact]
    public void Compose_MatchesGtinAndCarrierLabelsByBarcode()
    {
        var gtinLabels = new List<CartonGtinLabelData> { Gtin("1"), Gtin("2") };
        var carrierLabels = new List<CarrierLabelData> { Carrier("1"), Carrier("2") };

        var result = CartonSheetComposer.Compose(gtinLabels, carrierLabels);

        Assert.Equal(2, result.Sheets.Count);
        Assert.Equal(0, result.UnmatchedGtinCount);
        Assert.All(result.Sheets, s => Assert.Equal(3, s.Panels.Count));
    }

    [Fact]
    public void Compose_SkipsGtinLabelsWithNoMatchingCarrierBarcode_AndCountsThem()
    {
        var gtinLabels = new List<CartonGtinLabelData> { Gtin("1"), Gtin("2") };
        var carrierLabels = new List<CarrierLabelData> { Carrier("1") };

        var result = CartonSheetComposer.Compose(gtinLabels, carrierLabels);

        Assert.Single(result.Sheets);
        Assert.Equal(1, result.UnmatchedGtinCount);
    }

    [Fact]
    public void Compose_WithNoMatchesAtAll_ReturnsNoSheets()
    {
        var result = CartonSheetComposer.Compose([Gtin("1")], [Carrier("other")]);

        Assert.Empty(result.Sheets);
        Assert.Equal(1, result.UnmatchedGtinCount);
    }

    [Fact]
    public void Compose_FillsTheApprovalTableFromTheGtinLabel()
    {
        var result = CartonSheetComposer.Compose([Gtin("1")], [Carrier("1")]);

        var sheet = Assert.Single(result.Sheets);
        Assert.Equal(SheetKind.CartonAndShipping, sheet.Kind);
        Assert.Equal("134876", sheet.PoNumber);
        Assert.Equal("BURGUNDY HEATHER", sheet.ColorValue);
        Assert.Equal("NE725", sheet.Table.StyleValue);
        Assert.Equal("CARTON & SHIPPING", sheet.Table.ProdMethodValue);
        Assert.Equal(4, sheet.Arrows.Count);
    }
}
