import { AlertTriangle, CheckCircle2 } from 'lucide-react'
import type { ExpiringLicense } from '../types'

interface Props {
  licenses: ExpiringLicense[]
  totalExpired: number
}

function urgencyClass(days: number): string {
  if (days <= 0) return 'bg-red-50 text-red-800'
  if (days <= 30) return 'bg-orange-50 text-orange-800'
  return 'bg-amber-50 text-amber-800'
}

function urgencyBadge(days: number) {
  if (days <= 0) return <span className="text-xs font-semibold text-red-600">EXPIRED</span>
  if (days <= 30) return <span className="text-xs font-semibold text-orange-600">{days}d left</span>
  return <span className="text-xs font-medium text-amber-600">{days}d left</span>
}

export default function ExpiringLicensesTable({ licenses, totalExpired }: Props) {
  return (
    <div className="bg-white rounded-xl shadow-sm p-6">
      <div className="flex items-start justify-between mb-4">
        <div>
          <h3 className="text-base font-semibold text-slate-800">
            Licenses Expiring Within 90 Days
          </h3>
          <p className="text-xs text-slate-400 mt-0.5">
            Providers may remain Active while licenses are expired — action may be required
          </p>
        </div>
        {totalExpired > 0 && (
          <div className="flex items-center gap-1.5 bg-red-50 text-red-700 text-xs font-semibold px-3 py-1.5 rounded-full">
            <AlertTriangle size={13} />
            {totalExpired} total expired
          </div>
        )}
      </div>

      {licenses.length === 0 ? (
        <div className="flex items-center gap-2 text-emerald-600 bg-emerald-50 rounded-lg px-4 py-3">
          <CheckCircle2 size={18} />
          <span className="text-sm font-medium">No licenses expiring in the next 90 days.</span>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-100">
                <th className="text-left py-2 px-3 font-semibold text-[#4A5568] text-xs uppercase tracking-wide bg-[#F7FAFC]">Provider</th>
                <th className="text-left py-2 px-3 font-semibold text-[#4A5568] text-xs uppercase tracking-wide bg-[#F7FAFC]">County</th>
                <th className="text-left py-2 px-3 font-semibold text-[#4A5568] text-xs uppercase tracking-wide bg-[#F7FAFC]">License #</th>
                <th className="text-left py-2 px-3 font-semibold text-[#4A5568] text-xs uppercase tracking-wide bg-[#F7FAFC]">Status</th>
                <th className="text-left py-2 px-3 font-semibold text-[#4A5568] text-xs uppercase tracking-wide bg-[#F7FAFC]">Expires</th>
                <th className="text-left py-2 px-3 font-semibold text-[#4A5568] text-xs uppercase tracking-wide bg-[#F7FAFC]">Urgency</th>
              </tr>
            </thead>
            <tbody>
              {licenses.map((lic, i) => (
                <tr
                  key={i}
                  className={`border-b border-slate-50 ${urgencyClass(lic.daysUntilExpiry)}`}
                >
                  <td className="py-2.5 px-3 font-medium">{lic.providerName}</td>
                  <td className="py-2.5 px-3">{lic.county}</td>
                  <td className="py-2.5 px-3 font-mono text-xs">{lic.licenseNumber}</td>
                  <td className="py-2.5 px-3">{lic.licenseStatus}</td>
                  <td className="py-2.5 px-3">{new Date(lic.expirationDate).toLocaleDateString()}</td>
                  <td className="py-2.5 px-3">{urgencyBadge(lic.daysUntilExpiry)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
