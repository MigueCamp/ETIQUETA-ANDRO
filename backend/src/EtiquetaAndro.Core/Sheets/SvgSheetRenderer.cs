using System.Globalization;
using System.Text;
using EtiquetaAndro.Core.Rendering;

namespace EtiquetaAndro.Core.Sheets;

/// <summary>
/// SVG counterpart to <see cref="SheetRenderer"/>, for on-screen preview of
/// an approval sheet before downloading the PDF — same draw order, same
/// geometry/wording tables, delegating each panel's own markup to
/// <see cref="SvgLabelRenderer"/> unchanged.
/// </summary>
public static class SvgSheetRenderer
{
    private const double ArrowLabelFontSizePt = 10.0;

    public static string RenderSvg(ComposedSheet sheet)
    {
        var sb = new StringBuilder();
        var w = Fmt(sheet.WidthMm);
        var h = Fmt(sheet.HeightMm);

        sb.Append(CultureInfo.InvariantCulture,
            $"""<svg xmlns="http://www.w3.org/2000/svg" width="{w}mm" height="{h}mm" viewBox="0 0 {w} {h}" font-family="Arial, sans-serif">""");
        sb.Append("""<rect x="0" y="0" width="100%" height="100%" fill="white"/>""");

        AppendApprovalTable(sb, sheet);
        AppendPoAndColorLine(sb, sheet);

        foreach (var arrow in sheet.Arrows)
        {
            AppendArrow(sb, arrow);
        }

        foreach (var panel in sheet.Panels)
        {
            sb.Append(CultureInfo.InvariantCulture, $"""<g transform="translate({Fmt(panel.OffsetXMm)},{Fmt(panel.OffsetYMm)})">""");
            SvgLabelRenderer.AppendLabelBody(sb, panel.Template, panel.Data);
            sb.Append("</g>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    private static void AppendApprovalTable(StringBuilder sb, ComposedSheet sheet)
    {
        var geometry = SheetGeometry.For(sheet.Kind);
        var wording = SheetWording.For(sheet.Kind);
        var t = geometry.Table;
        const string color = "#7A1217";

        sb.Append(CultureInfo.InvariantCulture,
            $"""<rect x="{Fmt(t.X)}" y="{Fmt(t.Y)}" width="{Fmt(t.Width)}" height="{Fmt(t.Height)}" fill="none" stroke="{color}" stroke-width="0.5"/>""");
        sb.Append(CultureInfo.InvariantCulture,
            $"""<line x1="{Fmt(t.ColDivider1X)}" y1="{Fmt(t.Y)}" x2="{Fmt(t.ColDivider1X)}" y2="{Fmt(t.Y + t.Height)}" stroke="{color}" stroke-width="0.5"/>""");
        sb.Append(CultureInfo.InvariantCulture,
            $"""<line x1="{Fmt(t.ColDivider2X)}" y1="{Fmt(t.Y)}" x2="{Fmt(t.ColDivider2X)}" y2="{Fmt(t.Y + t.Height)}" stroke="{color}" stroke-width="0.5"/>""");
        sb.Append(CultureInfo.InvariantCulture,
            $"""<line x1="{Fmt(t.X)}" y1="{Fmt(t.RowDividerY)}" x2="{Fmt(t.X + t.Width)}" y2="{Fmt(t.RowDividerY)}" stroke="{color}" stroke-width="0.5"/>""");

        void Cell(double x, double baselineY, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var escaped = System.Security.SecurityElement.Escape(text);
            sb.Append(CultureInfo.InvariantCulture,
                $"""<text x="{Fmt(x)}" y="{Fmt(baselineY)}" font-size="{Fmt(t.FontSizePt * PtToMm)}">{escaped}</text>""");
        }

        Cell(t.Col1X, t.Row1BaselineY, wording.BrandLabel);
        Cell(t.Col2X, t.Row1BaselineY, $"{wording.StyleLabel} {sheet.Table.StyleValue}");
        Cell(t.Col3X, t.Row1BaselineY, "1st Submitted Date :");

        Cell(t.Col1X, t.Row2BaselineY, wording.SecondaryLabel);
        Cell(t.Col2X, t.Row2BaselineY, $"{wording.ProdMethodLabel} {sheet.Table.ProdMethodValue}");
        Cell(t.Col3X, t.Row2BaselineY, "2nd Submitted Date :");
    }

    private static void AppendPoAndColorLine(StringBuilder sb, ComposedSheet sheet)
    {
        var geometry = SheetGeometry.For(sheet.Kind);
        var wording = SheetWording.For(sheet.Kind);

        sb.Append(CultureInfo.InvariantCulture,
            $"""<text x="{Fmt(geometry.PoX)}" y="{Fmt(geometry.PoBaselineY)}" font-size="{Fmt(geometry.PoFontSizePt * PtToMm)}">PO={System.Security.SecurityElement.Escape(sheet.PoNumber)}</text>""");
        sb.Append(CultureInfo.InvariantCulture,
            $"""<text x="{Fmt(geometry.ColorX)}" y="{Fmt(geometry.ColorBaselineY)}" font-size="{Fmt(geometry.ColorFontSizePt * PtToMm)}">{System.Security.SecurityElement.Escape(wording.ColorLinePrefix)} {System.Security.SecurityElement.Escape(sheet.ColorValue)}</text>""");
    }

    private static void AppendArrow(StringBuilder sb, DimensionArrow arrow)
    {
        const string shaftColor = "#08080A";
        const string headFill = "#EB2C33";
        const string headStroke = "#231F20";

        sb.Append(CultureInfo.InvariantCulture,
            $"""<line x1="{Fmt(arrow.X0)}" y1="{Fmt(arrow.Y0)}" x2="{Fmt(arrow.X1)}" y2="{Fmt(arrow.Y1)}" stroke="{shaftColor}" stroke-width="0.3"/>""");

        var horizontal = Math.Abs(arrow.X1 - arrow.X0) >= Math.Abs(arrow.Y1 - arrow.Y0);
        const double headLenMm = 2.6;
        const double headWidMm = 3.0;

        void Head(double tipX, double tipY, double dirX, double dirY)
        {
            var baseX = tipX - dirX * headLenMm;
            var baseY = tipY - dirY * headLenMm;
            var perpX = -dirY * headWidMm / 2;
            var perpY = dirX * headWidMm / 2;
            sb.Append(CultureInfo.InvariantCulture,
                $"""<polygon points="{Fmt(tipX)},{Fmt(tipY)} {Fmt(baseX + perpX)},{Fmt(baseY + perpY)} {Fmt(baseX - perpX)},{Fmt(baseY - perpY)}" fill="{headFill}" stroke="{headStroke}" stroke-width="0.15"/>""");
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

        var midX = (arrow.X0 + arrow.X1) / 2;
        var midY = (arrow.Y0 + arrow.Y1) / 2;
        var escapedLabel = System.Security.SecurityElement.Escape(arrow.Label);

        if (horizontal)
        {
            sb.Append(CultureInfo.InvariantCulture,
                $"""<text x="{Fmt(midX)}" y="{Fmt(arrow.Y0 - 1.6)}" font-size="{Fmt(ArrowLabelFontSizePt * PtToMm)}" text-anchor="middle">{escapedLabel}</text>""");
        }
        else
        {
            var x = arrow.X0 - 3.3;
            sb.Append(CultureInfo.InvariantCulture,
                $"""<text x="{Fmt(x)}" y="{Fmt(midY)}" font-size="{Fmt(ArrowLabelFontSizePt * PtToMm)}" text-anchor="middle" transform="rotate(-90,{Fmt(x)},{Fmt(midY)})">{escapedLabel}</text>""");
        }
    }

    private const double PtToMm = 25.4 / 72.0;

    private static string Fmt(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
