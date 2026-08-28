import type { LabelData } from '../api/types'

interface Props {
  labels: LabelData[]
  selectedIndex: number
  onSelect: (index: number) => void
}

function summarize(label: LabelData): string {
  if ('color' in label) {
    return `${label.color} · ${label.size}`
  }
  if ('colorDescription' in label) {
    return `${label.colorDescription} · ${label.size}`
  }
  return label.shipToCompany ?? 'Sin datos de envío'
}

export function LabelList({ labels, selectedIndex, onSelect }: Props) {
  return (
    <ul className="label-list">
      {labels.map((label, index) => (
        <li key={label.barcode}>
          <button
            type="button"
            className={`label-list__item${index === selectedIndex ? ' label-list__item--active' : ''}`}
            onClick={() => onSelect(index)}
          >
            <span className="label-list__summary">{summarize(label)}</span>
            <span className="label-list__barcode">{label.barcode}</span>
          </button>
        </li>
      ))}
    </ul>
  )
}
