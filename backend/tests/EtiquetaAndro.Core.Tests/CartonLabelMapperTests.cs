using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Tests;

public class CartonLabelMapperTests
{
    private static IReadOnlyList<ZplLabelBlock> ParseFixture(string fileName)
    {
        var zpl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));
        return ZplParser.ParseLabels(zpl);
    }

    [Fact]
    public void Map_ProducesExpectedDataForFirstCarton()
    {
        var blocks = ParseFixture("carton-gtin-derecha.prn");

        var data = CartonGtinLabelMapper.Map(blocks[0]);

        Assert.Equal("M115", data.Style);
        Assert.Equal("00", data.ColorCode);
        Assert.Equal("WHITE", data.ColorDescription);
        Assert.Equal("12", data.Quantity);
        Assert.Equal("", data.CartonNumber); // "CARTON"/"OF" print blank in the reference artwork
        Assert.Equal("LT", data.Size);
        Assert.Equal("07822", data.Manufacturer);
        Assert.Equal("BANGLADESH", data.Country);
        Assert.Equal("00774582787153", data.Barcode);
        Assert.Equal("23163", data.PoNumber); // a PO/job-level constant found in the PRN itself
    }

    [Fact]
    public void Map_AnExternallySuppliedPoNumberOverridesTheOneFoundInThePrn()
    {
        var blocks = ParseFixture("carton-gtin-derecha.prn");

        var data = CartonGtinLabelMapper.Map(blocks[0], poNumber: "134876");

        Assert.Equal("134876", data.PoNumber);
    }

    [Fact]
    public void Map_TreatsAMissingColorCodeFieldAsBlank_RatherThanThrowing()
    {
        // Reproduces a real block from "ETIQUETA ANDROMEDA DERECHA
        // 134856.prn": the exporting software omits the ^FT/^FD command
        // entirely (rather than emitting it with an empty value) on blocks
        // whose color has no code, while every other RightPanel field is
        // still present at its usual position.
        var fields = new[]
        {
            new ZplTextField(113, 1196, "M115", IsBarcode: false),
            new ZplTextField(523, 1101, "36", IsBarcode: false),
            new ZplTextField(381, 1109, "S", IsBarcode: false),
            new ZplTextField(120, 189, "07822", IsBarcode: false),
            new ZplTextField(117, 942, "BANGLADESH", IsBarcode: false),
            new ZplTextField(246, 692, "BLACK", IsBarcode: false),
            new ZplTextField(0, 0, "00774582786071", IsBarcode: true),
        };
        var block = new ZplLabelBlock { PrintWidthDots = 812, LabelLengthDots = 1218, TextFields = fields, RawZpl = string.Empty };

        var data = CartonGtinLabelMapper.Map(block);

        Assert.Equal("", data.ColorCode);
        Assert.Equal("M115", data.Style);
    }

    [Fact]
    public void Detect_RecognizesCartonGtinPanelTemplate()
    {
        var blocks = ParseFixture("carton-gtin-derecha.prn");

        Assert.Equal(LabelTemplateKind.CartonShippingGtinPanel, ZplTemplateDetector.Detect(blocks[0]));
    }

    [Fact]
    public void Map_HandlesTheMirroredLeftPanelLayout_ProducingTheSameSemanticData()
    {
        // CARA LARGA.prn is the visual mirror of DERECHA.prn (the two GTIN
        // panels printed side by side on the same carton) — same fields,
        // completely different (x, y) positions.
        var blocks = ParseFixture("carton-gtin-cara-larga.prn");

        var data = CartonGtinLabelMapper.Map(blocks[0]);

        Assert.Equal("M115", data.Style);
        Assert.Equal("00", data.ColorCode);
        Assert.Equal("WHITE", data.ColorDescription);
        Assert.Equal("12", data.Quantity);
        Assert.Equal("", data.CartonNumber);
        Assert.Equal("LT", data.Size);
        Assert.Equal("07822", data.Manufacturer);
        Assert.Equal("BANGLADESH", data.Country);
        Assert.Equal("00774582787153", data.Barcode);
        Assert.Equal("23163", data.PoNumber);
    }

    [Fact]
    public void MirroredPanel_AndRightPanel_ShareTheSameBarcodesInOrder()
    {
        var rightPanel = ParseFixture("carton-gtin-derecha.prn");
        var leftPanel = ParseFixture("carton-gtin-cara-larga.prn");

        Assert.Equal(
            rightPanel.Select(b => b.Barcode).ToList(),
            leftPanel.Select(b => b.Barcode).ToList());
    }

    [Fact]
    public void CarrierPanel_OnlyExposesTheBarcodeAsRealText()
    {
        var blocks = ParseFixture("carton-carrier-cara-corta.prn");

        var data = CarrierLabelMapper.Map(blocks[0]);

        Assert.Equal("00774582787153", data.Barcode);
        Assert.Null(data.Carrier);
        Assert.Null(data.ShipToCompany);

        Assert.Equal(LabelTemplateKind.CartonShippingCarrierPanel, ZplTemplateDetector.Detect(blocks[0]));
    }

    [Fact]
    public void CarrierPanel_AndGtinPanel_ShareTheSameBarcodesInOrder()
    {
        // The carrier panel and the GTIN panel are two faces of the same
        // cartons, so their barcode sequences must line up carton-for-carton.
        var gtinBlocks = ParseFixture("carton-gtin-derecha.prn");
        var carrierBlocks = ParseFixture("carton-carrier-cara-corta.prn");

        var gtinBarcodes = gtinBlocks.Select(b => b.Barcode).ToList();
        var carrierBarcodes = carrierBlocks.Select(b => b.Barcode).ToList();

        Assert.Equal(gtinBarcodes, carrierBarcodes);
    }
}
