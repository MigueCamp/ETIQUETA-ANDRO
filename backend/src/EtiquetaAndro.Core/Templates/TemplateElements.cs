namespace EtiquetaAndro.Core.Templates;

/// <summary>
/// Fixed visual building blocks a <see cref="LabelTemplate"/> is made of.
/// Every coordinate is in millimetres from the label's top-left corner —
/// the same coordinate system a renderer uses regardless of output format
/// (PDF, on-screen preview, ...). None of this is user-editable; it is the
/// one visual source of truth reproduced from the reference artwork PDFs.
/// </summary>
public enum FontWeight
{
    Regular,
    Bold,
}

/// <summary>
/// Static label text that never changes (e.g. "STYLE:").
/// </summary>
/// <param name="RotationDegrees">
/// Rotates the text around its own (X, BaselineY) anchor point — that point
/// does not move, the text direction does. Some panels (the carrier/ship-to
/// one) are entirely made of text rotated -90° from the printer's own
/// coordinate frame; see <see cref="CarrierTemplateDefinition"/> remarks.
/// </param>
public sealed record StaticText(double X, double BaselineY, double FontSizePt, FontWeight Weight, string Text, double RotationDegrees = 0);

/// <summary>
/// Text whose content comes from the label data at render time.
/// </summary>
/// <param name="ValueSelector">
/// Reads the value to print from a label-data instance. Takes <c>object</c>
/// rather than a generic type parameter so a <see cref="LabelTemplate"/> can
/// be stored/passed around uniformly; each template is only ever rendered
/// with the one data type it was written for.
/// </param>
/// <param name="MaxWidthMm">
/// The available horizontal space before this text would run into the next
/// fixed element. Since the exact value here comes from the PRN and its
/// length isn't fixed at design time (a country or colour name could be
/// longer than whatever the reference artwork happened to show), the
/// renderer shrinks the text horizontally to fit rather than letting it
/// overlap — the same "auto-shrink" behaviour real label design tools use
/// for variable-length fields. Null means the position is far enough from
/// anything else that this isn't needed.
/// </param>
/// <param name="RotationDegrees">See <see cref="StaticText.RotationDegrees"/>.</param>
public sealed record DynamicText(
    double X,
    double BaselineY,
    double FontSizePt,
    FontWeight Weight,
    Func<object, string> ValueSelector,
    double? MaxWidthMm = null,
    double RotationDegrees = 0);

/// <summary>
/// A thin filled bar — an underline beneath a field label, or a horizontal
/// or vertical divider line between sections of a template. Which one it is
/// just depends on whether <paramref name="Width"/> or <paramref name="Height"/>
/// is the larger dimension.
/// </summary>
public sealed record Rule(double X, double Y, double Width, double Height);

/// <summary>The label's outer border.</summary>
public sealed record RoundedBorder(double X, double Y, double Width, double Height, double CornerRadiusMm, double StrokeWidthMm);

/// <summary>
/// A Code128 barcode whose value comes from the label data, drawn to fill
/// exactly this box (no extra quiet zone beyond what's specified here).
/// </summary>
/// <param name="RotationDegrees">
/// Rotates the barcode around its own (X, Y) corner — see
/// <see cref="StaticText.RotationDegrees"/>. <paramref name="Width"/> and
/// <paramref name="Height"/> are always the barcode's own (unrotated)
/// length and bar-height, in that order.
/// </param>
public sealed record BarcodeArea(double X, double Y, double Width, double Height, Func<object, string> ValueSelector, double RotationDegrees = 0);
