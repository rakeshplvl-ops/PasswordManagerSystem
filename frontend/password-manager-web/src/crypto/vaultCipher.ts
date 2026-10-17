import {
  base64ToBytes,
  bytesToBase64,
  bytesToString,
  generateRandomBytes,
  stringToBytes,
  zeroize,
} from './cryptoUtils';
import type { CredentialPayload, EncryptedData } from './types';

/**
 * Vault Cipher Service.
 * Provides AES-256-GCM authenticated encryption and decryption for vault credentials,
 * metadata, and encrypted folder names.
 */
export class VaultCipher {
  /**
   * Encrypts arbitrary raw bytes using AES-256-GCM with a fresh 96-bit random nonce.
   */
  public static async encryptBytes(
    data: Uint8Array,
    vek: CryptoKey
  ): Promise<EncryptedData> {
    const nonceBytes = generateRandomBytes(12);

    try {
      const ciphertextBuffer = await crypto.subtle.encrypt(
        {
          name: 'AES-GCM',
          iv: nonceBytes as unknown as BufferSource,
        },
        vek,
        data as unknown as BufferSource
      );

      return {
        ciphertext: bytesToBase64(new Uint8Array(ciphertextBuffer)),
        nonce: bytesToBase64(nonceBytes),
      };
    } finally {
      zeroize(nonceBytes);
    }
  }

  /**
   * Decrypts AES-256-GCM ciphertext back into raw bytes.
   * Throws if the key is incorrect or if the ciphertext or nonce has been altered.
   */
  public static async decryptBytes(
    encrypted: EncryptedData,
    vek: CryptoKey
  ): Promise<Uint8Array> {
    if (!encrypted.ciphertext) {
      throw new Error('Ciphertext cannot be empty.');
    }
    if (!encrypted.nonce) {
      throw new Error('Nonce cannot be empty.');
    }

    const ciphertextBytes = base64ToBytes(encrypted.ciphertext);
    const nonceBytes = base64ToBytes(encrypted.nonce);

    let decryptedBuffer: ArrayBuffer;
    try {
      decryptedBuffer = await crypto.subtle.decrypt(
        {
          name: 'AES-GCM',
          iv: nonceBytes as unknown as BufferSource,
        },
        vek,
        ciphertextBytes as unknown as BufferSource
      );

      return new Uint8Array(decryptedBuffer);
    } catch {
      throw new Error('Decryption failed: Ciphertext or nonce integrity check failed (tampering detected or invalid key).');
    } finally {
      zeroize(ciphertextBytes);
      zeroize(nonceBytes);
    }
  }

  /**
   * Encrypts a UTF-8 string (e.g. folder name, secret note) with AES-256-GCM.
   */
  public static async encryptString(
    plaintext: string,
    vek: CryptoKey
  ): Promise<EncryptedData> {
    const bytes = stringToBytes(plaintext);
    try {
      return await this.encryptBytes(bytes, vek);
    } finally {
      zeroize(bytes);
    }
  }

  /**
   * Decrypts an AES-256-GCM ciphertext into a UTF-8 string.
   */
  public static async decryptString(
    encrypted: EncryptedData,
    vek: CryptoKey
  ): Promise<string> {
    const bytes = await this.decryptBytes(encrypted, vek);
    try {
      return bytesToString(bytes);
    } finally {
      zeroize(bytes);
    }
  }

  /**
   * Encrypts any JSON-serializable object (such as CredentialPayload).
   */
  public static async encryptPayload<T>(
    payload: T,
    vek: CryptoKey
  ): Promise<EncryptedData> {
    const jsonString = JSON.stringify(payload);
    return await this.encryptString(jsonString, vek);
  }

  /**
   * Decrypts ciphertext and parses the resulting JSON into the expected payload type.
   */
  public static async decryptPayload<T>(
    encrypted: EncryptedData,
    vek: CryptoKey
  ): Promise<T> {
    const jsonString = await this.decryptString(encrypted, vek);
    return JSON.parse(jsonString) as T;
  }

  /**
   * Convenience method to encrypt a credential payload.
   */
  public static async encryptCredential(
    payload: CredentialPayload,
    vek: CryptoKey
  ): Promise<EncryptedData> {
    return await this.encryptPayload<CredentialPayload>(payload, vek);
  }

  /**
   * Convenience method to decrypt a credential payload.
   */
  public static async decryptCredential(
    encrypted: EncryptedData,
    vek: CryptoKey
  ): Promise<CredentialPayload> {
    return await this.decryptPayload<CredentialPayload>(encrypted, vek);
  }

  /**
   * Helper to encrypt a folder name for zero-knowledge folder organization.
   */
  public static async encryptFolderName(
    name: string,
    vek: CryptoKey
  ): Promise<EncryptedData> {
    return await this.encryptString(name, vek);
  }

  /**
   * Helper to decrypt a folder name.
   */
  public static async decryptFolderName(
    encrypted: EncryptedData,
    vek: CryptoKey
  ): Promise<string> {
    return await this.decryptString(encrypted, vek);
  }
}
