namespace EtiquetaAndro.Core.Zpl;

/// <summary>
/// A single ^FO/^FT field extracted from a ZPL label block, keyed by its
/// print-space coordinates (in dots). Coordinates are stable across every
/// label generated from the same template, which is what lets us map a
/// position directly to a semantic field name.
/// </summary>
public sealed record ZplTextField(int X, int Y, string Value, bool IsBarcode);
