namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Data for one CARTON &amp; SHIPPING GTIN panel (one of the two mirrored 6"x4"
/// panels shown side by side in the reference PDF).
/// </summary>
/// <param name="CartonNumber">
/// Not present as text in this template's PRN output — the reference
/// artwork's "CARTON"/"OF" cells print blank in every sample seen. Always
/// empty unless a future PRN sample is found to actually carry it.
/// </param>
/// <param name="PoNumber">
/// Extracted from the PRN by <see cref="CartonGtinLabelMapper"/> (a value
/// constant across every block in the file, e.g. "23163" in
/// carton-gtin-derecha.prn — a PO/job number, not a per-carton figure) when
/// present, or supplied externally (e.g. the matching hangtag PRN's own PO,
/// or a manually entered order number) otherwise.
/// </param>
/// <param name="ColorCode">
/// Blank for any block where the source PRN omits this field entirely — the
/// exporting software drops the ^FT/^FD command rather than emitting it with
/// an empty value on blocks where the color has no code.
/// </param>
public sealed record CartonGtinLabelData(
    string Style,
    string ColorCode,
    string ColorDescription,
    string Quantity,
    string CartonNumber,
    string Size,
    string Manufacturer,
    string Country,
    string Barcode,
    string? PoNumber);
