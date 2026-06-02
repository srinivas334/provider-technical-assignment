import { Building2, CheckCircle2, Clock, FileCheck, FileX, Trash2 } from 'lucide-react'
import type { DashboardStats } from '../types'

interface Props { stats: DashboardStats }

interface CardProps {
  icon: React.ReactNode
  label: string
  value: number
  sub?: string
  accent: string
  iconBg: string
}

function Card({ icon, label, value, sub, accent, iconBg }: CardProps) {
  return (
    <div className={`bg-white rounded-xl shadow-sm border-l-4 ${accent} p-5 flex items-center gap-4`}>
      <div className={`${iconBg} rounded-lg p-3 flex-shrink-0`}>
        {icon}
      </div>
      <div>
        <p className="text-sm font-medium text-[#718096]">{label}</p>
        <p className="text-3xl font-bold text-[#1D2636] leading-tight">{value.toLocaleString()}</p>
        {sub && <p className="text-xs text-[#718096] mt-0.5">{sub}</p>}
      </div>
    </div>
  )
}

export default function SummaryCards({ stats }: Props) {
  return (
    <section>
      <h2 className="text-sm font-semibold uppercase tracking-widest text-slate-400 mb-4">Overview</h2>
      <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-6 gap-4">
        <Card
          icon={<Building2 size={22} className="text-[#003366]" />}
          label="Total Providers"
          value={stats.totalProviders}
          sub="Excluding soft-deleted"
          accent="border-[#003366]"
          iconBg="bg-blue-50"
        />
        <Card
          icon={<CheckCircle2 size={22} className="text-[#276749]" />}
          label="Active"
          value={stats.activeProviders}
          accent="border-[#276749]"
          iconBg="bg-green-50"
        />
        <Card
          icon={<Clock size={22} className="text-[#A08430]" />}
          label="Pending"
          value={stats.pendingProviders}
          accent="border-[#C8A951]"
          iconBg="bg-amber-50"
        />
        <Card
          icon={<FileCheck size={22} className="text-[#003366]" />}
          label="Active Licenses"
          value={stats.activeLicenses}
          sub={`of ${stats.totalLicenses} total`}
          accent="border-[#003366]"
          iconBg="bg-blue-50"
        />
        <Card
          icon={<FileX size={22} className="text-[#C53030]" />}
          label="Expired Licenses"
          value={stats.expiredLicenses}
          accent="border-[#C53030]"
          iconBg="bg-red-50"
        />
        <Card
          icon={<Trash2 size={22} className="text-slate-400" />}
          label="Soft-Deleted"
          value={stats.softDeletedProviders}
          sub="Audit trail only"
          accent="border-slate-300"
          iconBg="bg-slate-100"
        />
      </div>
    </section>
  )
}
