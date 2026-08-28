using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Maps a parsed CARTON &amp; SHIPPING carrier-panel label block to
/// <see cref="CarrierLabelData"/>. Only the barcode is extractable as text —
/// see <see cref="CarrierLabelData"/> remarks for why the rest is not.
/// </summary>
public static class CarrierLabelMapper
{
    public static CarrierLabelData Map(
        ZplLabelBlock block,
        string? carrier = null,
        string? shipToCompany = null,
        string? shipToStreet = null,
        string? shipToCityState = null,
        string? shipToZip = null,
        string? manufacturerCode = null,
        string? internalCode = null)
    {
        return new CarrierLabelData(
            Barcode: block.Barcode
                ?? throw new InvalidOperationException(
                    "Missing barcode field in carrier label block."),
            Carrier: carrier,
            ShipToCompany: shipToCompany,
            ShipToStreet: shipToStreet,
            ShipToCityState: shipToCityState,
            ShipToZip: shipToZip,
            ManufacturerCode: manufacturerCode,
            InternalCode: internalCode);
    }
}
