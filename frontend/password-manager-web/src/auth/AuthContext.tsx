import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react';
import { KdfService } from '../crypto/kdfService';
import { VekService } from '../crypto/vekService';
import { settingsApi } from '../vault/settingsApi';
import { useVaultStore } from '../vault/vaultStore';
import { authApi, type UserResponse } from './authApi';

interface AuthContextType {
  user: UserResponse | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, masterPassword: string) => Promise<void>;
  register: (email: string, masterPassword: string) => Promise<void>;
  logout: () => Promise<void>;
  unlockVault: (masterPassword: string) => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const setVek = useVaultStore((state) => state.setVek);
  const lockVault = useVaultStore((state) => state.lockVault);

  const refreshUser = useCallback(async () => {
    try {
      const currentUser = await authApi.getCurrentUser();
      setUser(currentUser);
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    refreshUser();
  }, [refreshUser]);

  const unlockVault = async (masterPassword: string) => {
    const settings = await settingsApi.getSettings();
    if (!settings) {
      throw new Error('No vault settings found for this account.');
    }

    // Derive KEK via client-side Argon2id
    const kek = await KdfService.deriveKek(masterPassword, {
      algorithm: 'Argon2id',
      iterations: settings.kdfIterations,
      memoryKb: settings.kdfMemoryKb,
      parallelism: settings.kdfParallelism,
      salt: settings.kdfSalt,
    });

    // Unwrap VEK using KEK
    const vek = await VekService.unwrapVek(
      settings.encryptedVek,
      settings.vekNonce,
      kek
    );

    // Save exclusively in memory
    setVek(vek);
  };

  const login = async (email: string, masterPassword: string) => {
    setIsLoading(true);
    try {
      await authApi.login(email, masterPassword);
      const currentUser = await authApi.getCurrentUser();
      setUser(currentUser);

      // Attempt to unlock vault immediately with master password
      if (currentUser?.hasVaultSettings) {
        try {
          await unlockVault(masterPassword);
        } catch (err) {
          console.warn('Auto-unlock failed after login, manual unlock required:', err);
        }
      }
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (email: string, masterPassword: string) => {
    setIsLoading(true);
    try {
      // 1. Initialize vault bundle client-side (Zero-Knowledge)
      const bundle = await VekService.initializeVault(masterPassword);

      // 2. Register account and send encrypted VEK + KDF parameters to server
      await authApi.register(email, masterPassword, bundle);

      // 3. Set user state and unlock in-memory vault immediately
      const currentUser = await authApi.getCurrentUser();
      setUser(currentUser);
      setVek(bundle.vek);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } finally {
      lockVault(); // Wipe VEK from memory
      setUser(null);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login,
        register,
        logout,
        unlockVault,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
