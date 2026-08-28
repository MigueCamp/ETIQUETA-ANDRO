import type {
  ApiError,
  CarrierLabelData,
  CartonGtinLabelData,
  HangtagLabelData,
  PrnExtractionResult,
} from './types'

async function asJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let message = `Request failed (${response.status})`
    try {
      const body = (await response.json()) as ApiError
      if (body.error) message = body.error
    } catch {
      // response body wasn't JSON — keep the generic message
    }
    throw new Error(message)
  }
  return response.json() as Promise<T>
}

export async function parsePrn(file: File, poNumber?: string): Promise<PrnExtractionResult> {
  const form = new FormData()
  form.append('file', file)
  const query = poNumber ? `?poNumber=${encodeURIComponent(poNumber)}` : ''
  const response = await fetch(`/api/labels/parse${query}`, { method: 'POST', body: form })
  return asJson<PrnExtractionResult>(response)
}

export async function previewHangtag(labels: HangtagLabelData[]): Promise<string[]> {
  const response = await fetch('/api/labels/hangtag/preview', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(labels),
  })
  return asJson<string[]>(response)
}

export async function previewCartonGtin(labels: CartonGtinLabelData[]): Promise<string[]> {
  const response = await fetch('/api/labels/carton-gtin/preview', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(labels),
  })
  return asJson<string[]>(response)
}

export async function previewCarrier(labels: CarrierLabelData[]): Promise<string[]> {
  const response = await fetch('/api/labels/carrier/preview', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(labels),
  })
  return asJson<string[]>(response)
}

async function downloadPdf(url: string, body: unknown, fileName: string): Promise<void> {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    throw new Error(`No se pudo generar el PDF (${response.status})`)
  }
  const blob = await response.blob()
  const objectUrl = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = objectUrl
  link.download = fileName
  link.click()
  URL.revokeObjectURL(objectUrl)
}

export function renderHangtagPdf(labels: HangtagLabelData[]): Promise<void> {
  return downloadPdf('/api/labels/hangtag/render', labels, 'hangtag-labels.pdf')
}

export function renderCartonGtinPdf(labels: CartonGtinLabelData[]): Promise<void> {
  return downloadPdf('/api/labels/carton-gtin/render', labels, 'carton-gtin-labels.pdf')
}

export function renderCarrierPdf(labels: CarrierLabelData[]): Promise<void> {
  return downloadPdf('/api/labels/carrier/render', labels, 'carrier-labels.pdf')
}

/**
 * The "Artwork Approval Form" style sheet — letterhead, Brand/Style/Prod
 * Method table, dimension-arrow callouts, grouped one sheet per color — a
 * separate, additional download from the print-ready PDF above.
 */
export function renderHangtagSheetPdf(labels: HangtagLabelData[]): Promise<void> {
  return downloadPdf('/api/labels/hangtag/sheet', labels, 'hangtag-approval-sheet.pdf')
}

/**
 * Same approval-sheet style, one page per carton — combines the mirrored
 * GTIN panel pair and the carrier panel, matched by barcode.
 */
export function renderCartonSheetPdf(
  gtinLabels: CartonGtinLabelData[],
  carrierLabels: CarrierLabelData[],
): Promise<void> {
  return downloadPdf('/api/labels/carton/sheet', { gtinLabels, carrierLabels }, 'carton-approval-sheet.pdf')
}
