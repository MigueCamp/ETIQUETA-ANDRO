using EtiquetaAndro.Core.Labels;

namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// Looks up the fixed <see cref="LabelTemplate"/> for a given
/// <see cref="LabelTemplateKind"/>.
/// </summary>
public static class LabelTemplateRegistry
{
    public static LabelTemplate? TryGet(LabelTemplateKind kind) => kind switch
    {
        LabelTemplateKind.HangtagSticker => HangtagTemplateDefinition.Template,
        LabelTemplateKind.CartonShippingGtinPanel => CartonGtinTemplateDefinition.Template,
        LabelTemplateKind.CartonShippingCarrierPanel => CarrierTemplateDefinition.Template,
        _ => null,
    };
}
