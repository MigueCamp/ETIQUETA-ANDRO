import type { FieldDef } from '../api/types'

interface Props<T> {
  data: T
  fields: FieldDef<T>[]
  onChange: (next: T) => void
}

/**
 * Plain text inputs for the values a template exposes as editable — nothing
 * here can touch position, font, color, or any other design property. The
 * set and order of fields is defined once per template in api/types.ts.
 */
export function LabelFieldsForm<T extends object>({ data, fields, onChange }: Props<T>) {
  return (
    <div className="fields-form">
      {fields.map((field) => {
        const key = field.key
        const rawValue = data[key]
        const value = rawValue === null || rawValue === undefined ? '' : String(rawValue)
        return (
          <label key={String(key)} className="fields-form__row">
            <span className="fields-form__label">{field.label}</span>
            <input
              className="fields-form__input"
              type="text"
              value={value}
              onChange={(e) => onChange({ ...data, [key]: e.target.value } as T)}
            />
          </label>
        )
      })}
    </div>
  )
}
