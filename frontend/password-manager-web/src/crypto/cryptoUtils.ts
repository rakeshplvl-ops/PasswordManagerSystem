/**
 * Cryptographic utility functions for byte array conversions,
 * secure random generation, and memory zeroization.
 */

export function bytesToBase64(bytes: Uint8Array): string {
  // Process in chunks to prevent Maximum Call Stack Size Exceeded for large arrays
  const CHUNK_SIZE = 0x8000;
  const chunks: string[] = [];
  for (let i = 0; i < bytes.length; i += CHUNK_SIZE) {
    const chunk = bytes.subarray(i, Math.min(i + CHUNK_SIZE, bytes.length));
    chunks.push(String.fromCharCode.apply(null, chunk as unknown as number[]));
  }
  return btoa(chunks.join(''));
}

export function base64ToBytes(base64: string): Uint8Array {
  const binaryString = atob(base64.trim());
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  return bytes;
}

export function stringToBytes(str: string): Uint8Array {
  return new TextEncoder().encode(str);
}

export function bytesToString(bytes: Uint8Array): string {
  return new TextDecoder().decode(bytes);
}

export function generateRandomBytes(length: number): Uint8Array {
  if (length <= 0) {
    throw new Error('Length must be greater than zero.');
  }
  const buffer = new Uint8Array(length);
  crypto.getRandomValues(buffer);
  return buffer;
}

export function generateSalt(length = 16): string {
  const bytes = generateRandomBytes(length);
  const base64 = bytesToBase64(bytes);
  zeroize(bytes);
  return base64;
}

/**
 * Generates a 96-bit (12-byte) nonce standard for AES-GCM encryption.
 */
export function generateNonce(length = 12): string {
  const bytes = generateRandomBytes(length);
  const base64 = bytesToBase64(bytes);
  zeroize(bytes);
  return base64;
}

/**
 * Wipes sensitive byte arrays by filling memory with zero bytes.
 */
export function zeroize(buffer: Uint8Array | ArrayBuffer | Uint8ClampedArray): void {
  if (buffer instanceof Uint8Array || buffer instanceof Uint8ClampedArray) {
    buffer.fill(0);
  } else if (buffer instanceof ArrayBuffer) {
    new Uint8Array(buffer).fill(0);
  }
}

/**
 * Constant-time comparison of two Uint8Arrays to prevent timing attacks.
 */
export function constantTimeEquals(a: Uint8Array, b: Uint8Array): boolean {
  if (a.length !== b.length) {
    return false;
  }
  let diff = 0;
  for (let i = 0; i < a.length; i++) {
    diff |= a[i] ^ b[i];
  }
  return diff === 0;
}
