import { LayoutDashboard } from 'lucide-react'

export default function Header() {
  return (
    <header className="bg-[#003366] text-white shadow-lg">
      <div className="bg-[#1A1A1A] text-white text-xs py-1 px-4 flex justify-between border-b-2 border-[#C8A951]">
        <span></span>
        <a
          href="http://localhost:5236/Providers"
          className="text-white/70 hover:text-[#C8A951] transition-colors"
        >
          Provider Management
        </a>
      </div>

      <div className="py-3 px-4 flex items-center gap-4">
        <div className="bg-[#C8A951] rounded-lg p-2.5 flex-shrink-0">
          <LayoutDashboard size={24} className="text-[#003366]" />
        </div>
        <div>
          <h1 className="text-xl font-bold tracking-tight">Provider &amp; License Dashboard</h1>
          <p className="text-sm text-white/70">Department of Early Care and Learning — Analytics</p>
        </div>
      </div>

      <div className="bg-[#0A4580] border-t border-white/15 px-4 py-1">
        <a
          href="http://localhost:5236/Providers"
          className="text-xs text-white/80 hover:text-[#C8A951] transition-colors mr-6"
        >
          ← Provider Management
        </a>
        <span className="text-xs text-white/40">Read-only analytics view</span>
      </div>
    </header>
  )
}
