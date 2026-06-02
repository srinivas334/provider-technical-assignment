export interface ProviderStatusCount {
  status: string
  count: number
}

export interface LicenseStatusCount {
  status: string
  count: number
}

export interface LicensePerProvider {
  providerName: string
  total: number
  active: number
  expired: number
}

export interface ExpiringLicense {
  providerName: string
  county: string
  licenseNumber: string
  licenseStatus: string
  expirationDate: string
  daysUntilExpiry: number
}

export interface DashboardStats {
  totalProviders: number
  activeProviders: number
  inactiveProviders: number
  pendingProviders: number
  softDeletedProviders: number
  totalLicenses: number
  activeLicenses: number
  expiredLicenses: number
  suspendedLicenses: number
  providersByStatus: ProviderStatusCount[]
  licensesByStatus: LicenseStatusCount[]
  licensesPerProvider: LicensePerProvider[]
  expiringWithin90Days: ExpiringLicense[]
}
