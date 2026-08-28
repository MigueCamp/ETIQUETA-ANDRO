namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// The fixed set of label designs this system knows how to reproduce.
/// Every kind has an immutable visual template (positions, fonts, lines,
/// barcode placement) defined independently of any PRN — the PRN only
/// supplies the values for a given kind.
/// </summary>
public enum LabelTemplateKind
{
    /// <summary>70mm x 38mm hangtag sticker (P/O, STYLE, COLOR, SIZE + barcode).</summary>
    HangtagSticker,

    /// <summary>6"x4" carton & shipping GTIN panel (one of the two mirrored panels).</summary>
    CartonShippingGtinPanel,

    /// <summary>6"x4" carton & shipping carrier / ship-to panel.</summary>
    CartonShippingCarrierPanel,
}
