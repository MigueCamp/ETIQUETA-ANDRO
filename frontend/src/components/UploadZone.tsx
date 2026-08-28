import { useCallback, useRef, useState } from 'react'
import type { DragEvent } from 'react'

interface Props {
  onFileSelected: (file: File) => void
  disabled?: boolean
}

export function UploadZone({ onFileSelected, disabled }: Props) {
  const [isDragOver, setIsDragOver] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  const handleDrop = useCallback(
    (event: DragEvent<HTMLDivElement>) => {
      event.preventDefault()
      setIsDragOver(false)
      if (disabled) return
      const file = event.dataTransfer.files[0]
      if (file) onFileSelected(file)
    },
    [disabled, onFileSelected],
  )

  return (
    <div
      className={`upload-zone${isDragOver ? ' upload-zone--active' : ''}${disabled ? ' upload-zone--disabled' : ''}`}
      onDragOver={(e) => {
        e.preventDefault()
        if (!disabled) setIsDragOver(true)
      }}
      onDragLeave={() => setIsDragOver(false)}
      onDrop={handleDrop}
      onClick={() => !disabled && inputRef.current?.click()}
      role="button"
      tabIndex={0}
      aria-disabled={disabled}
    >
      <input
        ref={inputRef}
        type="file"
        accept=".prn,.txt"
        hidden
        disabled={disabled}
        onChange={(e) => {
          const file = e.target.files?.[0]
          if (file) onFileSelected(file)
          e.target.value = ''
        }}
      />
      <p className="upload-zone__title">Arrastra un archivo .PRN aquí</p>
      <p className="upload-zone__hint">o haz clic para seleccionarlo</p>
    </div>
  )
}
