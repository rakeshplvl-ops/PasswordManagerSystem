import { create } from 'zustand';

interface VaultSessionState {
  vek: CryptoKey | null;
  isUnlocked: boolean;
  setVek: (key: CryptoKey) => void;
  lockVault: () => void;
}

/**
 * Transient in-memory vault session store.
 * SECURITY INVARIANT: The Vault Encryption Key (VEK) is NEVER persisted
 * to localStorage, sessionStorage, or IndexedDB. It exists exclusively in
 * memory while the vault session is active and unlocked.
 */
export const useVaultStore = create<VaultSessionState>((set) => ({
  vek: null,
  isUnlocked: false,
  setVek: (key: CryptoKey) => set({ vek: key, isUnlocked: true }),
  lockVault: () => set({ vek: null, isUnlocked: false }),
}));
