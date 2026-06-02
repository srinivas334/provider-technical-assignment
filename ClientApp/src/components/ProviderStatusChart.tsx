import { PieChart, Pie, Cell, Legend, Tooltip, ResponsiveContainer } from 'recharts'
import type { ProviderStatusCount } from '../types'

interface Props { data: ProviderStatusCount[] }

const COLORS: Record<string, string> = {
  Active: '#276749',
  Inactive: '#4A5568',
  Pending: '#C8A951',
}

const DEFAULT_COLOR = '#003366'

export default function ProviderStatusChart({ data }: Props) {
  return (
    <div className="bg-white rounded-xl shadow-sm p-6">
      <h3 className="text-base font-semibold text-slate-800 mb-1">Providers by Status</h3>
      <p className="text-xs text-slate-400 mb-4">Distribution of active provider lifecycle states</p>
      <ResponsiveContainer width="100%" height={260}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            innerRadius={65}
            outerRadius={100}
            paddingAngle={3}
            dataKey="count"
            nameKey="status"
            label={({ status, percent }) =>
              `${status} ${(percent * 100).toFixed(0)}%`
            }
            labelLine={false}
          >
            {data.map(entry => (
              <Cell
                key={entry.status}
                fill={COLORS[entry.status] ?? DEFAULT_COLOR}
              />
            ))}
          </Pie>
          <Tooltip
            formatter={(value: number) => [`${value} providers`, '']}
          />
          <Legend
            formatter={value => (
              <span className="text-sm text-slate-600">{value}</span>
            )}
          />
        </PieChart>
      </ResponsiveContainer>
    </div>
  )
}
