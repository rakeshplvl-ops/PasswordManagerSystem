import { argon2id } from 'hash-wasm';
import { base64ToBytes, bytesToBase64, generateSalt, zeroize } from './cryptoUtils';
import { DEFAULT_KDF_PARAMS, type KdfParams } from './types';

/**
 * Key Derivation Service (KDF) using Argon2id via WebAssembly.
 * Derives a 256-bit Key Encryption Key (KEK) from the user's master password and salt.
 */
export class KdfService {
  /**
   * Generates default KDF parameters with a fresh cryptographically secure 16-byte salt.
   */
  public static createDefaultParams(): KdfParams {
    return {
      ...DEFAULT_KDF_PARAMS,
      salt: generateSalt(16),
    };
  }

  /**
   * Derives raw 32-byte key material using Argon2id.
   * NOTE: The caller is responsible for zeroizing the returned Uint8Array when finished.
   */
  public static async deriveKeyMaterial(
    masterPassword: string,
    params: KdfParams
  ): Promise<Uint8Array> {
    if (!masterPassword) {
      throw new Error('Master password cannot be empty.');
    }
    if (!params.salt) {
      throw new Error('KDF salt cannot be empty.');
    }

    const saltBytes = base64ToBytes(params.salt);
    const keyLength = params.keyLengthBytes ?? 32;

    try {
      const derivedBytes = await argon2id({
        password: masterPassword,
        salt: saltBytes,
        iterations: params.iterations,
        memorySize: params.memoryKb,
        parallelism: params.parallelism,
        hashLength: keyLength,
        outputType: 'binary',
      });

      return derivedBytes;
    } finally {
      zeroize(saltBytes);
    }
  }

  /**
   * Derives the Key Encryption Key (KEK) as an AES-GCM CryptoKey from master password.
   */
  public static async deriveKek(
    masterPassword: string,
    params: KdfParams,
    extractable = false
  ): Promise<CryptoKey> {
    const rawKey = await this.deriveKeyMaterial(masterPassword, params);

    try {
      const kek = await crypto.subtle.importKey(
        'raw',
        rawKey as BufferSource,
        { name: 'AES-GCM', length: 256 },
        extractable,
        ['encrypt', 'decrypt']
      );

      return kek;
    } finally {
      zeroize(rawKey);
    }
  }

  /**
   * Derives a deterministic client-side authentication proof from the master password and salt.
   * This ensures the plaintext master password is never transmitted across the network.
   */
  public static async deriveAuthProof(
    masterPassword: string,
    saltBase64: string
  ): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(`PasswordManager:AuthProof:${masterPassword}:${saltBase64}`);
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    return bytesToBase64(new Uint8Array(hashBuffer));
  }
}
