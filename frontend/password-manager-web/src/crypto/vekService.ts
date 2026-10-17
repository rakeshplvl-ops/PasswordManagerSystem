import {
  base64ToBytes,
  bytesToBase64,
  generateRandomBytes,
  zeroize,
} from './cryptoUtils';
import { KdfService } from './kdfService';
import type { InitialVaultBundle, KdfParams } from './types';

/**
 * Vault Encryption Key (VEK) Service.
 * Manages the generation, envelope-wrapping (encryption with KEK),
 * and unwrapping (decryption with KEK) of the symmetric vault key.
 */
export class VekService {
  /**
   * Generates a fresh 256-bit AES-GCM key for encrypting vault items.
   */
  public static async generateVek(): Promise<CryptoKey> {
    return await crypto.subtle.generateKey(
      { name: 'AES-GCM', length: 256 },
      true, // extractable so it can be wrapped and exported into local session
      ['encrypt', 'decrypt']
    );
  }

  /**
   * Wraps (encrypts) the VEK with the user's Key Encryption Key (KEK) using AES-256-GCM.
   */
  public static async wrapVek(
    vek: CryptoKey,
    kek: CryptoKey
  ): Promise<{ encryptedVek: string; nonce: string }> {
    const rawVek = await crypto.subtle.exportKey('raw', vek);
    const nonceBytes = generateRandomBytes(12); // 96-bit AES-GCM standard nonce

    try {
      const ciphertextBuffer = await crypto.subtle.encrypt(
        {
          name: 'AES-GCM',
          iv: nonceBytes as unknown as BufferSource,
        },
        kek,
        rawVek as unknown as BufferSource
      );

      return {
        encryptedVek: bytesToBase64(new Uint8Array(ciphertextBuffer)),
        nonce: bytesToBase64(nonceBytes),
      };
    } finally {
      zeroize(rawVek);
      zeroize(nonceBytes);
    }
  }

  /**
   * Unwraps (decrypts) the encrypted VEK using the user's Key Encryption Key (KEK).
   * Throws if the KEK is incorrect or if the ciphertext/nonce was tampered with.
   */
  public static async unwrapVek(
    encryptedVek: string,
    nonce: string,
    kek: CryptoKey,
    extractable = true
  ): Promise<CryptoKey> {
    if (!encryptedVek) {
      throw new Error('Encrypted VEK cannot be empty.');
    }
    if (!nonce) {
      throw new Error('VEK nonce cannot be empty.');
    }

    const ciphertextBytes = base64ToBytes(encryptedVek);
    const nonceBytes = base64ToBytes(nonce);

    let decryptedBuffer: ArrayBuffer;
    try {
      decryptedBuffer = await crypto.subtle.decrypt(
        {
          name: 'AES-GCM',
          iv: nonceBytes as unknown as BufferSource,
        },
        kek,
        ciphertextBytes as unknown as BufferSource
      );
    } catch {
      throw new Error('Failed to unwrap VEK: Invalid master password or corrupted key material.');
    } finally {
      zeroize(ciphertextBytes);
      zeroize(nonceBytes);
    }

    try {
      const vek = await crypto.subtle.importKey(
        'raw',
        decryptedBuffer,
        { name: 'AES-GCM', length: 256 },
        extractable,
        ['encrypt', 'decrypt']
      );

      return vek;
    } finally {
      zeroize(decryptedBuffer);
    }
  }

  /**
   * Complete setup helper for initial vault creation:
   * Generates salt, derives KEK, generates VEK, and wraps VEK with KEK.
   */
  public static async initializeVault(
    masterPassword: string,
    customParams?: Partial<KdfParams>
  ): Promise<InitialVaultBundle> {
    const defaultParams = KdfService.createDefaultParams();
    const kdfParams: KdfParams = {
      ...defaultParams,
      ...customParams,
    };

    const kek = await KdfService.deriveKek(masterPassword, kdfParams);
    const vek = await this.generateVek();
    const { encryptedVek, nonce: vekNonce } = await this.wrapVek(vek, kek);

    return {
      vek,
      kek,
      encryptedVek,
      vekNonce,
      kdfParams,
    };
  }
}
