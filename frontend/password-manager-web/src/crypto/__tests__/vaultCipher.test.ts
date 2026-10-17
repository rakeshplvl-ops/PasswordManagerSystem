import { describe, expect, it } from 'vitest';
import { base64ToBytes, bytesToBase64 } from '../cryptoUtils';
import type { CredentialPayload } from '../types';
import { VaultCipher } from '../vaultCipher';
import { VekService } from '../vekService';

describe('VaultCipher (AES-256-GCM)', () => {
  it('encrypts and decrypts a plain text string correctly', async () => {
    const vek = await VekService.generateVek();
    const plaintext = 'TopSecretAPIKey_sk_live_938102847192847192';

    const encrypted = await VaultCipher.encryptString(plaintext, vek);
    expect(encrypted.ciphertext).toBeDefined();
    expect(encrypted.nonce).toBeDefined();

    const decrypted = await VaultCipher.decryptString(encrypted, vek);
    expect(decrypted).toBe(plaintext);
  });

  it('encrypts and decrypts a complex credential payload correctly', async () => {
    const vek = await VekService.generateVek();
    const credential: CredentialPayload = {
      title: 'GitHub Enterprise',
      username: 'rakesh.dev',
      password: 'Strong&Long$Password!2026',
      website: 'https://github.com/login',
      notes: 'Contains primary production deploy keys',
      folderId: 'folder-123-uuid',
      customFields: {
        'Recovery Code 1': 'ABCD-EFGH',
        'PIN': '9876',
      },
    };

    const encrypted = await VaultCipher.encryptCredential(credential, vek);
    const decrypted = await VaultCipher.decryptCredential(encrypted, vek);

    expect(decrypted).toEqual(credential);
  });

  it('encrypts and decrypts folder names for zero-knowledge metadata', async () => {
    const vek = await VekService.generateVek();
    const folderName = 'Financial & Banking';

    const encrypted = await VaultCipher.encryptFolderName(folderName, vek);
    const decrypted = await VaultCipher.decryptFolderName(encrypted, vek);

    expect(decrypted).toBe(folderName);
  });

  it('generates fresh nonces and unique ciphertexts for identical plaintext', async () => {
    const vek = await VekService.generateVek();
    const plaintext = 'IdenticalSecretPassword';

    const enc1 = await VaultCipher.encryptString(plaintext, vek);
    const enc2 = await VaultCipher.encryptString(plaintext, vek);

    // Nonce must NEVER be reused across encryptions!
    expect(enc1.nonce).not.toBe(enc2.nonce);
    expect(enc1.ciphertext).not.toBe(enc2.ciphertext);

    // Both must decrypt to the original plaintext
    expect(await VaultCipher.decryptString(enc1, vek)).toBe(plaintext);
    expect(await VaultCipher.decryptString(enc2, vek)).toBe(plaintext);
  });

  it('rejects decryption if ciphertext has been tampered with (integrity authentication tag check)', async () => {
    const vek = await VekService.generateVek();
    const plaintext = 'Do not tamper with this payload';

    const encrypted = await VaultCipher.encryptString(plaintext, vek);

    // Corrupt ciphertext by flipping a bit
    const rawBytes = base64ToBytes(encrypted.ciphertext);
    rawBytes[rawBytes.length - 1] ^= 0x01; // flip bit in tag/ciphertext
    const tampered = {
      ciphertext: bytesToBase64(rawBytes),
      nonce: encrypted.nonce,
    };

    await expect(VaultCipher.decryptString(tampered, vek)).rejects.toThrow(
      /Decryption failed: Ciphertext or nonce integrity check failed/
    );
  });

  it('rejects decryption if nonce has been altered', async () => {
    const vek = await VekService.generateVek();
    const plaintext = 'Important secret';

    const encrypted = await VaultCipher.encryptString(plaintext, vek);

    // Corrupt nonce
    const nonceBytes = base64ToBytes(encrypted.nonce);
    nonceBytes[0] ^= 0x02;
    const tampered = {
      ciphertext: encrypted.ciphertext,
      nonce: bytesToBase64(nonceBytes),
    };

    await expect(VaultCipher.decryptString(tampered, vek)).rejects.toThrow(
      /Decryption failed: Ciphertext or nonce integrity check failed/
    );
  });

  it('rejects decryption if attempted with a different VEK', async () => {
    const vek1 = await VekService.generateVek();
    const vek2 = await VekService.generateVek();

    const encrypted = await VaultCipher.encryptString('Secret for Key 1', vek1);

    await expect(VaultCipher.decryptString(encrypted, vek2)).rejects.toThrow(
      /Decryption failed: Ciphertext or nonce integrity check failed/
    );
  });
});
