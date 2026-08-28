using EtiquetaAndro.Core.Zpl;

namespace EtiquetaAndro.Core.Labels;

/// <summary>
/// Identifies which fixed label template a parsed ZPL block belongs to, so an
/// uploaded .prn file can be routed to the right mapper without the user
/// having to say which template it is.
/// </summary>
public static class ZplTemplateDetector
{
    public static LabelTemplateKind? Detect(ZplLabelBlock block)
    {
        if (block.PrintWidthDots == 304 && block.LabelLengthDots == 559)
        {
            return LabelTemplateKind.HangtagSticker;
        }

        if (block.PrintWidthDots == 812 && block.LabelLengthDots == 1218)
        {
            // Both carton panels share the same page size; the carrier panel
            // is the one with the "CARRIER" static label, the GTIN panel is
            // the one with the "GTIN" static label.
            if (block.TextFields.Any(f => f.Value == "CARRIER"))
            {
                return LabelTemplateKind.CartonShippingCarrierPanel;
            }

            if (block.TextFields.Any(f => f.Value == "GTIN"))
            {
                return LabelTemplateKind.CartonShippingGtinPanel;
            }
        }

        return null;
    }
}
