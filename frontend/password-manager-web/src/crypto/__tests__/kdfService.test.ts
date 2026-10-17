import { describe, expect, it } from 'vitest';
import { generateSalt } from '../cryptoUtils';
import { KdfService } from '../kdfService';
import type { KdfParams } from '../types';

describe('KdfService (Argon2id)', () => {
  const testParams: KdfParams = {
    algorithm: 'Argon2id',
    iterations: 2,
    memoryKb: 4096, // 4 MB for fast unit test execution
    parallelism: 2,
    salt: generateSalt(16),
    keyLengthBytes: 32,
  };

  it('generates default KDF parameters with secure 16-byte salt', () => {
    const params = KdfService.createDefaultParams();
    expect(params.algorithm).toBe('Argon2id');
    expect(params.iterations).toBe(3);
    expect(params.memoryKb).toBe(65536);
    expect(params.parallelism).toBe(4);
    expect(params.salt).toBeDefined();
    expect(params.salt.length).toBeGreaterThan(10);
  });

  it('derives 32-byte key material deterministically for same password and salt', async () => {
    const password = 'SuperSecretMasterPassword#2026!';
    const key1 = await KdfService.deriveKeyMaterial(password, testParams);
    const key2 = await KdfService.deriveKeyMaterial(password, testParams);

    expect(key1.length).toBe(32);
    expect(key2.length).toBe(32);
    expect(key1).toEqual(key2);
  });

  it('produces different key material for different passwords', async () => {
    const key1 = await KdfService.deriveKeyMaterial('Password-A', testParams);
    const key2 = await KdfService.deriveKeyMaterial('Password-B', testParams);

    expect(key1).not.toEqual(key2);
  });

  it('produces different key material for different salts', async () => {
    const password = 'SamePassword';
    const params1 = { ...testParams, salt: generateSalt(16) };
    const params2 = { ...testParams, salt: generateSalt(16) };

    const key1 = await KdfService.deriveKeyMaterial(password, params1);
    const key2 = await KdfService.deriveKeyMaterial(password, params2);

    expect(key1).not.toEqual(key2);
  });

  it('derives a valid AES-GCM CryptoKey (KEK)', async () => {
    const kek = await KdfService.deriveKek('MasterPassword123!', testParams);

    expect(kek).toBeDefined();
    expect(kek.type).toBe('secret');
    expect(kek.algorithm.name).toBe('AES-GCM');
    expect(kek.usages).toContain('encrypt');
    expect(kek.usages).toContain('decrypt');
  });

  it('derives deterministic client-side auth proof', async () => {
    const proof1 = await KdfService.deriveAuthProof('MyMasterPass', testParams.salt);
    const proof2 = await KdfService.deriveAuthProof('MyMasterPass', testParams.salt);
    const proofDifferentPass = await KdfService.deriveAuthProof('OtherPass', testParams.salt);

    expect(proof1).toBe(proof2);
    expect(proof1).not.toBe(proofDifferentPass);
    expect(proof1.length).toBeGreaterThan(20);
  });
});
