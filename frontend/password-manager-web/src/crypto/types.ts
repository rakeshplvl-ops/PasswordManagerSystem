export interface KdfParams {
  algorithm: 'Argon2id';
  iterations: number;
  memoryKb: number;
  parallelism: number;
  salt: string; // Base64 encoded
  keyLengthBytes?: number; // Defaults to 32 bytes (256-bit)
}

export interface EncryptedData {
  ciphertext: string; // Base64 encoded
  nonce: string; // Base64 encoded (12 bytes / 96 bits for AES-GCM)
}

export interface VaultSettings {
  userId?: string;
  encryptedVek: string;
  vekNonce: string;
  kdfAlgorithm: string;
  kdfIterations: number;
  kdfMemoryKb: number;
  kdfParallelism: number;
  kdfSalt: string;
  schemaVersion: number;
  updatedAt?: string;
}

export interface CredentialPayload {
  title: string;
  username: string;
  password: string;
  website?: string;
  notes?: string;
  folderId?: string | null;
  customFields?: Record<string, string>;
  isFavorite?: boolean;
}

export interface InitialVaultBundle {
  vek: CryptoKey;
  kek: CryptoKey;
  encryptedVek: string;
  vekNonce: string;
  kdfParams: KdfParams;
}

export const DEFAULT_KDF_PARAMS: Omit<KdfParams, 'salt'> = {
  algorithm: 'Argon2id',
  iterations: 3,
  memoryKb: 65536, // 64 MB
  parallelism: 4,
  keyLengthBytes: 32, // 256 bits
};
