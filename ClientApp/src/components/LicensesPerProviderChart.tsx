import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, Legend, ResponsiveContainer,
} from 'recharts'
import type { LicensePerProvider } from '../types'

interface Props { data: LicensePerProvider[] }

export default function LicensesPerProviderChart({ data }: Props) {
  // Truncate long names for x-axis legibility
  const truncate = (name: string) =>
    name.length > 18 ? name.slice(0, 16) + '…' : name

  const formatted = data.map(d => ({ ...d, shortName: truncate(d.providerName) }))

  return (
    <div className="bg-white rounded-xl shadow-sm p-6">
      <h3 className="text-base font-semibold text-slate-800 mb-1">Licenses per Provider</h3>
      <p className="text-xs text-slate-400 mb-4">
        Stacked view of active vs expired licenses by provider (soft-deleted providers excluded)
      </p>
      <ResponsiveContainer width="100%" height={300}>
        <BarChart
          data={formatted}
          margin={{ top: 5, right: 20, left: 0, bottom: 60 }}
        >
          <CartesianGrid strokeDasharray="3 3" stroke="#E2E8F0" />
          <XAxis
            dataKey="shortName"
            tick={{ fontSize: 11, fill: '#64748b' }}
            angle={-35}
            textAnchor="end"
            interval={0}
          />
          <YAxis tick={{ fontSize: 12, fill: '#64748b' }} allowDecimals={false} />
          <Tooltip
            labelFormatter={(label: string) => {
              const full = data.find(d => truncate(d.providerName) === label)?.providerName ?? label
              return full
            }}
            contentStyle={{ borderRadius: 8, border: '1px solid #e2e8f0' }}
          />
          <Legend wrapperStyle={{ paddingTop: 12 }} />
          <Bar dataKey="active" name="Active" stackId="a" fill="#276749" radius={[0, 0, 0, 0]} />
          <Bar dataKey="expired" name="Expired" stackId="a" fill="#C53030" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
