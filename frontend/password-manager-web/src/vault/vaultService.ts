import { VaultCipher } from '../crypto/vaultCipher';
import type { CredentialPayload } from '../crypto/types';
import type {
  CreateVaultEntryPayload,
  DecryptedFolder,
  DecryptedVaultEntry,
  FolderResponse,
  UpdateVaultEntryPayload,
  VaultEntryResponse,
} from './types';

/**
 * Vault Service provides client-side encryption before sending payloads
 * to the API, and transparent decryption after receiving them.
 */
export class VaultService {
  /**
   * Encrypts a plaintext credential payload using the session VEK
   * and prepares a CreateVaultEntryPayload for API submission.
   */
  public static async encryptAndPrepareCreate(
    payload: CredentialPayload,
    vek: CryptoKey,
    folderId?: string | null,
    schemaVersion = 1
  ): Promise<CreateVaultEntryPayload> {
    const encrypted = await VaultCipher.encryptCredential(payload, vek);

    return {
      ciphertext: encrypted.ciphertext,
      nonce: encrypted.nonce,
      folderId: folderId ?? null,
      schemaVersion,
    };
  }

  /**
   * Encrypts an updated plaintext credential payload using the session VEK.
   */
  public static async encryptAndPrepareUpdate(
    payload: CredentialPayload,
    vek: CryptoKey,
    folderId?: string | null,
    schemaVersion = 1
  ): Promise<UpdateVaultEntryPayload> {
    const encrypted = await VaultCipher.encryptCredential(payload, vek);

    return {
      ciphertext: encrypted.ciphertext,
      nonce: encrypted.nonce,
      folderId: folderId ?? null,
      schemaVersion,
    };
  }

  /**
   * Decrypts an encrypted vault entry from the API into a plaintext credential payload.
   */
  public static async decryptEntry(
    entry: VaultEntryResponse,
    vek: CryptoKey
  ): Promise<DecryptedVaultEntry> {
    const data = await VaultCipher.decryptCredential(
      {
        ciphertext: entry.ciphertext,
        nonce: entry.nonce,
      },
      vek
    );

    return {
      id: entry.id,
      folderId: entry.folderId,
      schemaVersion: entry.schemaVersion,
      createdAt: entry.createdAt,
      updatedAt: entry.updatedAt,
      deletedAt: entry.deletedAt,
      data,
    };
  }

  /**
   * Decrypts a list of vault entries. Silently flags or gracefully handles any corrupted items.
   */
  public static async decryptEntries(
    entries: VaultEntryResponse[],
    vek: CryptoKey
  ): Promise<DecryptedVaultEntry[]> {
    return await Promise.all(entries.map((entry) => this.decryptEntry(entry, vek)));
  }

  /**
   * Encrypts a folder name using the session VEK for zero-knowledge folder organization.
   */
  public static async encryptFolder(
    name: string,
    vek: CryptoKey
  ): Promise<{ encryptedName: string; nonce: string }> {
    const encrypted = await VaultCipher.encryptFolderName(name, vek);
    return {
      encryptedName: encrypted.ciphertext,
      nonce: encrypted.nonce,
    };
  }

  /**
   * Decrypts an encrypted folder name.
   */
  public static async decryptFolder(
    folder: FolderResponse,
    vek: CryptoKey
  ): Promise<DecryptedFolder> {
    const name = await VaultCipher.decryptFolderName(
      {
        ciphertext: folder.encryptedName,
        nonce: folder.nonce,
      },
      vek
    );

    return {
      id: folder.id,
      name,
      createdAt: folder.createdAt,
      updatedAt: folder.updatedAt,
    };
  }

  /**
   * Decrypts a list of folders.
   */
  public static async decryptFolders(
    folders: FolderResponse[],
    vek: CryptoKey
  ): Promise<DecryptedFolder[]> {
    return await Promise.all(folders.map((folder) => this.decryptFolder(folder, vek)));
  }
}
