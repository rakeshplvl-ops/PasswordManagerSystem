import { describe, expect, it } from 'vitest';
import {
  base64ToBytes,
  bytesToBase64,
  bytesToString,
  constantTimeEquals,
  generateNonce,
  generateRandomBytes,
  generateSalt,
  stringToBytes,
  zeroize,
} from '../cryptoUtils';

describe('cryptoUtils', () => {
  it('converts byte array to Base64 and back correctly', () => {
    const original = new Uint8Array([0, 1, 2, 255, 128, 64, 32, 16]);
    const base64 = bytesToBase64(original);
    const roundTrip = base64ToBytes(base64);

    expect(roundTrip).toEqual(original);
  });

  it('handles large byte arrays without stack overflow in Base64 conversion', () => {
    const largeArray = new Uint8Array(100_000);
    for (let i = 0; i < largeArray.length; i++) {
      largeArray[i] = i % 256;
    }

    const base64 = bytesToBase64(largeArray);
    const roundTrip = base64ToBytes(base64);

    expect(roundTrip.length).toBe(largeArray.length);
    expect(roundTrip[0]).toBe(0);
    expect(roundTrip[100]).toBe(100 % 256);
    expect(roundTrip[99999]).toBe(99999 % 256);
  });

  it('encodes and decodes UTF-8 strings with Unicode and emojis', () => {
    const testString = 'Enterprise Vault 🔐 100% Confidential! こんにちは $¥€';
    const bytes = stringToBytes(testString);
    const result = bytesToString(bytes);

    expect(result).toBe(testString);
  });

  it('generates cryptographically random bytes of requested length', () => {
    const bytes1 = generateRandomBytes(32);
    const bytes2 = generateRandomBytes(32);

    expect(bytes1.length).toBe(32);
    expect(bytes2.length).toBe(32);
    expect(bytes1).not.toEqual(bytes2);
  });

  it('generates random Base64 salt of 16 bytes', () => {
    const salt1 = generateSalt(16);
    const salt2 = generateSalt(16);

    expect(salt1).not.toBe(salt2);
    const bytes = base64ToBytes(salt1);
    expect(bytes.length).toBe(16);
  });

  it('generates 96-bit (12-byte) AES-GCM nonce', () => {
    const nonce = generateNonce(12);
    const bytes = base64ToBytes(nonce);

    expect(bytes.length).toBe(12);
  });

  it('zeroizes memory buffers effectively', () => {
    const buffer = new Uint8Array([42, 99, 123, 255]);
    zeroize(buffer);

    expect(buffer).toEqual(new Uint8Array([0, 0, 0, 0]));
  });

  it('checks constant time equality properly', () => {
    const a = new Uint8Array([1, 2, 3, 4]);
    const b = new Uint8Array([1, 2, 3, 4]);
    const c = new Uint8Array([1, 2, 3, 5]);
    const d = new Uint8Array([1, 2, 3]);

    expect(constantTimeEquals(a, b)).toBe(true);
    expect(constantTimeEquals(a, c)).toBe(false);
    expect(constantTimeEquals(a, d)).toBe(false);
  });
});
