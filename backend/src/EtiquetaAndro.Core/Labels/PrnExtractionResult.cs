namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// The outcome of extracting label data from one .prn file: which fixed
/// template it matched, and one mapped data record per physical label found
/// (all of the same runtime type — a .prn observed so far only ever contains
/// labels from a single template).
/// </summary>
public sealed class PrnExtractionResult
{
    public required LabelTemplateKind TemplateKind { get; init; }
    public required IReadOnlyList<object> Labels { get; init; }

    /// <summary>
    /// One SVG preview per entry in <see cref="Labels"/>, same order, so the
    /// UI can show a first preview without a second round trip. Rendered
    /// from the exact same fixed template used for the print PDF.
    /// </summary>
    public required IReadOnlyList<string> PreviewsSvg { get; init; }

    /// <summary>
    /// Number of label blocks found in the PRN but excluded from
    /// <see cref="Labels"/> because they had no barcode field at all — seen
    /// in real orders for a size/colour whose GTIN hadn't been assigned yet
    /// when the PRN was exported. Not an error unless it accounts for every
    /// block in the file (see <see cref="PrnLabelExtractor.Extract"/>).
    /// </summary>
    public required int SkippedBlockCount { get; init; }
}
