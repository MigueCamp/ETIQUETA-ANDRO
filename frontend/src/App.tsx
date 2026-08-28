import { useCallback, useRef, useState } from 'react'
import './App.css'
import {
  parsePrn,
  previewCarrier,
  previewCartonGtin,
  previewHangtag,
  renderCarrierPdf,
  renderCartonGtinPdf,
  renderCartonSheetPdf,
  renderHangtagPdf,
  renderHangtagSheetPdf,
} from './api/client'
import {
  CARRIER_FIELDS,
  CARTON_GTIN_FIELDS,
  HANGTAG_FIELDS,
  type CarrierLabelData,
  type CartonGtinLabelData,
  type HangtagLabelData,
  type LabelData,
  type PrnExtractionResult,
} from './api/types'
import { LabelFieldsForm } from './components/LabelFieldsForm'
import { LabelList } from './components/LabelList'
import { LabelPreview } from './components/LabelPreview'
import { UploadZone } from './components/UploadZone'

const TEMPLATE_NAMES: Record<string, string> = {
  HangtagSticker: 'Hangtag Sticker (70mm x 38mm)',
  CartonShippingGtinPanel: 'Carton & Shipping — panel GTIN',
  CartonShippingCarrierPanel: 'Carton & Shipping — panel Carrier/Ship-To',
}

async function previewFor(kind: string, labels: LabelData[]): Promise<string[]> {
  if (kind === 'HangtagSticker') return previewHangtag(labels as HangtagLabelData[])
  if (kind === 'CartonShippingGtinPanel') return previewCartonGtin(labels as CartonGtinLabelData[])
  return previewCarrier(labels as CarrierLabelData[])
}

function App() {
  const [poNumber, setPoNumber] = useState('')
  const [result, setResult] = useState<PrnExtractionResult | null>(null)
  const [labels, setLabels] = useState<LabelData[]>([])
  const [previews, setPreviews] = useState<string[]>([])
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [isLoading, setIsLoading] = useState(false)
  const [isDownloading, setIsDownloading] = useState(false)
  const [isDownloadingSheet, setIsDownloadingSheet] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Shared shipment-level values for the CARTON & SHIPPING approval sheet's
  // carrier panel — same "fill once" idea as the standalone Carrier flow,
  // since this data is never real ZPL text (see CarrierLabelData remarks).
  const [sheetCarrierData, setSheetCarrierData] = useState<CarrierLabelData>({
    barcode: '',
    carrier: null,
    shipToCompany: null,
    shipToStreet: null,
    shipToCityState: null,
    shipToZip: null,
    manufacturerCode: null,
    internalCode: null,
  })

  const previewDebounce = useRef<number | undefined>(undefined)

  const handleFile = useCallback(
    async (file: File) => {
      setIsLoading(true)
      setError(null)
      try {
        const parsed = await parsePrn(file, poNumber || undefined)
        setResult(parsed)
        setLabels(parsed.labels)
        setPreviews(parsed.previewsSvg)
        setSelectedIndex(0)
      } catch (e) {
        setError(e instanceof Error ? e.message : String(e))
        setResult(null)
        setLabels([])
        setPreviews([])
      } finally {
        setIsLoading(false)
      }
    },
    [poNumber],
  )

  const refreshPreview = useCallback((kind: string, allLabels: LabelData[], index: number) => {
    window.clearTimeout(previewDebounce.current)
    previewDebounce.current = window.setTimeout(async () => {
      try {
        const svgs = await previewFor(kind, [allLabels[index]])
        setPreviews((prev) => {
          const next = [...prev]
          next[index] = svgs[0]
          return next
        })
      } catch (e) {
        setError(e instanceof Error ? e.message : String(e))
      }
    }, 300)
  }, [])

  const handleFieldsChange = useCallback(
    (next: LabelData) => {
      if (!result) return
      setLabels((prev) => {
        const updated = [...prev]
        updated[selectedIndex] = next
        refreshPreview(result.templateKind, updated, selectedIndex)
        return updated
      })
    },
    [result, selectedIndex, refreshPreview],
  )

  // Carrier/ship-to/manufacturer/internal-use values are constant for the
  // whole shipment (only the barcode differs label to label), so filling
  // them in once and broadcasting is the realistic workflow rather than
  // repeating the same values 35 times.
  const handleApplyCarrierDataToAll = useCallback(async () => {
    if (!result || result.templateKind !== 'CartonShippingCarrierPanel') return
    const current = labels[selectedIndex] as CarrierLabelData
    const updated = (labels as CarrierLabelData[]).map((l) => ({
      ...current,
      barcode: l.barcode,
    }))
    setLabels(updated)
    try {
      const svgs = await previewCarrier(updated)
      setPreviews(svgs)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }, [result, labels, selectedIndex])

  const handleDownloadHangtagSheet = useCallback(async () => {
    setIsDownloadingSheet(true)
    setError(null)
    try {
      await renderHangtagSheetPdf(labels as HangtagLabelData[])
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    } finally {
      setIsDownloadingSheet(false)
    }
  }, [labels])

  const hasSheetCarrierData = Object.entries(sheetCarrierData).some(
    ([key, value]) => key !== 'barcode' && typeof value === 'string' && value.trim() !== '',
  )

  const handleDownloadCartonSheet = useCallback(async () => {
    if (!hasSheetCarrierData) return
    setIsDownloadingSheet(true)
    setError(null)
    try {
      const gtinLabels = labels as CartonGtinLabelData[]
      const carrierLabels: CarrierLabelData[] = gtinLabels.map((l) => ({
        ...sheetCarrierData,
        barcode: l.barcode,
      }))
      await renderCartonSheetPdf(gtinLabels, carrierLabels)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    } finally {
      setIsDownloadingSheet(false)
    }
  }, [labels, sheetCarrierData, hasSheetCarrierData])

  const handleDownload = useCallback(async () => {
    if (!result) return
    setIsDownloading(true)
    setError(null)
    try {
      if (result.templateKind === 'HangtagSticker') {
        await renderHangtagPdf(labels as HangtagLabelData[])
      } else if (result.templateKind === 'CartonShippingGtinPanel') {
        await renderCartonGtinPdf(labels as CartonGtinLabelData[])
      } else if (result.templateKind === 'CartonShippingCarrierPanel') {
        await renderCarrierPdf(labels as CarrierLabelData[])
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    } finally {
      setIsDownloading(false)
    }
  }, [result, labels])

  return (
    <div className="app">
      <header className="app__header">
        <h1>Etiqueta Andrómeda</h1>
        <p>PRN → datos → plantilla fija → PDF</p>
      </header>

      <div className="app__toolbar">
        <label className="app__po-input">
          <span>PO number (opcional — reemplaza el que trae el PRN, solo panel GTIN de cartón)</span>
          <input
            type="text"
            value={poNumber}
            onChange={(e) => setPoNumber(e.target.value)}
            placeholder="ej. 134876"
          />
        </label>
      </div>

      <UploadZone onFileSelected={handleFile} disabled={isLoading} />

      {error && <p className="app__error">{error}</p>}
      {isLoading && <p className="app__status">Procesando PRN…</p>}

      {result && (
        <div className="app__workspace">
          <aside className="app__sidebar">
            <h2>{TEMPLATE_NAMES[result.templateKind] ?? result.templateKind}</h2>
            <p className="app__count">{labels.length} etiqueta(s)</p>
            <LabelList labels={labels} selectedIndex={selectedIndex} onSelect={setSelectedIndex} />
          </aside>

          <main className="app__main">
            <div className="app__preview">
              <LabelPreview svg={previews[selectedIndex] ?? ''} />
            </div>

            <div className="app__editor">
              {result.templateKind === 'HangtagSticker' && (
                <>
                  <LabelFieldsForm
                    data={labels[selectedIndex] as HangtagLabelData}
                    fields={HANGTAG_FIELDS}
                    onChange={handleFieldsChange}
                  />
                  <button
                    type="button"
                    className="app__secondary"
                    onClick={handleDownloadHangtagSheet}
                    disabled={isDownloadingSheet}
                  >
                    {isDownloadingSheet ? 'Generando…' : 'Descargar hoja de aprobación'}
                  </button>
                </>
              )}
              {result.templateKind === 'CartonShippingGtinPanel' && (
                <>
                  <LabelFieldsForm
                    data={labels[selectedIndex] as CartonGtinLabelData}
                    fields={CARTON_GTIN_FIELDS}
                    onChange={handleFieldsChange}
                  />
                  <p className="app__notice">
                    La hoja de aprobación combina el panel GTIN con el panel Carrier/Ship-To en
                    una sola página por cartón — completa aquí los datos de envío compartidos
                    (igual que en el panel Carrier) para emparejarlos por código de barras.
                  </p>
                  <LabelFieldsForm
                    data={sheetCarrierData}
                    fields={CARRIER_FIELDS.filter((f) => f.key !== 'barcode')}
                    onChange={setSheetCarrierData}
                  />
                  {!hasSheetCarrierData && (
                    <p className="app__error">
                      Completa al menos un dato de envío arriba para poder generar la hoja de
                      aprobación — el panel Carrier saldría en blanco si no.
                    </p>
                  )}
                  <button
                    type="button"
                    className="app__secondary"
                    onClick={handleDownloadCartonSheet}
                    disabled={isDownloadingSheet || !hasSheetCarrierData}
                  >
                    {isDownloadingSheet ? 'Generando…' : 'Descargar hoja de aprobación'}
                  </button>
                </>
              )}
              {result.templateKind === 'CartonShippingCarrierPanel' && (
                <>
                  <p className="app__notice">
                    El transportista, la dirección de destino y los códigos no vienen como texto en
                    el PRN (están incrustados como imagen) — complétalos aquí una vez y aplícalos a
                    las {labels.length} etiquetas del envío.
                  </p>
                  <LabelFieldsForm
                    data={labels[selectedIndex] as CarrierLabelData}
                    fields={CARRIER_FIELDS}
                    onChange={handleFieldsChange}
                  />
                  <button type="button" className="app__secondary" onClick={handleApplyCarrierDataToAll}>
                    Aplicar estos valores a las {labels.length} etiquetas
                  </button>
                </>
              )}

              <button type="button" className="app__download" onClick={handleDownload} disabled={isDownloading}>
                {isDownloading ? 'Generando…' : `Descargar PDF (${labels.length} etiquetas)`}
              </button>
            </div>
          </main>
        </div>
      )}
    </div>
  )
}

export default App
