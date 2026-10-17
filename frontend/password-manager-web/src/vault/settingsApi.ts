import type { VaultSettings } from '../crypto/types';

const BASE_URL = '/api/v1/vault/settings';

const DEFAULT_HEADERS: Record<string, string> = {
  'Content-Type': 'application/json',
  'X-Requested-With': 'XMLHttpRequest',
};

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    let errorDetail = `Request failed with status ${res.status}`;
    try {
      const problem = await res.json();
      errorDetail = problem.detail || problem.title || errorDetail;
    } catch {
      // Body not JSON
    }
    throw new Error(errorDetail);
  }
  return res.json() as Promise<T>;
}

export const settingsApi = {
  async getSettings(): Promise<VaultSettings | null> {
    const res = await fetch(BASE_URL, {
      method: 'GET',
      headers: DEFAULT_HEADERS,
      credentials: 'include',
    });

    if (res.status === 404) {
      return null;
    }

    return handleResponse<VaultSettings>(res);
  },

  async updateSettings(settings: Omit<VaultSettings, 'userId' | 'updatedAt'>): Promise<VaultSettings> {
    const res = await fetch(BASE_URL, {
      method: 'PUT',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify(settings),
      credentials: 'include',
    });

    return handleResponse<VaultSettings>(res);
  },
};
