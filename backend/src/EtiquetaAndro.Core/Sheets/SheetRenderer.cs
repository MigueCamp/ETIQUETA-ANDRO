using EtiquetaAndro.Core.Rendering;
using SkiaSharp;

namespace EtiquetaAndro.Core.Sheets;

/// <summary>
/// Renders <see cref="ComposedSheet"/>s to a print-ready PDF: one page per
/// sheet, reproducing the "Artwork Approval Form" structure (Brand/Style/
/// Prod Method/Dates table, PO/color line, dimension-arrow callouts) around
/// the actual label artwork — without the printer's own letterhead — which
/// is drawn by delegating to <see cref="SkiaLabelRenderer"/> unchanged.
/// </summary>
public static class SheetRenderer
{
    private const double PtPerMm = 72.0 / 25.4;

    private static readonly SKColor TableColor = new(0x7A, 0x12, 0x17);
    private static readonly SKColor ArrowShaftColor = new(0x08, 0x08, 0x0A);
    private static readonly SKColor ArrowHeadFill = new(0xEB, 0x2C, 0x33);
    private static readonly SKColor ArrowHeadStroke = new(0x23, 0x1F, 0x20);
    private const double ArrowLabelFontSizePt = 10.0;

    public static byte[] RenderPdf(IReadOnlyList<ComposedSheet> sheets)
    {
        using var stream = new SKDynamicMemoryWStream();

        using (var document = SKDocument.CreatePdf(stream))
        {
            foreach (var sheet in sheets)
            {
                var widthPt = (float)(sheet.WidthMm * PtPerMm);
                var heightPt = (float)(sheet.HeightMm * PtPerMm);
                using var canvas = document.BeginPage(widthPt, heightPt);
                Draw(canvas, sheet);
                document.EndPage();
            }
        }

        using var data = stream.DetachAsData();
        return data.ToArray();
    }

    private static void Draw(SKCanvas canvas, ComposedSheet sheet)
    {
        canvas.Clear(SKColors.White);

        DrawApprovalTable(canvas, sheet);
        DrawPoAndColorLine(canvas, sheet);

        foreach (var arrow in sheet.Arrows)
        {
            DrawArrow(canvas, arrow);
        }

        foreach (var panel in sheet.Panels)
        {
            canvas.Save();
            canvas.Translate((float)(panel.OffsetXMm * PtPerMm), (float)(panel.OffsetYMm * PtPerMm));
            SkiaLabelRenderer.DrawLabelContent(canvas, panel.Template, panel.Data);
            canvas.Restore();
        }
    }

    private static void DrawApprovalTable(SKCanvas canvas, ComposedSheet sheet)
    {
        var geometry = SheetGeometry.For(sheet.Kind);
        var wording = SheetWording.For(sheet.Kind);

        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = TableColor,
            StrokeWidth = (float)(0.5 * PtPerMm),
            IsAntialias = true,
        };

        var t = geometry.Table;
        canvas.DrawRect(MmRect(t.X, t.Y, t.Width, t.Height), strokePaint);
        canvas.DrawLine(MmPt(t.ColDivider1X, t.Y), MmPt(t.ColDivider1X, t.Y + t.Height), strokePaint);
        canvas.DrawLine(MmPt(t.ColDivider2X, t.Y), MmPt(t.ColDivider2X, t.Y + t.Height), strokePaint);
        canvas.DrawLine(MmPt(t.X, t.RowDividerY), MmPt(t.X + t.Width, t.RowDividerY), strokePaint);

        using var typeface = SKTypeface.FromFamilyName("Arial");
        using var font = new SKFont(typeface, (float)t.FontSizePt);
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

        void Cell(double x, double baselineY, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            canvas.DrawText(text, (float)(x * PtPerMm), (float)(baselineY * PtPerMm), SKTextAlign.Left, font, textPaint);
        }

        Cell(t.Col1X, t.Row1BaselineY, wording.BrandLabel);
        Cell(t.Col2X, t.Row1BaselineY, $"{wording.StyleLabel} {sheet.Table.StyleValue}");
        Cell(t.Col3X, t.Row1BaselineY, "1st Submitted Date :");

        Cell(t.Col1X, t.Row2BaselineY, wording.SecondaryLabel);
        Cell(t.Col2X, t.Row2BaselineY, $"{wording.ProdMethodLabel} {sheet.Table.ProdMethodValue}");
        Cell(t.Col3X, t.Row2BaselineY, "2nd Submitted Date :");
    }

    private static void DrawPoAndColorLine(SKCanvas canvas, ComposedSheet sheet)
    {
        var geometry = SheetGeometry.For(sheet.Kind);
        var wording = SheetWording.For(sheet.Kind);

        using var typeface = SKTypeface.FromFamilyName("Arial");
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

        using var poFont = new SKFont(typeface, (float)geometry.PoFontSizePt);
        canvas.DrawText($"PO={sheet.PoNumber}", (float)(geometry.PoX * PtPerMm), (float)(geometry.PoBaselineY * PtPerMm), SKTextAlign.Left, poFont, paint);

        using var colorFont = new SKFont(typeface, (float)geometry.ColorFontSizePt);
        canvas.DrawText($"{wording.ColorLinePrefix} {sheet.ColorValue}", (float)(geometry.ColorX * PtPerMm), (float)(geometry.ColorBaselineY * PtPerMm), SKTextAlign.Left, colorFont, paint);
    }

    private static void DrawArrow(SKCanvas canvas, DimensionArrow arrow)
    {
        using var shaftPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = ArrowShaftColor, StrokeWidth = (float)(0.3 * PtPerMm), IsAntialias = true };
        using var headFill = new SKPaint { Style = SKPaintStyle.Fill, Color = ArrowHeadFill, IsAntialias = true };
        using var headStroke = new SKPaint { Style = SKPaintStyle.Stroke, Color = ArrowHeadStroke, StrokeWidth = (float)(0.15 * PtPerMm), IsAntialias = true };

        canvas.DrawLine(MmPt(arrow.X0, arrow.Y0), MmPt(arrow.X1, arrow.Y1), shaftPaint);

        var horizontal = Math.Abs(arrow.X1 - arrow.X0) >= Math.Abs(arrow.Y1 - arrow.Y0);
        const double headLenMm = 2.6;
        const double headWidMm = 3.0;

        void Head(double tipX, double tipY, double dirX, double dirY)
        {
            var baseX = tipX - dirX * headLenMm;
            var baseY = tipY - dirY * headLenMm;
            var perpX = -dirY * headWidMm / 2;
            var perpY = dirX * headWidMm / 2;

            using var path = new SKPath();
            path.MoveTo((float)(tipX * PtPerMm), (float)(tipY * PtPerMm));
            path.LineTo((float)((baseX + perpX) * PtPerMm), (float)((baseY + perpY) * PtPerMm));
            path.LineTo((float)((baseX - perpX) * PtPerMm), (float)((baseY - perpY) * PtPerMm));
            path.Close();
            canvas.DrawPath(path, headFill);
            canvas.DrawPath(path, headStroke);
        }

        if (horizontal)
        {
            var dir = arrow.X1 >= arrow.X0 ? 1 : -1;
            Head(arrow.X0, arrow.Y0, -dir, 0);
            Head(arrow.X1, arrow.Y1, dir, 0);
        }
        else
        {
            var dir = arrow.Y1 >= arrow.Y0 ? 1 : -1;
            Head(arrow.X0, arrow.Y0, 0, -dir);
            Head(arrow.X1, arrow.Y1, 0, dir);
        }

        using var typeface = SKTypeface.FromFamilyName("Arial");
        using var font = new SKFont(typeface, (float)ArrowLabelFontSizePt);
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        var midX = (arrow.X0 + arrow.X1) / 2;
        var midY = (arrow.Y0 + arrow.Y1) / 2;

        if (horizontal)
        {
            canvas.DrawText(arrow.Label, (float)(midX * PtPerMm), (float)((arrow.Y0 - 1.6) * PtPerMm), SKTextAlign.Center, font, textPaint);
        }
        else
        {
            canvas.Save();
            var x = (float)((arrow.X0 - 3.3) * PtPerMm);
            var y = (float)(midY * PtPerMm);
            canvas.RotateDegrees(-90, x, y);
            canvas.DrawText(arrow.Label, x, y, SKTextAlign.Center, font, textPaint);
            canvas.Restore();
        }
    }

    private static SKRect MmRect(double xMm, double yMm, double widthMm, double heightMm) => new(
        (float)(xMm * PtPerMm),
        (float)(yMm * PtPerMm),
        (float)((xMm + widthMm) * PtPerMm),
        (float)((yMm + heightMm) * PtPerMm));

    private static SKPoint MmPt(double xMm, double yMm) => new((float)(xMm * PtPerMm), (float)(yMm * PtPerMm));
}
