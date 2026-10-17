export interface GeneratorOptions {
  length: number;
  uppercase: boolean;
  lowercase: boolean;
  numbers: boolean;
  symbols: boolean;
  avoidAmbiguous: boolean;
}

export const DEFAULT_GENERATOR_OPTIONS: GeneratorOptions = {
  length: 18,
  uppercase: true,
  lowercase: true,
  numbers: true,
  symbols: true,
  avoidAmbiguous: false,
};

const UPPERCASE_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
const LOWERCASE_CHARS = 'abcdefghijklmnopqrstuvwxyz';
const NUMBER_CHARS = '0123456789';
const SYMBOL_CHARS = '!@#$%^&*()_+-=[]{}|;:,.<>?';
const AMBIGUOUS_CHARS = new Set(['0', 'O', 'o', 'l', '1', 'I']);

export function generatePassword(options: GeneratorOptions = DEFAULT_GENERATOR_OPTIONS): string {
  let charPool = '';

  if (options.uppercase) charPool += UPPERCASE_CHARS;
  if (options.lowercase) charPool += LOWERCASE_CHARS;
  if (options.numbers) charPool += NUMBER_CHARS;
  if (options.symbols) charPool += SYMBOL_CHARS;

  if (options.avoidAmbiguous) {
    charPool = charPool
      .split('')
      .filter((ch) => !AMBIGUOUS_CHARS.has(ch))
      .join('');
  }

  if (charPool.length === 0) {
    charPool = LOWERCASE_CHARS; // fallback to lowercase if everything unchecked
  }

  const length = Math.max(8, Math.min(64, options.length));
  const randomBytes = new Uint32Array(length);
  crypto.getRandomValues(randomBytes);

  const passwordChars: string[] = [];
  for (let i = 0; i < length; i++) {
    const charIndex = randomBytes[i] % charPool.length;
    passwordChars.push(charPool[charIndex]);
  }

  return passwordChars.join('');
}

export interface PasswordStrength {
  score: number; // 0 to 100
  entropy: number; // bits
  label: 'Very Weak' | 'Weak' | 'Moderate' | 'Strong' | 'Very Strong';
  color: string;
}

export function evaluatePasswordStrength(password: string): PasswordStrength {
  if (!password) {
    return { score: 0, entropy: 0, label: 'Very Weak', color: 'bg-red-500' };
  }

  let poolSize = 0;
  if (/[a-z]/.test(password)) poolSize += 26;
  if (/[A-Z]/.test(password)) poolSize += 26;
  if (/[0-9]/.test(password)) poolSize += 10;
  if (/[^a-zA-Z0-9]/.test(password)) poolSize += 32;

  if (poolSize === 0) poolSize = 26;

  const entropy = Math.round(password.length * Math.log2(poolSize));
  const score = Math.min(100, Math.round((entropy / 100) * 100));

  if (entropy < 40) {
    return { score: Math.max(10, score), entropy, label: 'Very Weak', color: 'bg-red-500' };
  } else if (entropy < 60) {
    return { score, entropy, label: 'Weak', color: 'bg-orange-500' };
  } else if (entropy < 80) {
    return { score, entropy, label: 'Moderate', color: 'bg-yellow-500' };
  } else if (entropy < 100) {
    return { score, entropy, label: 'Strong', color: 'bg-emerald-500' };
  } else {
    return { score: 100, entropy, label: 'Very Strong', color: 'bg-cyan-400' };
  }
}
