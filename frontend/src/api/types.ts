export type LabelTemplateKind =
  | 'HangtagSticker'
  | 'CartonShippingGtinPanel'
  | 'CartonShippingCarrierPanel'

export interface HangtagLabelData {
  poNumber: string
  style: string
  color: string
  size: string
  gender: string
  barcode: string
}

export interface CartonGtinLabelData {
  style: string
  colorCode: string
  colorDescription: string
  quantity: string
  cartonNumber: string
  size: string
  manufacturer: string
  country: string
  barcode: string
  poNumber: string | null
}

export interface CarrierLabelData {
  barcode: string
  carrier: string | null
  shipToCompany: string | null
  shipToStreet: string | null
  shipToCityState: string | null
  shipToZip: string | null
  manufacturerCode: string | null
  internalCode: string | null
}

export type LabelData = HangtagLabelData | CartonGtinLabelData | CarrierLabelData

export interface PrnExtractionResult {
  templateKind: LabelTemplateKind
  labels: LabelData[]
  previewsSvg: string[]
}

export interface ApiError {
  error: string
}

/** Field definitions the editor uses to render inputs for a given template kind — the one place that knows which fields are editable, in what order. */
export interface FieldDef<T> {
  key: keyof T
  label: string
}

export const HANGTAG_FIELDS: FieldDef<HangtagLabelData>[] = [
  { key: 'poNumber', label: 'P/O' },
  { key: 'style', label: 'Style' },
  { key: 'color', label: 'Color' },
  { key: 'size', label: 'Size' },
  { key: 'gender', label: 'Gender' },
  { key: 'barcode', label: 'Barcode' },
]

export const CARTON_GTIN_FIELDS: FieldDef<CartonGtinLabelData>[] = [
  { key: 'style', label: 'Style' },
  { key: 'colorCode', label: 'Color code' },
  { key: 'colorDescription', label: 'Color description' },
  { key: 'quantity', label: 'Quantity' },
  { key: 'cartonNumber', label: 'Carton number' },
  { key: 'size', label: 'Size' },
  { key: 'manufacturer', label: 'Manufacturer' },
  { key: 'country', label: 'Country' },
  { key: 'poNumber', label: 'PO number' },
  { key: 'barcode', label: 'Barcode' },
]

export const CARRIER_FIELDS: FieldDef<CarrierLabelData>[] = [
  { key: 'carrier', label: 'Carrier (e.g. BY SEA)' },
  { key: 'shipToCompany', label: 'Ship to — company' },
  { key: 'shipToStreet', label: 'Ship to — street' },
  { key: 'shipToCityState', label: 'Ship to — city, state' },
  { key: 'shipToZip', label: 'Ship to — ZIP' },
  { key: 'manufacturerCode', label: 'Manufacturer code (FROM)' },
  { key: 'internalCode', label: 'Internal use code' },
  { key: 'barcode', label: 'Barcode' },
]
