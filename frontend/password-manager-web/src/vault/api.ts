import type {
  CreateVaultEntryPayload,
  FolderResponse,
  UpdateVaultEntryPayload,
  VaultEntryResponse,
} from './types';

const BASE_URL = '/api/v1/vault';

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
      // Non-JSON response body
    }
    throw new Error(errorDetail);
  }
  return res.json() as Promise<T>;
}

export const vaultApi = {
  async getEntries(folderId?: string, includeDeleted = false): Promise<VaultEntryResponse[]> {
    const params = new URLSearchParams();
    if (folderId) params.append('folderId', folderId);
    if (includeDeleted) params.append('includeDeleted', 'true');

    const url = `${BASE_URL}/entries${params.toString() ? `?${params.toString()}` : ''}`;
    const res = await fetch(url, {
      method: 'GET',
      headers: DEFAULT_HEADERS,
      credentials: 'include',
    });
    return handleResponse<VaultEntryResponse[]>(res);
  },

  async getEntryById(id: string): Promise<VaultEntryResponse> {
    const res = await fetch(`${BASE_URL}/entries/${id}`, {
      method: 'GET',
      headers: DEFAULT_HEADERS,
      credentials: 'include',
    });
    return handleResponse<VaultEntryResponse>(res);
  },

  async createEntry(payload: CreateVaultEntryPayload): Promise<VaultEntryResponse> {
    const res = await fetch(`${BASE_URL}/entries`, {
      method: 'POST',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify(payload),
      credentials: 'include',
    });
    return handleResponse<VaultEntryResponse>(res);
  },

  async updateEntry(id: string, payload: UpdateVaultEntryPayload): Promise<VaultEntryResponse> {
    const res = await fetch(`${BASE_URL}/entries/${id}`, {
      method: 'PUT',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify(payload),
      credentials: 'include',
    });
    return handleResponse<VaultEntryResponse>(res);
  },

  async moveEntry(id: string, folderId: string | null): Promise<VaultEntryResponse> {
    const res = await fetch(`${BASE_URL}/entries/${id}/move`, {
      method: 'PUT',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify({ folderId }),
      credentials: 'include',
    });
    return handleResponse<VaultEntryResponse>(res);
  },

  async softDeleteEntry(id: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/entries/${id}`, {
      method: 'DELETE',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
      credentials: 'include',
    });
    if (!res.ok) {
      throw new Error(`Failed to delete entry: ${res.statusText}`);
    }
  },

  async restoreEntry(id: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/entries/${id}/restore`, {
      method: 'POST',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
      credentials: 'include',
    });
    if (!res.ok) {
      throw new Error(`Failed to restore entry: ${res.statusText}`);
    }
  },

  async purgeEntry(id: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/entries/${id}/purge`, {
      method: 'DELETE',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
      credentials: 'include',
    });
    if (!res.ok) {
      throw new Error(`Failed to purge entry: ${res.statusText}`);
    }
  },

  async getFolders(): Promise<FolderResponse[]> {
    const res = await fetch(`${BASE_URL}/folders`, {
      method: 'GET',
      headers: DEFAULT_HEADERS,
      credentials: 'include',
    });
    return handleResponse<FolderResponse[]>(res);
  },

  async getFolderById(id: string): Promise<FolderResponse> {
    const res = await fetch(`${BASE_URL}/folders/${id}`, {
      method: 'GET',
      headers: DEFAULT_HEADERS,
      credentials: 'include',
    });
    return handleResponse<FolderResponse>(res);
  },

  async createFolder(payload: { encryptedName: string; nonce: string }): Promise<FolderResponse> {
    const res = await fetch(`${BASE_URL}/folders`, {
      method: 'POST',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify(payload),
      credentials: 'include',
    });
    return handleResponse<FolderResponse>(res);
  },

  async updateFolder(
    id: string,
    payload: { encryptedName: string; nonce: string }
  ): Promise<FolderResponse> {
    const res = await fetch(`${BASE_URL}/folders/${id}`, {
      method: 'PUT',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify(payload),
      credentials: 'include',
    });
    return handleResponse<FolderResponse>(res);
  },

  async deleteFolder(id: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/folders/${id}`, {
      method: 'DELETE',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
      credentials: 'include',
    });
    if (!res.ok) {
      throw new Error(`Failed to delete folder: ${res.statusText}`);
    }
  },
};
