using EtiquetaAndro.Core.Rendering;
using EtiquetaAndro.Core.Templates;
using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Entry point that ties parsing, template detection and field mapping
/// together: given the raw contents of a .prn file, it identifies which
/// fixed template it was generated from and returns the mapped data for
/// every physical label inside it.
/// </summary>
public static class PrnLabelExtractor
{
    /// <param name="zplSource">Raw contents of the .prn file.</param>
    /// <param name="poNumber">
    /// The GTIN panel template never carries a PO number in its own ZPL data
    /// (see <see cref="CartonGtinLabelData"/>). Pass it here when known (e.g.
    /// entered by the user, or read from a companion hangtag PRN for the
    /// same order) so it ends up on the mapped data; ignored for other
    /// template kinds.
    /// </param>
    public static PrnExtractionResult Extract(string zplSource, string? poNumber = null)
    {
        var allBlocks = ZplParser.ParseLabels(zplSource);
        if (allBlocks.Count == 0)
        {
            throw new InvalidOperationException(
                "No label blocks found in this PRN (no ^XA...^XZ block declares a ^PW print width).");
        }

        var kind = ZplTemplateDetector.Detect(allBlocks[0])
            ?? throw new InvalidOperationException(
                $"Unrecognized label template (print width {allBlocks[0].PrintWidthDots}, " +
                $"length {allBlocks[0].LabelLengthDots} dots). This PRN doesn't match the HANGTAG " +
                "STICKER or CARTON & SHIPPING templates this system knows.");

        // A handful of real orders include size/colour blocks with every
        // other field present but no barcode at all — the GTIN simply hadn't
        // been assigned yet when the file was exported (observed in
        // "ETIQUETA ANDROMEDA DERECHA 134856.prn": 9 of 81 blocks). Such a
        // block can never become a scannable label, so it's dropped here
        // rather than failing the whole file the way a genuinely wrong
        // template layout should.
        var blocks = allBlocks.Where(b => b.Barcode is not null).ToList();
        var skippedBlockCount = allBlocks.Count - blocks.Count;
        if (blocks.Count == 0)
        {
            throw new InvalidOperationException(
                "Every label block in this PRN is missing its barcode field — there is nothing to extract.");
        }

        IReadOnlyList<object> labels = kind switch
        {
            LabelTemplateKind.HangtagSticker =>
                blocks.Select(HangtagLabelMapper.Map).ToList(),
            LabelTemplateKind.CartonShippingGtinPanel =>
                blocks.Select(b => CartonGtinLabelMapper.Map(b, poNumber)).ToList(),
            LabelTemplateKind.CartonShippingCarrierPanel =>
                blocks.Select(b => CarrierLabelMapper.Map(b)).ToList(),
            _ => throw new NotSupportedException($"No mapper registered for template kind '{kind}'."),
        };

        var template = LabelTemplateRegistry.TryGet(kind);
        var previews = template is not null
            ? labels.Select(l => SvgLabelRenderer.RenderSvg(template, l)).ToList()
            : labels.Select(_ => string.Empty).ToList();

        return new PrnExtractionResult
        {
            TemplateKind = kind,
            Labels = labels,
            PreviewsSvg = previews,
            SkippedBlockCount = skippedBlockCount,
        };
    }
}
