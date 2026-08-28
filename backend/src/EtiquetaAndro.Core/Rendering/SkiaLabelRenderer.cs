using EtiquetaAndro.Core.Templates;
using SkiaSharp;
using ZXing;
using ZXing.SkiaSharp;

namespace EtiquetaAndro.Core.Rendering;

/// <summary>
/// Renders a <see cref="LabelTemplate"/> to a print-ready PDF: one page per
/// label, sized to the template's exact physical dimensions (no page
/// margins, no scaling) so what comes out matches the reference artwork.
/// </summary>
public static class SkiaLabelRenderer
{
    private const double PtPerMm = 72.0 / 25.4;

    /// <summary>Renders one PDF page per item in <paramref name="labels"/>.</summary>
    public static byte[] RenderPdf(LabelTemplate template, IReadOnlyList<object> labels)
    {
        using var stream = new SKDynamicMemoryWStream();
        var widthPt = (float)(template.WidthMm * PtPerMm);
        var heightPt = (float)(template.HeightMm * PtPerMm);

        using (var document = SKDocument.CreatePdf(stream))
        {
            foreach (var label in labels)
            {
                using var canvas = document.BeginPage(widthPt, heightPt);
                DrawLabel(canvas, template, label);
                document.EndPage();
            }
        }

        using var data = stream.DetachAsData();
        return data.ToArray();
    }

    private static void DrawLabel(SKCanvas canvas, LabelTemplate template, object data)
    {
        canvas.Clear(SKColors.White);
        DrawLabelContent(canvas, template, data);
    }

    /// <summary>
    /// Draws just a template's own content (borders/rules/text/barcode) with
    /// no background clear — used both by <see cref="DrawLabel"/> (which
    /// clears first, for the standalone per-label PDF) and by
    /// <see cref="Sheets.SheetRenderer"/>, which places several of these on
    /// one bigger already-drawn sheet via <c>canvas.Translate</c>.
    /// </summary>
    internal static void DrawLabelContent(SKCanvas canvas, LabelTemplate template, object data)
    {
        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            IsAntialias = true,
        };
        foreach (var border in template.Borders)
        {
            strokePaint.StrokeWidth = (float)(border.StrokeWidthMm * PtPerMm);
            var rect = MmRect(border.X, border.Y, border.Width, border.Height);
            var radius = (float)(border.CornerRadiusMm * PtPerMm);
            canvas.DrawRoundRect(rect, radius, radius, strokePaint);
        }

        using var fillPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Black, IsAntialias = true };
        foreach (var rule in template.Rules)
        {
            var rect = MmRect(rule.X, rule.Y, rule.Width, rule.Height);
            canvas.DrawRect(rect, fillPaint);
        }

        foreach (var text in template.StaticTexts)
        {
            DrawText(canvas, text.X, text.BaselineY, text.FontSizePt, text.Weight, text.Text, maxWidthMm: null, text.RotationDegrees);
        }

        foreach (var text in template.DynamicTexts)
        {
            DrawText(canvas, text.X, text.BaselineY, text.FontSizePt, text.Weight, text.ValueSelector(data), text.MaxWidthMm, text.RotationDegrees);
        }

        foreach (var barcode in template.Barcodes)
        {
            DrawBarcode(canvas, barcode, barcode.ValueSelector(data));
        }
    }

    private static SKRect MmRect(double xMm, double yMm, double widthMm, double heightMm) => new(
        (float)(xMm * PtPerMm),
        (float)(yMm * PtPerMm),
        (float)((xMm + widthMm) * PtPerMm),
        (float)((yMm + heightMm) * PtPerMm));

    private static void DrawText(
        SKCanvas canvas,
        double xMm,
        double baselineYMm,
        double fontSizePt,
        FontWeight weight,
        string text,
        double? maxWidthMm,
        double rotationDegrees = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        using var typeface = SKTypeface.FromFamilyName(
            "Arial",
            weight == FontWeight.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, (float)fontSizePt);
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

        if (maxWidthMm is { } limitMm)
        {
            var actualWidthPt = font.MeasureText(text);
            var limitPt = (float)(limitMm * PtPerMm);
            if (actualWidthPt > limitPt)
            {
                // The system font substituted for the design's original one
                // (see MaxWidthMm remarks) measures wider for this string
                // than the space available — condense it horizontally
                // rather than let it run into the next fixed element.
                font.ScaleX = limitPt / actualWidthPt;
            }
        }

        var xPt = (float)(xMm * PtPerMm);
        var yPt = (float)(baselineYMm * PtPerMm);

        var rotate = rotationDegrees != 0;
        if (rotate)
        {
            canvas.Save();
            canvas.RotateDegrees((float)rotationDegrees, xPt, yPt);
        }

        canvas.DrawText(text, xPt, yPt, SKTextAlign.Left, font, paint);

        if (rotate)
        {
            canvas.Restore();
        }
    }

    /// <summary>
    /// Renders the barcode via ZXing (a well-tested Code128 implementation,
    /// deliberately not a hand-rolled symbol table) as a raster image at a
    /// resolution well above typical thermal-printer DPI, then scales that
    /// bitmap into the template's exact barcode box. A correctly-encoded
    /// raster barcode at this resolution prints just as crisply as a vector
    /// one — Code128 bars are flat rectangles, so there is no curve/edge
    /// quality lost by rasterizing.
    /// </summary>
    private static void DrawBarcode(SKCanvas canvas, BarcodeArea area, string value)
    {
        const int sourceDpi = 600;
        var widthPx = (int)Math.Round(area.Width / 25.4 * sourceDpi);
        var heightPx = (int)Math.Round(area.Height / 25.4 * sourceDpi);

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = widthPx,
                Height = heightPx,
                Margin = 0,
                PureBarcode = true,
            },
        };

        using var bitmap = writer.Write(value);
        var destRect = MmRect(area.X, area.Y, area.Width, area.Height);

        var rotate = area.RotationDegrees != 0;
        if (rotate)
        {
            canvas.Save();
            canvas.RotateDegrees((float)area.RotationDegrees, (float)(area.X * PtPerMm), (float)(area.Y * PtPerMm));
        }

        canvas.DrawBitmap(bitmap, destRect, SKSamplingOptions.Default);

        if (rotate)
        {
            canvas.Restore();
        }
    }
}
