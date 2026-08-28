using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Maps a parsed CARTON &amp; SHIPPING GTIN-panel label block to
/// <see cref="CartonGtinLabelData"/> using the fixed field positions (in ZPL
/// dots) observed in sample PRNs for this template.
/// </summary>
/// <remarks>
/// The reference PDF shows this panel printed twice per carton, mirrored
/// side by side (e.g. "MANUFACTURER | REVISION | COUNTRY | STYLE" on one
/// side, "STYLE | COUNTRY | REVISION | MANUFACTURER" on the other). Each
/// mirror is exported as its own PRN with a completely different set of
/// field coordinates for the exact same semantic data — this mapper knows
/// both layouts and picks whichever one actually matches the block.
///
/// The field once assumed to be the "CARTON" box's value is actually the PO
/// number: in "carton-gtin-derecha.prn" it holds the same "23163" on all 35
/// blocks (a PO/job-level constant, not a per-carton sequence number) — the
/// reference PDF's "CARTON"/"OF" cells print blank, and its "PO NUMBER" cell
/// is the one that holds a real value. So this position now feeds
/// <see cref="CartonGtinLabelData.PoNumber"/> (falling back to it only when
/// the caller doesn't pass an explicit one), and CartonNumber is left blank.
/// </remarks>
public static class CartonGtinLabelMapper
{
    private sealed record FieldPositions(
        (int X, int Y) Style,
        (int X, int Y) ColorCode,
        (int X, int Y) Quantity,
        (int X, int Y) PoNumberInPrn,
        (int X, int Y) Size,
        (int X, int Y) Manufacturer,
        (int X, int Y) Country,
        (int X, int Y) ColorDescription);

    // Observed in DERECHA.prn.
    private static readonly FieldPositions RightPanel = new(
        Style: (113, 1196),
        ColorCode: (249, 1119),
        Quantity: (523, 1101),
        PoNumberInPrn: (651, 1157),
        Size: (381, 1109),
        Manufacturer: (120, 189),
        Country: (117, 942),
        ColorDescription: (246, 692));

    // Observed in CARA LARGA.prn — the mirrored counterpart of RightPanel.
    private static readonly FieldPositions LeftPanel = new(
        Style: (97, 232),
        ColorCode: (233, 156),
        Quantity: (507, 138),
        PoNumberInPrn: (635, 194),
        Size: (365, 146),
        Manufacturer: (99, 1202),
        Country: (96, 719),
        ColorDescription: (230, 972));

    public static CartonGtinLabelData Map(ZplLabelBlock block, string? poNumber = null)
    {
        var positions = block.TryGetValue(RightPanel.Style.X, RightPanel.Style.Y) is not null
            ? RightPanel
            : block.TryGetValue(LeftPanel.Style.X, LeftPanel.Style.Y) is not null
                ? LeftPanel
                : throw new InvalidOperationException(
                    "Could not find a STYLE value at either known GTIN-panel layout position. " +
                    "This PRN does not match the CARTON & SHIPPING GTIN panel template.");

        return new CartonGtinLabelData(
            Style: Require(block, positions.Style, "STYLE"),
            // Unlike the other fields, the label design software omits this
            // ^FT/^FD command entirely on blocks where the color code is
            // blank (observed in "ETIQUETA ANDROMEDA DERECHA 134856.prn" —
            // some size blocks have every other field but no field at all at
            // the COLOR CODE position) rather than emitting it with an empty
            // value, so it can't be Require()'d like the rest.
            ColorCode: block.TryGetValue(positions.ColorCode.X, positions.ColorCode.Y) ?? string.Empty,
            ColorDescription: Require(block, positions.ColorDescription, "COLOR DESCRIPTION"),
            Quantity: Require(block, positions.Quantity, "QUANTITY"),
            // The reference artwork's "CARTON"/"OF" cells have no ZPL-text
            // equivalent in any sample seen — they print blank.
            CartonNumber: string.Empty,
            Size: Require(block, positions.Size, "SIZE"),
            Manufacturer: Require(block, positions.Manufacturer, "MANUFACTURER"),
            Country: Require(block, positions.Country, "COUNTRY"),
            Barcode: block.Barcode
                ?? throw new InvalidOperationException(
                    "Missing barcode field in carton GTIN label block."),
            PoNumber: poNumber ?? block.TryGetValue(positions.PoNumberInPrn.X, positions.PoNumberInPrn.Y));
    }

    private static string Require(ZplLabelBlock block, (int X, int Y) position, string fieldName)
    {
        return block.TryGetValue(position.X, position.Y)
            ?? throw new InvalidOperationException(
                $"Missing expected '{fieldName}' field at ({position.X},{position.Y}). " +
                "This PRN does not match the CARTON & SHIPPING GTIN panel template layout.");
    }
}
