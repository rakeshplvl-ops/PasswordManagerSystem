import { describe, expect, it } from 'vitest';
import { base64ToBytes, bytesToBase64, generateSalt } from '../cryptoUtils';
import { KdfService } from '../kdfService';
import type { KdfParams } from '../types';
import { VaultCipher } from '../vaultCipher';
import { VekService } from '../vekService';

describe('VekService (Envelope Encryption)', () => {
  const kdfParams: KdfParams = {
    algorithm: 'Argon2id',
    iterations: 2,
    memoryKb: 4096,
    parallelism: 2,
    salt: generateSalt(16),
  };

  it('generates a 256-bit AES-GCM CryptoKey', async () => {
    const vek = await VekService.generateVek();

    expect(vek).toBeDefined();
    expect(vek.type).toBe('secret');
    expect(vek.algorithm.name).toBe('AES-GCM');
    expect(vek.usages).toContain('encrypt');
    expect(vek.usages).toContain('decrypt');
  });

  it('wraps and unwraps VEK successfully with correct KEK', async () => {
    const masterPassword = 'MasterPassword#2026';
    const kek = await KdfService.deriveKek(masterPassword, kdfParams);
    const originalVek = await VekService.generateVek();

    const { encryptedVek, nonce } = await VekService.wrapVek(originalVek, kek);
    expect(encryptedVek).toBeDefined();
    expect(nonce).toBeDefined();

    const unwrappedVek = await VekService.unwrapVek(encryptedVek, nonce, kek);
    expect(unwrappedVek).toBeDefined();

    // Verify unwrapped VEK works: encrypt with original, decrypt with unwrapped
    const secretMessage = 'Secret bank credential details';
    const encrypted = await VaultCipher.encryptString(secretMessage, originalVek);
    const decrypted = await VaultCipher.decryptString(encrypted, unwrappedVek);

    expect(decrypted).toBe(secretMessage);
  });

  it('fails to unwrap VEK if master password / KEK is wrong', async () => {
    const kek1 = await KdfService.deriveKek('CorrectPassword', kdfParams);
    const kek2 = await KdfService.deriveKek('WrongPassword', kdfParams);

    const vek = await VekService.generateVek();
    const { encryptedVek, nonce } = await VekService.wrapVek(vek, kek1);

    await expect(VekService.unwrapVek(encryptedVek, nonce, kek2)).rejects.toThrow(
      /Failed to unwrap VEK/
    );
  });

  it('fails to unwrap VEK if encrypted ciphertext was tampered with', async () => {
    const kek = await KdfService.deriveKek('CorrectPassword', kdfParams);
    const vek = await VekService.generateVek();
    const { encryptedVek, nonce } = await VekService.wrapVek(vek, kek);

    // Tamper with ciphertext by flipping a bit in the decoded byte array
    const bytes = base64ToBytes(encryptedVek);
    bytes[0] ^= 0x01; // flip 1 bit
    const corruptedVek = bytesToBase64(bytes);

    await expect(VekService.unwrapVek(corruptedVek, nonce, kek)).rejects.toThrow(
      /Failed to unwrap VEK/
    );
  });

  it('fails to unwrap VEK if nonce was tampered with', async () => {
    const kek = await KdfService.deriveKek('CorrectPassword', kdfParams);
    const vek = await VekService.generateVek();
    const { encryptedVek, nonce } = await VekService.wrapVek(vek, kek);

    const nonceBytes = base64ToBytes(nonce);
    nonceBytes[0] ^= 0x01;
    const corruptedNonce = bytesToBase64(nonceBytes);

    await expect(VekService.unwrapVek(encryptedVek, corruptedNonce, kek)).rejects.toThrow(
      /Failed to unwrap VEK/
    );
  });

  it('initializes a full vault bundle correctly', async () => {
    const bundle = await VekService.initializeVault('InitialMasterPassword', {
      memoryKb: 4096,
      iterations: 2,
      parallelism: 2,
    });

    expect(bundle.vek).toBeDefined();
    expect(bundle.kek).toBeDefined();
    expect(bundle.encryptedVek).toBeDefined();
    expect(bundle.vekNonce).toBeDefined();
    expect(bundle.kdfParams.salt).toBeDefined();

    // Verify unwrapping bundle
    const restoredVek = await VekService.unwrapVek(
      bundle.encryptedVek,
      bundle.vekNonce,
      bundle.kek
    );
    expect(restoredVek).toBeDefined();
  });
});
