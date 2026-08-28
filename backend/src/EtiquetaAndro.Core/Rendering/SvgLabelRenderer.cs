using System.Globalization;
using System.Text;
using EtiquetaAndro.Core.Templates;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.OneD;

namespace EtiquetaAndro.Core.Rendering;

/// <summary>
/// Renders a <see cref="LabelTemplate"/> to an SVG string for on-screen
/// preview, walking the exact same element list <see cref="SkiaLabelRenderer"/>
/// uses for the print PDF — there is no separate "preview layout", so the
/// preview can never drift from what actually prints.
/// </summary>
public static class SvgLabelRenderer
{
    private const double PtToMm = 25.4 / 72.0;

    public static string RenderSvg(LabelTemplate template, object data)
    {
        var sb = new StringBuilder();
        var w = template.WidthMm.ToString(CultureInfo.InvariantCulture);
        var h = template.HeightMm.ToString(CultureInfo.InvariantCulture);

        sb.Append(CultureInfo.InvariantCulture, $"""<svg xmlns="http://www.w3.org/2000/svg" width="{w}mm" height="{h}mm" viewBox="0 0 {w} {h}" font-family="Arial, sans-serif">""");
        sb.Append("""<rect x="0" y="0" width="100%" height="100%" fill="white"/>""");
        AppendLabelBody(sb, template, data);
        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>
    /// Appends just a template's own markup (borders/rules/text/barcode), no
    /// <c>&lt;svg&gt;</c> wrapper or background — used both by
    /// <see cref="RenderSvg"/> and by <see cref="Sheets.SvgSheetRenderer"/>,
    /// which places several of these inside one bigger sheet's
    /// <c>&lt;g transform="translate(...)"&gt;</c> groups.
    /// </summary>
    internal static void AppendLabelBody(StringBuilder sb, LabelTemplate template, object data)
    {
        foreach (var border in template.Borders)
        {
            sb.Append(CultureInfo.InvariantCulture,
                $"""<rect x="{Fmt(border.X)}" y="{Fmt(border.Y)}" width="{Fmt(border.Width)}" height="{Fmt(border.Height)}" rx="{Fmt(border.CornerRadiusMm)}" fill="none" stroke="black" stroke-width="{Fmt(border.StrokeWidthMm)}"/>""");
        }

        foreach (var rule in template.Rules)
        {
            sb.Append(CultureInfo.InvariantCulture,
                $"""<rect x="{Fmt(rule.X)}" y="{Fmt(rule.Y)}" width="{Fmt(rule.Width)}" height="{Fmt(rule.Height)}" fill="black"/>""");
        }

        foreach (var text in template.StaticTexts)
        {
            AppendText(sb, text.X, text.BaselineY, text.FontSizePt, text.Weight, text.Text, maxWidthMm: null, text.RotationDegrees);
        }

        foreach (var text in template.DynamicTexts)
        {
            AppendText(sb, text.X, text.BaselineY, text.FontSizePt, text.Weight, text.ValueSelector(data), text.MaxWidthMm, text.RotationDegrees);
        }

        foreach (var barcode in template.Barcodes)
        {
            AppendBarcode(sb, barcode, barcode.ValueSelector(data));
        }
    }

    private static void AppendText(StringBuilder sb, double xMm, double baselineYMm, double fontSizePt, FontWeight weight, string text, double? maxWidthMm, double rotationDegrees = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var weightAttr = weight == FontWeight.Bold ? "bold" : "normal";
        var escaped = System.Security.SecurityElement.Escape(text);
        var transformAttr = rotationDegrees != 0
            ? $""" transform="rotate({Fmt(rotationDegrees)},{Fmt(xMm)},{Fmt(baselineYMm)})" """
            : " ";

        // The viewBox maps user units to millimetres (not px), so font-size
        // must be given as a bare number in that same space — NOT "12pt".
        // SVG resolves an absolute CSS unit like pt via the fixed 96px/in
        // reference and then treats the result as a raw user-unit count, so
        // "21pt" here would come out as ~28 user units (28mm!), not the
        // ~7.4mm the design actually calls for. Geometry attributes
        // (x/y/width/stroke-width/textLength) don't have this problem since
        // they're already bare numbers in mm.
        var fontSizeMm = fontSizePt * PtToMm;

        // Browsers lay out text with real font metrics too, so the same
        // "don't run into the next element" risk from SkiaLabelRenderer
        // applies here — approximated with textLength/lengthAdjust, which
        // every SVG-capable browser honours. Unlike a bare width check this
        // must stay conditional on the text actually being too wide: an SVG
        // textLength doesn't just cap width the way SkiaLabelRenderer's
        // ScaleX does, it forces the rendered width to match exactly — so
        // applying it unconditionally stretches every short value (e.g.
        // "WHITE" in a budget sized for "BURGUNDY HEATHER") out to fill the
        // whole budget, which looks worse than doing nothing. Measure with
        // the same SkiaSharp engine SkiaLabelRenderer uses (close enough to
        // a browser's own Arial metrics) and only constrain when it's
        // genuinely needed, exactly mirroring the PDF's shrink-only rule.
        var lengthAttr = maxWidthMm is { } max && MeasureTextWidthMm(text, fontSizePt, weight) > max
            ? $""" textLength="{Fmt(max)}" lengthAdjust="spacingAndGlyphs" """
            : " ";

        sb.Append(CultureInfo.InvariantCulture,
            $"""<text x="{Fmt(xMm)}" y="{Fmt(baselineYMm)}" font-size="{Fmt(fontSizeMm)}" font-weight="{weightAttr}"{lengthAttr}{transformAttr}>{escaped}</text>""");
    }

    private static double MeasureTextWidthMm(string text, double fontSizePt, FontWeight weight)
    {
        using var typeface = SKTypeface.FromFamilyName(
            "Arial",
            weight == FontWeight.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, (float)fontSizePt);
        return font.MeasureText(text) * PtToMm;
    }

    /// <summary>
    /// Draws the barcode as vector &lt;rect&gt; bars (one per contiguous
    /// black run) rather than a raster image — in a browser, vector bars
    /// stay crisp at any zoom level with no need to pick a source DPI.
    /// </summary>
    private static void AppendBarcode(StringBuilder sb, BarcodeArea area, string value)
    {
        var writer = new Code128Writer();
        var matrix = writer.encode(value, BarcodeFormat.CODE_128, 0, 0);
        var moduleCount = matrix.Width;

        var transform = area.RotationDegrees != 0
            ? $"translate({Fmt(area.X)},{Fmt(area.Y)}) rotate({Fmt(area.RotationDegrees)})"
            : $"translate({Fmt(area.X)},{Fmt(area.Y)})";
        sb.Append(CultureInfo.InvariantCulture, $"""<g transform="{transform}">""");

        var moduleWidthMm = area.Width / moduleCount;
        var runStart = -1;
        for (var i = 0; i <= moduleCount; i++)
        {
            var isBlack = i < moduleCount && matrix[i, 0];
            if (isBlack && runStart < 0)
            {
                runStart = i;
            }
            else if (!isBlack && runStart >= 0)
            {
                var x = runStart * moduleWidthMm;
                var width = (i - runStart) * moduleWidthMm;
                sb.Append(CultureInfo.InvariantCulture,
                    $"""<rect x="{Fmt(x)}" y="0" width="{Fmt(width)}" height="{Fmt(area.Height)}" fill="black"/>""");
                runStart = -1;
            }
        }

        sb.Append("</g>");
    }

    private static string Fmt(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
