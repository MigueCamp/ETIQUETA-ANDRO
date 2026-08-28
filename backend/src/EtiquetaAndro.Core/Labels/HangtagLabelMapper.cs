using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Maps a parsed HANGTAG STICKER label block to <see cref="HangtagLabelData"/>
/// using the fixed field positions (in ZPL dots) observed in every sample
/// PRN for this template.
/// </summary>
public static class HangtagLabelMapper
{
    private const int PoX = 40, PoY = 431;
    private const int StyleX = 75, StyleY = 440;
    private const int ColorX = 115, ColorY = 431;
    private const int SizeX = 155, SizeY = 431;
    private const int GenderX = 65, GenderY = 127;

    public static HangtagLabelData Map(ZplLabelBlock block)
    {
        return new HangtagLabelData(
            PoNumber: Require(block, PoX, PoY, "P/O"),
            Style: Require(block, StyleX, StyleY, "STYLE"),
            Color: Require(block, ColorX, ColorY, "COLOR"),
            Size: Require(block, SizeX, SizeY, "SIZE"),
            Gender: Require(block, GenderX, GenderY, "GENDER"),
            Barcode: block.Barcode
                ?? throw new InvalidOperationException(
                    "Missing barcode field in hangtag label block."));
    }

    private static string Require(ZplLabelBlock block, int x, int y, string fieldName)
    {
        return block.TryGetValue(x, y)
            ?? throw new InvalidOperationException(
                $"Missing expected '{fieldName}' field at ({x},{y}). " +
                "This PRN does not match the HANGTAG STICKER template layout.");
    }
}
