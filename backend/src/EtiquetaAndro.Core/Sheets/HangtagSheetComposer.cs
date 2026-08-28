using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Templates;

namespace EtiquetaAndro.Core.Sheets;

/// <summary>
/// Groups HANGTAG STICKER labels into approval sheets — one sheet (or more,
/// if a group has more sizes than fit) per (Style, Color), laid out 3-per-row
/// exactly as the reference artwork does for one color's full size run.
/// </summary>
public static class HangtagSheetComposer
{
    private const double SheetWidthMm = 320.05;

    // The reference artwork's own canvas (276.17mm tall) has room for a
    // letterhead band above the table that this sheet no longer draws — the
    // page is shortened by that same amount (matching SheetGeometry's own
    // shift for the table/PO/color line), and everything below the
    // letterhead's old bottom edge (the grid, arrows) is shifted up too, to
    // close the gap rather than leaving it blank.
    private const double RemovedLetterheadShiftMm = 23.56;
    private const double SheetHeightMm = 276.17 - RemovedLetterheadShiftMm;

    private const double GridOriginX = 60.47;
    private const double GridOriginY = 71.05 - RemovedLetterheadShiftMm;
    private const double ColumnPitchMm = 70.0;
    private const double RowPitchMm = 50.0;
    private const int Columns = 3;
    private const int MaxRowsPerPage = 4;
    private const int MaxPerPage = Columns * MaxRowsPerPage;

    public static IReadOnlyList<ComposedSheet> Compose(IReadOnlyList<HangtagLabelData> labels)
    {
        var sheets = new List<ComposedSheet>();

        var groups = labels
            .GroupBy(l => (l.Style, l.Color, l.PoNumber))
            .OrderBy(g => g.Key.Style).ThenBy(g => g.Key.Color);

        foreach (var group in groups)
        {
            var members = group.ToList();
            for (var pageStart = 0; pageStart < members.Count; pageStart += MaxPerPage)
            {
                var pageMembers = members.Skip(pageStart).Take(MaxPerPage).ToList();
                sheets.Add(ComposePage(group.Key.Style, group.Key.Color, group.Key.PoNumber, pageMembers));
            }
        }

        return sheets;
    }

    private static ComposedSheet ComposePage(string style, string color, string poNumber, IReadOnlyList<HangtagLabelData> members)
    {
        var panels = new List<PanelPlacement>();
        for (var i = 0; i < members.Count; i++)
        {
            var col = i % Columns;
            var row = i / Columns;
            panels.Add(new PanelPlacement(
                HangtagTemplateDefinition.Template,
                members[i],
                GridOriginX + col * ColumnPitchMm,
                GridOriginY + row * RowPitchMm));
        }

        var widthLabel = $"{HangtagTemplateDefinition.Template.WidthMm:0} MM";
        var heightLabel = $"{HangtagTemplateDefinition.Template.HeightMm:0} MM";

        var arrows = new List<DimensionArrow>
        {
            new(GridOriginX + 1.5, GridOriginY - 3.92, GridOriginX + ColumnPitchMm - 2.0, GridOriginY - 3.92, widthLabel),
            new(GridOriginX - 4.13, GridOriginY + 0.6, GridOriginX - 4.13, GridOriginY + HangtagTemplateDefinition.Template.HeightMm - 0.4, heightLabel),
        };

        return new ComposedSheet(
            Kind: SheetKind.Hangtag,
            WidthMm: SheetWidthMm,
            HeightMm: SheetHeightMm,
            PoNumber: poNumber,
            ColorValue: color,
            Table: new ApprovalTableData(StyleValue: style, ProdMethodValue: "HANGTAG STICKER"),
            Panels: panels,
            Arrows: arrows);
    }
}
