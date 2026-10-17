import { describe, expect, it } from 'vitest';
import {
  evaluatePasswordStrength,
  generatePassword,
} from '../passwordGenerator';

describe('passwordGenerator', () => {
  it('generates password of specified length', () => {
    const pwd16 = generatePassword({ length: 16, uppercase: true, lowercase: true, numbers: true, symbols: true, avoidAmbiguous: false });
    const pwd32 = generatePassword({ length: 32, uppercase: true, lowercase: true, numbers: true, symbols: true, avoidAmbiguous: false });

    expect(pwd16.length).toBe(16);
    expect(pwd32.length).toBe(32);
  });

  it('generates passwords without ambiguous characters when requested', () => {
    const pwd = generatePassword({
      length: 50,
      uppercase: true,
      lowercase: true,
      numbers: true,
      symbols: true,
      avoidAmbiguous: true,
    });

    const ambiguous = ['0', 'O', 'o', 'l', '1', 'I'];
    for (const ch of ambiguous) {
      expect(pwd).not.toContain(ch);
    }
  });

  it('correctly evaluates entropy and strength', () => {
    const weak = evaluatePasswordStrength('123456');
    expect(weak.label).toBe('Very Weak');
    expect(weak.score).toBeLessThan(40);

    const strong = evaluatePasswordStrength('MySuper$SecureP@ssw0rd!2026');
    expect(strong.score).toBeGreaterThanOrEqual(80);
    expect(['Strong', 'Very Strong']).toContain(strong.label);
  });
});
