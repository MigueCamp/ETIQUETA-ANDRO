interface Props {
  svg: string
}

/**
 * Renders the server-generated SVG as-is. The design (positions, borders,
 * fonts, barcode) is fixed and comes entirely from the backend template —
 * this component has no layout knowledge and cannot be used to move or
 * resize anything, by construction.
 */
export function LabelPreview({ svg }: Props) {
  if (!svg) {
    return <div className="label-preview label-preview--empty">Sin vista previa</div>
  }

  return (
    <div className="label-preview" dangerouslySetInnerHTML={{ __html: svg }} />
  )
}
