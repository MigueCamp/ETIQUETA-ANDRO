using System.Text;
using EtiquetaAndro.Core.Labels;
using EtiquetaAndro.Core.Rendering;
using EtiquetaAndro.Core.Sheets;
using EtiquetaAndro.Core.Templates;
using Microsoft.AspNetCore.Mvc;

namespace EtiquetaAndro.Api.Controllers;

/// <param name="GtinLabels">The GTIN panel data for every carton in the shipment.</param>
/// <param name="CarrierLabels">
/// Carrier/ship-to data, one per carton, matched to <see cref="GtinLabels"/>
/// by <see cref="CartonGtinLabelData.Barcode"/> — typically built client-side
/// by copying one shared set of shipment values onto each GTIN label's own
/// barcode (see the "apply to all" carrier workflow), since this data is
/// never real ZPL text.
/// </param>
public sealed record CartonSheetRequest(List<CartonGtinLabelData> GtinLabels, List<CarrierLabelData> CarrierLabels);

[ApiController]
[Route("api/[controller]")]
public class LabelsController : ControllerBase
{
    /// <summary>
    /// Accepts an uploaded .prn file, identifies which fixed label template
    /// it was generated from, and returns the mapped data for every physical
    /// label found inside it. The visual template itself is never read from
    /// the file — only these values feed the fixed on-screen/print template.
    /// </summary>
    [HttpPost("parse")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Parse(IFormFile file, [FromQuery] string? poNumber)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No .prn file was uploaded." });
        }

        var zplSource = await ReadAsUtf8Async(file);

        try
        {
            var result = PrnLabelExtractor.Extract(zplSource, poNumber);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Accepts an uploaded HANGTAG STICKER .prn file and returns a
    /// print-ready PDF — one 70mm x 38mm page per label, positioned exactly
    /// as in the reference artwork, with real data from the PRN.
    /// </summary>
    [HttpPost("hangtag/pdf")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> HangtagPdf(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No .prn file was uploaded." });
        }

        var zplSource = await ReadAsUtf8Async(file);

        try
        {
            var result = PrnLabelExtractor.Extract(zplSource);
            if (result.TemplateKind != LabelTemplateKind.HangtagSticker)
            {
                return UnprocessableEntity(new
                {
                    error = $"Expected a HANGTAG STICKER PRN but this file is a '{result.TemplateKind}'.",
                });
            }

            var pdfBytes = SkiaLabelRenderer.RenderPdf(HangtagTemplateDefinition.Template, result.Labels);
            return File(pdfBytes, "application/pdf", "hangtag-labels.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Re-renders SVG previews for HANGTAG STICKER label data the caller
    /// already has (typically edited in the UI after an initial
    /// <see cref="Parse"/>) — no .prn involved, just data in, previews out.
    /// </summary>
    [HttpPost("hangtag/preview")]
    public IActionResult HangtagPreview([FromBody] List<HangtagLabelData> labels)
    {
        var svgs = labels.Select(l => SvgLabelRenderer.RenderSvg(HangtagTemplateDefinition.Template, l)).ToList();
        return Ok(svgs);
    }

    /// <summary>
    /// Renders a print-ready PDF directly from HANGTAG STICKER label data the
    /// caller already has (typically edited in the UI), without needing the
    /// original .prn again.
    /// </summary>
    [HttpPost("hangtag/render")]
    public IActionResult HangtagRender([FromBody] List<HangtagLabelData> labels)
    {
        var pdfBytes = SkiaLabelRenderer.RenderPdf(HangtagTemplateDefinition.Template, labels.Cast<object>().ToList());
        return File(pdfBytes, "application/pdf", "hangtag-labels.pdf");
    }

    /// <summary>
    /// Re-renders SVG previews for CARTON &amp; SHIPPING GTIN-panel label data
    /// the caller already has (typically edited in the UI after an initial
    /// <see cref="Parse"/>).
    /// </summary>
    [HttpPost("carton-gtin/preview")]
    public IActionResult CartonGtinPreview([FromBody] List<CartonGtinLabelData> labels)
    {
        var svgs = labels.Select(l => SvgLabelRenderer.RenderSvg(CartonGtinTemplateDefinition.Template, l)).ToList();
        return Ok(svgs);
    }

    /// <summary>
    /// Renders a print-ready PDF directly from CARTON &amp; SHIPPING GTIN-panel
    /// label data the caller already has (typically edited in the UI).
    /// </summary>
    [HttpPost("carton-gtin/render")]
    public IActionResult CartonGtinRender([FromBody] List<CartonGtinLabelData> labels)
    {
        var pdfBytes = SkiaLabelRenderer.RenderPdf(CartonGtinTemplateDefinition.Template, labels.Cast<object>().ToList());
        return File(pdfBytes, "application/pdf", "carton-gtin-labels.pdf");
    }

    /// <summary>
    /// Accepts an uploaded CARTON &amp; SHIPPING GTIN-panel .prn file (either
    /// mirrored layout — see <see cref="CartonGtinLabelMapper"/>) and returns
    /// a print-ready PDF, one page per carton.
    /// </summary>
    [HttpPost("carton-gtin/pdf")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> CartonGtinPdf(IFormFile file, [FromQuery] string? poNumber)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No .prn file was uploaded." });
        }

        var zplSource = await ReadAsUtf8Async(file);

        try
        {
            var result = PrnLabelExtractor.Extract(zplSource, poNumber);
            if (result.TemplateKind != LabelTemplateKind.CartonShippingGtinPanel)
            {
                return UnprocessableEntity(new
                {
                    error = $"Expected a CARTON & SHIPPING GTIN panel PRN but this file is a '{result.TemplateKind}'.",
                });
            }

            var pdfBytes = SkiaLabelRenderer.RenderPdf(CartonGtinTemplateDefinition.Template, result.Labels);
            return File(pdfBytes, "application/pdf", "carton-gtin-labels.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Re-renders SVG previews for CARTON &amp; SHIPPING carrier-panel label
    /// data the caller already has. Carrier, ship-to, manufacturer and
    /// internal-use values never come from the PRN (see
    /// <see cref="CarrierLabelData"/>) — the caller fills them in once and
    /// applies them to every barcode from <see cref="Parse"/>.
    /// </summary>
    [HttpPost("carrier/preview")]
    public IActionResult CarrierPreview([FromBody] List<CarrierLabelData> labels)
    {
        var svgs = labels.Select(l => SvgLabelRenderer.RenderSvg(CarrierTemplateDefinition.Template, l)).ToList();
        return Ok(svgs);
    }

    /// <summary>
    /// Renders a print-ready PDF directly from CARTON &amp; SHIPPING
    /// carrier-panel label data the caller already has.
    /// </summary>
    [HttpPost("carrier/render")]
    public IActionResult CarrierRender([FromBody] List<CarrierLabelData> labels)
    {
        var pdfBytes = SkiaLabelRenderer.RenderPdf(CarrierTemplateDefinition.Template, labels.Cast<object>().ToList());
        return File(pdfBytes, "application/pdf", "carrier-labels.pdf");
    }

    /// <summary>
    /// Renders the HANGTAG STICKER "Artwork Approval Form" style sheet —
    /// letterhead, Brand/Style/Prod Method table, dimension-arrow callouts,
    /// and a grid of the actual label artwork, grouped one sheet (or more,
    /// paginated) per (Style, Color). This is a separate, additional
    /// download from <see cref="HangtagRender"/> — a review/sign-off
    /// document, not what feeds the label printer.
    /// </summary>
    [HttpPost("hangtag/sheet")]
    public IActionResult HangtagSheet([FromBody] List<HangtagLabelData> labels)
    {
        var sheets = HangtagSheetComposer.Compose(labels);
        var pdfBytes = SheetRenderer.RenderPdf(sheets);
        return File(pdfBytes, "application/pdf", "hangtag-approval-sheet.pdf");
    }

    /// <summary>
    /// Renders the CARTON &amp; SHIPPING "Artwork Approval Form" style sheet —
    /// one page per carton, combining the mirrored GTIN panel pair and the
    /// carrier panel (matched by barcode) under the same letterhead/table/
    /// arrow chrome as <see cref="HangtagSheet"/>. Separate, additional
    /// download from the individual GTIN/carrier print PDFs.
    /// </summary>
    [HttpPost("carton/sheet")]
    public IActionResult CartonSheet([FromBody] CartonSheetRequest request)
    {
        var composition = CartonSheetComposer.Compose(request.GtinLabels, request.CarrierLabels);
        if (composition.Sheets.Count == 0)
        {
            return UnprocessableEntity(new
            {
                error = "No GTIN label's barcode matched a carrier label's barcode — nothing to render.",
            });
        }

        var pdfBytes = SheetRenderer.RenderPdf(composition.Sheets);
        return File(pdfBytes, "application/pdf", "carton-approval-sheet.pdf");
    }

    private static async Task<string> ReadAsUtf8Async(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
