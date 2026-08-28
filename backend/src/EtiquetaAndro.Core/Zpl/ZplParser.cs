using System.Text.RegularExpressions;

namespace EtiquetaAndro.Core.Zpl;

/// <summary>
/// Minimal ZPL II reader focused on extracting variable field data (^FD values
/// keyed by their ^FO/^FT position) rather than rendering the label. It does
/// not interpret graphics (^GF), fonts, or drawing commands — those are part
/// of the fixed visual template, which is reproduced independently from the
/// reference PDFs, not from the PRN.
/// </summary>
public static partial class ZplParser
{
    [GeneratedRegex(@"\^XA(.*?)\^XZ", RegexOptions.Singleline)]
    private static partial Regex BlockPattern();

    [GeneratedRegex(@"\^PW(\d+)")]
    private static partial Regex PrintWidthPattern();

    [GeneratedRegex(@"\^LL(\d+)")]
    private static partial Regex LabelLengthPattern();

    // Matches a ^FT (field typeset) command followed by an optional
    // font/barcode setup and then ^FD<value>^FS. Newlines in the source file
    // are purely cosmetic in ZPL (commands are delimited by '^'/'~', not by
    // line breaks) — callers must strip them before matching, which is why
    // this pattern uses plain '.' rather than trying to reason about lines.
    // A barcode field, for example, spans two source lines:
    //   ^BY2,3,64^FT271,440^BCB,,N,N
    //   ^FH\^FD>;00774582786859^FS
    //
    // Deliberately excludes ^FO (field origin): in every sample, ^FO is only
    // ever used for ^GB (line/box) and ^GFA (graphic) commands, neither of
    // which contains ^FD. Matching ^FO here would make the non-greedy ".*?"
    // skip straight over one of those graphics looking for the next ^FD, and
    // wrongly attribute a *later* field's value to the ^FO's position.
    [GeneratedRegex(@"\^FT(\d+),(\d+)(.*?)\^FD(.*?)\^FS")]
    private static partial Regex FieldPattern();

    /// <summary>
    /// Parses raw ZPL source (the full contents of a .prn file) into one
    /// <see cref="ZplLabelBlock"/> per physical label. The printer
    /// initialization block common to every export is skipped automatically
    /// because it never declares a ^PW print width.
    /// </summary>
    public static IReadOnlyList<ZplLabelBlock> ParseLabels(string zplSource)
    {
        var blocks = new List<ZplLabelBlock>();

        foreach (Match blockMatch in BlockPattern().Matches(zplSource))
        {
            var body = blockMatch.Groups[1].Value;

            var pwMatch = PrintWidthPattern().Match(body);
            if (!pwMatch.Success)
            {
                // No ^PW => this is the shared printer-configuration block, not a label.
                continue;
            }

            var llMatch = LabelLengthPattern().Match(body);

            // Newlines are cosmetic in ZPL; flatten before scanning for
            // fields so a barcode's ^FD (which the exporter places on the
            // line after its ^BC command) is still matched to its ^FT.
            var flattened = body.Replace("\r", string.Empty).Replace("\n", string.Empty);

            var fields = new List<ZplTextField>();
            foreach (Match fieldMatch in FieldPattern().Matches(flattened))
            {
                var x = int.Parse(fieldMatch.Groups[1].Value);
                var y = int.Parse(fieldMatch.Groups[2].Value);
                var middle = fieldMatch.Groups[3].Value;
                var rawValue = fieldMatch.Groups[4].Value;

                var isBarcode = middle.Contains("^BC", StringComparison.Ordinal);
                var value = isBarcode ? StripBarcodeControlCodes(rawValue) : NormalizeText(rawValue);

                fields.Add(new ZplTextField(x, y, value, isBarcode));
            }

            blocks.Add(new ZplLabelBlock
            {
                PrintWidthDots = int.Parse(pwMatch.Groups[1].Value),
                LabelLengthDots = llMatch.Success ? int.Parse(llMatch.Groups[1].Value) : 0,
                TextFields = fields,
                RawZpl = body,
            });
        }

        return blocks;
    }

    /// <summary>
    /// Code128 field data in these files is prefixed with a two-character
    /// subset-invoke control sequence (observed: "&gt;;" to invoke Subset C
    /// for the numeric pairs that follow). Zebra defines several such
    /// sequences (&gt;7, &gt;8, &gt;9, &gt;:, &gt;;, &gt;=, &gt;&gt;); none of
    /// them are meaningful once the barcode has been re-rendered by our own
    /// vector barcode generator, so we strip any leading occurrences.
    /// </summary>
    /// <summary>
    /// The label design software that produced these files writes an
    /// apostrophe as U+00B4 (ACUTE ACCENT, "´") rather than U+0027
    /// (APOSTROPHE, "'") — visible in "MEN´S" — which is a font/encoding
    /// quirk of that tool, not meaningful data (the reference artwork PDFs
    /// render a plain apostrophe in the same spot). Normalize it so the
    /// generated label matches the reference exactly.
    /// </summary>
    private static string NormalizeText(string value) => value.Replace('´', '\'');

    private static string StripBarcodeControlCodes(string raw)
    {
        var value = raw;
        while (value.Length >= 2 && value[0] == '>')
        {
            value = value[2..];
        }

        return value;
    }
}
