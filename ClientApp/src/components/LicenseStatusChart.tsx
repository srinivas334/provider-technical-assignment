import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts'
import type { LicenseStatusCount } from '../types'

interface Props { data: LicenseStatusCount[] }

const COLORS: Record<string, string> = {
  Active: '#276749',
  Expired: '#C53030',
  Suspended: '#C8A951',
}

const DEFAULT_COLOR = '#003366'

export default function LicenseStatusChart({ data }: Props) {
  return (
    <div className="bg-white rounded-xl shadow-sm p-6">
      <h3 className="text-base font-semibold text-slate-800 mb-1">License Status Breakdown</h3>
      <p className="text-xs text-slate-400 mb-4">Count of licenses by current status</p>
      <ResponsiveContainer width="100%" height={260}>
        <BarChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#E2E8F0" />
          <XAxis dataKey="status" tick={{ fontSize: 12, fill: '#64748b' }} />
          <YAxis tick={{ fontSize: 12, fill: '#64748b' }} allowDecimals={false} />
          <Tooltip
            formatter={(value: number) => [`${value} licenses`, 'Count']}
            contentStyle={{ borderRadius: 6, border: '1px solid #CBD5E0', color: '#1D2636' }}
          />
          <Bar dataKey="count" radius={[4, 4, 0, 0]}>
            {data.map(entry => (
              <Cell key={entry.status} fill={COLORS[entry.status] ?? DEFAULT_COLOR} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
