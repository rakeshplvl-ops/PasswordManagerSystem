import { describe, expect, it } from 'vitest';
import type { CredentialPayload } from '../../crypto/types';
import { VekService } from '../../crypto/vekService';
import type { FolderResponse, VaultEntryResponse } from '../types';
import { VaultService } from '../vaultService';

describe('VaultService (Transparent Client-Side Encryption/Decryption)', () => {
  it('encrypts and prepares entry payload, and decrypts it back', async () => {
    const vek = await VekService.generateVek();

    const credential: CredentialPayload = {
      title: 'AWS Production Console',
      username: 'cloud-admin',
      password: 'IAM$StrongPassword#2026',
      website: 'https://console.aws.amazon.com',
      notes: 'Root account credentials',
      folderId: 'folder-abc-123',
      customFields: {
        'MFA Serial': 'arn:aws:iam::123456789:mfa/root',
      },
    };

    const prepared = await VaultService.encryptAndPrepareCreate(
      credential,
      vek,
      'folder-abc-123',
      1
    );

    expect(prepared.ciphertext).toBeDefined();
    expect(prepared.nonce).toBeDefined();
    expect(prepared.folderId).toBe('folder-abc-123');
    expect(prepared.schemaVersion).toBe(1);

    // Mock API response structure
    const mockEntryResponse: VaultEntryResponse = {
      id: 'entry-uuid-999',
      folderId: prepared.folderId,
      ciphertext: prepared.ciphertext,
      nonce: prepared.nonce,
      schemaVersion: prepared.schemaVersion ?? 1,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    const decrypted = await VaultService.decryptEntry(mockEntryResponse, vek);

    expect(decrypted.id).toBe('entry-uuid-999');
    expect(decrypted.folderId).toBe('folder-abc-123');
    expect(decrypted.data).toEqual(credential);
  });

  it('encrypts and decrypts folder names correctly', async () => {
    const vek = await VekService.generateVek();
    const folderName = 'Personal Banking & Crypto';

    const prepared = await VaultService.encryptFolder(folderName, vek);
    expect(prepared.encryptedName).toBeDefined();
    expect(prepared.nonce).toBeDefined();

    const mockFolderResponse: FolderResponse = {
      id: 'folder-uuid-111',
      encryptedName: prepared.encryptedName,
      nonce: prepared.nonce,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    const decrypted = await VaultService.decryptFolder(mockFolderResponse, vek);
    expect(decrypted.id).toBe('folder-uuid-111');
    expect(decrypted.name).toBe(folderName);
  });

  it('fails to decrypt if ciphertext was altered', async () => {
    const vek = await VekService.generateVek();
    const credential: CredentialPayload = {
      title: 'Test',
      username: 'user',
      password: 'pass',
    };

    const prepared = await VaultService.encryptAndPrepareCreate(credential, vek);

    const corruptedResponse: VaultEntryResponse = {
      id: '123',
      ciphertext: prepared.ciphertext.slice(0, -4) + 'AAAA',
      nonce: prepared.nonce,
      schemaVersion: 1,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    await expect(VaultService.decryptEntry(corruptedResponse, vek)).rejects.toThrow(
      /Decryption failed/
    );
  });
});
