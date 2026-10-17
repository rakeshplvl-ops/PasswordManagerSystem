import type { CredentialPayload } from '../crypto/types';

export interface VaultEntryResponse {
  id: string;
  folderId?: string | null;
  ciphertext: string;
  nonce: string;
  schemaVersion: number;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
}

export interface DecryptedVaultEntry {
  id: string;
  folderId?: string | null;
  schemaVersion: number;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
  data: CredentialPayload;
}

export interface FolderResponse {
  id: string;
  encryptedName: string;
  nonce: string;
  createdAt: string;
  updatedAt: string;
}

export interface DecryptedFolder {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateVaultEntryPayload {
  ciphertext: string;
  nonce: string;
  folderId?: string | null;
  schemaVersion?: number;
}

export interface UpdateVaultEntryPayload {
  ciphertext: string;
  nonce: string;
  folderId?: string | null;
  schemaVersion?: number;
}
