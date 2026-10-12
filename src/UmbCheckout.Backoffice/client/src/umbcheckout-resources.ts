const MANAGEMENT_API_BASE = '/umbraco/management/api/v1/umbcheckout';
const LICENSING_API_BASE = '/umbraco/management/api/v1/umbhost-licensing';

export interface LicenseStatus {
  status: string;
  isDevelopment: boolean;
  registrationDate: string;
  expiryDate: string | null;
  validDomains: string[];
  validPaths: string[];
  addons: Array<{ name: string; status: string }>;
}

export interface ConfigProperty {
  alias: string;
  label: string;
  description: string;
  value: any;
  view: string;
  validation: {
    mandatory: boolean;
    mandatoryMessage?: string;
    pattern?: string;
    patternMessage?: string;
  };
  config?: Record<string, any>;
}

async function fetchJson<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  });
  if (!response.ok) throw new Error(`HTTP ${response.status}`);
  return response.json();
}

export async function getLicenseStatus(): Promise<LicenseStatus> {
  return fetchJson(`${LICENSING_API_BASE}/license-status`);
}

export async function getConfiguration(): Promise<ConfigProperty[]> {
  return fetchJson(`${MANAGEMENT_API_BASE}/configuration`);
}

export async function getHasUmbracoApplicationUrlSet(): Promise<boolean> {
  return fetchJson(`${MANAGEMENT_API_BASE}/configuration/has-url-set`);
}

export async function updateConfiguration(values: Record<string, any>): Promise<void> {
  await fetch(`${MANAGEMENT_API_BASE}/configuration`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(values),
  });
}

export async function checkLicense(): Promise<void> {
  await fetchJson(`${LICENSING_API_BASE}/licence-check`);
}
