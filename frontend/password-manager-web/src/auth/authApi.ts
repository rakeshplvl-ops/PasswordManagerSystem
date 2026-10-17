import type { InitialVaultBundle } from '../crypto/types';

export interface UserResponse {
  id: string;
  email: string;
  createdAt?: string;
  lastLoginAt?: string | null;
  hasVaultSettings: boolean;
}

export interface AuthResponse {
  userId: string;
  email: string;
}

const BASE_URL = '/api/v1/auth';

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

export const authApi = {
  async register(
    email: string,
    password: string,
    initialBundle?: InitialVaultBundle
  ): Promise<AuthResponse> {
    const payload = {
      email,
      password,
      vaultSettings: initialBundle
        ? {
            encryptedVek: initialBundle.encryptedVek,
            vekNonce: initialBundle.vekNonce,
            kdfAlgorithm: initialBundle.kdfParams.algorithm,
            kdfIterations: initialBundle.kdfParams.iterations,
            kdfMemoryKb: initialBundle.kdfParams.memoryKb,
            kdfParallelism: initialBundle.kdfParams.parallelism,
            kdfSalt: initialBundle.kdfParams.salt,
            schemaVersion: 1,
          }
        : null,
    };

    const res = await fetch(`${BASE_URL}/register`, {
      method: 'POST',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify(payload),
      credentials: 'include',
    });

    return handleResponse<AuthResponse>(res);
  },

  async login(email: string, password: string): Promise<AuthResponse> {
    const res = await fetch(`${BASE_URL}/login`, {
      method: 'POST',
      headers: DEFAULT_HEADERS,
      body: JSON.stringify({ email, password }),
      credentials: 'include',
    });

    return handleResponse<AuthResponse>(res);
  },

  async logout(): Promise<void> {
    const res = await fetch(`${BASE_URL}/logout`, {
      method: 'POST',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
      credentials: 'include',
    });
    if (!res.ok) {
      throw new Error(`Logout failed with status ${res.status}`);
    }
  },

  async getCurrentUser(): Promise<UserResponse | null> {
    const res = await fetch(`${BASE_URL}/me`, {
      method: 'GET',
      headers: DEFAULT_HEADERS,
      credentials: 'include',
    });

    if (res.status === 401) {
      return null;
    }

    return handleResponse<UserResponse>(res);
  },
};
