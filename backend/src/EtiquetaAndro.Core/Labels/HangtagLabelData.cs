namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Data for one HANGTAG STICKER (70mm x 38mm). All values come straight from
/// the PRN — this template is the only one where every visible field is
/// plain ZPL text (no rasterized values).
/// </summary>
public sealed record HangtagLabelData(
    string PoNumber,
    string Style,
    string Color,
    string Size,
    string Gender,
    string Barcode);
