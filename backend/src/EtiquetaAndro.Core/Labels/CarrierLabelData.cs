namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Data for one CARTON &amp; SHIPPING carrier / ship-to panel.
/// </summary>
/// <remarks>
/// In every sample PRN, the carrier, ship-to address, manufacturer and
/// internal-use values on this panel are burned into rasterized ^GFA
/// graphics rather than emitted as ZPL text — they are byte-identical across
/// every label in a file, i.e. constant for the shipment, not per label.
/// They are NOT recoverable by a byte-level ZPL parser (that would require
/// OCR on the decoded bitmap). Only <see cref="Barcode"/> is real ZPL text
/// and can be parsed directly; the remaining fields must come from an
/// order/shipment header the user fills in once, not from the PRN.
///
/// The ship-to address is four separate fields, not one free-text block,
/// because the reference artwork prints it as four fixed lines (company,
/// street, city/state, ZIP) at four fixed positions — matching how every
/// other field in this system is one value per fixed visual slot.
/// </remarks>
public sealed record CarrierLabelData(
    string Barcode,
    string? Carrier = null,
    string? ShipToCompany = null,
    string? ShipToStreet = null,
    string? ShipToCityState = null,
    string? ShipToZip = null,
    string? ManufacturerCode = null,
    string? InternalCode = null);
