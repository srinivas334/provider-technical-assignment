import { useEffect, useState } from 'react'
import { AlertCircle, Loader2 } from 'lucide-react'
import Header from './components/Header'
import SummaryCards from './components/SummaryCards'
import ProviderStatusChart from './components/ProviderStatusChart'
import LicenseStatusChart from './components/LicenseStatusChart'
import LicensesPerProviderChart from './components/LicensesPerProviderChart'
import ExpiringLicensesTable from './components/ExpiringLicensesTable'
import type { DashboardStats } from './types'

export default function App() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch('/api/dashboard')
      .then(r => {
        if (!r.ok) throw new Error(`API returned ${r.status}`)
        return r.json() as Promise<DashboardStats>
      })
      .then(data => { setStats(data); setLoading(false) })
      .catch(e => { setError(String(e)); setLoading(false) })
  }, [])

  if (loading) {
    return (
      <div className="min-h-screen bg-[#F4F6F9] flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-500">
          <Loader2 className="animate-spin" size={40} />
          <span className="text-lg font-medium">Loading dashboard…</span>
        </div>
      </div>
    )
  }

  if (error || !stats) {
    return (
      <div className="min-h-screen bg-[#F4F6F9] flex items-center justify-center">
        <div className="bg-white rounded-xl shadow p-8 max-w-md text-center">
          <AlertCircle size={48} className="text-red-500 mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-slate-800 mb-2">Could not load dashboard</h2>
          <p className="text-slate-500 text-sm mb-4">{error}</p>
          <p className="text-slate-400 text-xs">
            Make sure the ASP.NET backend is running on <code className="bg-slate-100 px-1 rounded">http://localhost:5236</code>
          </p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-[#F4F6F9]">
      <Header />

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
        <SummaryCards stats={stats} />

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <ProviderStatusChart data={stats.providersByStatus} />
          <LicenseStatusChart data={stats.licensesByStatus} />
        </div>

        <LicensesPerProviderChart data={stats.licensesPerProvider} />

        <ExpiringLicensesTable
          licenses={stats.expiringWithin90Days}
          totalExpired={stats.expiredLicenses}
        />
      </main>

      <footer className="bg-[#003366] text-white py-6 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col sm:flex-row justify-between items-center gap-2">
          <span className="text-sm font-medium text-white/80"> Provider Management System</span>
          <span className="text-xs text-white/50"> Department of Early Care and Learning</span>
        </div>
      </footer>
    </div>
  )
}
