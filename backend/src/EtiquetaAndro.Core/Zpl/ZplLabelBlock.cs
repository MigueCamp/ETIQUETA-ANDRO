namespace EtiquetaAndro.Core.Zpl;

/// <summary>
/// One physical label, i.e. the contents of a single ^XA ... ^XZ block that
/// declares a print width (^PW). The printer-init block at the top of every
/// exported .prn file has no ^PW and is filtered out by the parser.
/// </summary>
public sealed class ZplLabelBlock
{
    public required int PrintWidthDots { get; init; }
    public required int LabelLengthDots { get; init; }
    public required IReadOnlyList<ZplTextField> TextFields { get; init; }
    public required string RawZpl { get; init; }

    /// <summary>Looks up a non-barcode text field by its exact (x, y) position.</summary>
    public string? TryGetValue(int x, int y) =>
        TextFields.FirstOrDefault(f => !f.IsBarcode && f.X == x && f.Y == y)?.Value;

    /// <summary>Returns the value of the (first) barcode field in this label, if any.</summary>
    public string? Barcode => TextFields.FirstOrDefault(f => f.IsBarcode)?.Value;
}
