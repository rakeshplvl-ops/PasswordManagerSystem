import { useState, useEffect } from 'react';
import { Copy, RefreshCw, X, ShieldCheck } from 'lucide-react';
import {
  DEFAULT_GENERATOR_OPTIONS,
  evaluatePasswordStrength,
  generatePassword,
  type GeneratorOptions,
} from '../generator/passwordGenerator';
import { copyToClipboard } from './useToast';

interface PasswordGeneratorModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSelectPassword?: (password: string) => void;
}

export function PasswordGeneratorModal({
  isOpen,
  onClose,
  onSelectPassword,
}: PasswordGeneratorModalProps) {
  const [options, setOptions] = useState<GeneratorOptions>(DEFAULT_GENERATOR_OPTIONS);
  const [password, setPassword] = useState('');

  const regenerate = () => {
    setPassword(generatePassword(options));
  };

  useEffect(() => {
    if (isOpen) {
      regenerate();
    }
  }, [isOpen, options]);

  if (!isOpen) return null;

  const strength = evaluatePasswordStrength(password);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-sm animate-in fade-in">
      <div className="w-full max-w-md bg-slate-900 border border-slate-700/80 rounded-2xl shadow-2xl p-6 text-slate-100 flex flex-col gap-5">
        <div className="flex items-center justify-between border-b border-slate-800 pb-3">
          <div className="flex items-center gap-2">
            <div className="p-2 rounded-lg bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
              <ShieldCheck className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-white">Password Generator</h2>
              <p className="text-xs text-slate-400">Cryptographically secure, zero-predictability</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Generated Password Box */}
        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between p-3.5 bg-slate-950/80 rounded-xl border border-slate-800 font-mono text-base break-all text-indigo-300">
            <span>{password}</span>
            <div className="flex items-center gap-1.5 shrink-0 ml-2">
              <button
                type="button"
                onClick={regenerate}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
                title="Regenerate"
              >
                <RefreshCw className="w-4 h-4" />
              </button>
              <button
                type="button"
                onClick={() => copyToClipboard(password, 'Generated password')}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
                title="Copy to clipboard"
              >
                <Copy className="w-4 h-4" />
              </button>
            </div>
          </div>

          {/* Strength Bar */}
          <div className="flex items-center justify-between text-xs font-medium text-slate-400 mt-1">
            <span>
              Entropy: <strong className="text-slate-200">{strength.entropy} bits</strong>
            </span>
            <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${
              strength.label === 'Very Strong'
                ? 'bg-emerald-950 text-emerald-400 border border-emerald-500/30'
                : strength.label === 'Strong'
                ? 'bg-cyan-950 text-cyan-400 border border-cyan-500/30'
                : 'bg-yellow-950 text-yellow-400 border border-yellow-500/30'
            }`}>
              {strength.label}
            </span>
          </div>
          <div className="w-full h-1.5 bg-slate-800 rounded-full overflow-hidden">
            <div
              className={`h-full transition-all duration-300 ${strength.color}`}
              style={{ width: `${strength.score}%` }}
            />
          </div>
        </div>

        {/* Controls */}
        <div className="flex flex-col gap-4 text-sm text-slate-300">
          <div>
            <div className="flex justify-between mb-1.5">
              <span className="font-medium text-slate-200">Length</span>
              <span className="font-mono text-indigo-400 font-semibold">{options.length}</span>
            </div>
            <input
              type="range"
              min="8"
              max="64"
              value={options.length}
              onChange={(e) => setOptions({ ...options, length: Number(e.target.value) })}
              className="w-full h-1.5 bg-slate-800 rounded-lg appearance-none cursor-pointer accent-indigo-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-2.5">
            <label className="flex items-center gap-2.5 p-2.5 rounded-xl bg-slate-950/40 border border-slate-800/80 cursor-pointer hover:bg-slate-800/50 transition-colors">
              <input
                type="checkbox"
                checked={options.uppercase}
                onChange={(e) => setOptions({ ...options, uppercase: e.target.checked })}
                className="w-4 h-4 rounded text-indigo-600 accent-indigo-500"
              />
              <span>Uppercase (A-Z)</span>
            </label>

            <label className="flex items-center gap-2.5 p-2.5 rounded-xl bg-slate-950/40 border border-slate-800/80 cursor-pointer hover:bg-slate-800/50 transition-colors">
              <input
                type="checkbox"
                checked={options.lowercase}
                onChange={(e) => setOptions({ ...options, lowercase: e.target.checked })}
                className="w-4 h-4 rounded text-indigo-600 accent-indigo-500"
              />
              <span>Lowercase (a-z)</span>
            </label>

            <label className="flex items-center gap-2.5 p-2.5 rounded-xl bg-slate-950/40 border border-slate-800/80 cursor-pointer hover:bg-slate-800/50 transition-colors">
              <input
                type="checkbox"
                checked={options.numbers}
                onChange={(e) => setOptions({ ...options, numbers: e.target.checked })}
                className="w-4 h-4 rounded text-indigo-600 accent-indigo-500"
              />
              <span>Numbers (0-9)</span>
            </label>

            <label className="flex items-center gap-2.5 p-2.5 rounded-xl bg-slate-950/40 border border-slate-800/80 cursor-pointer hover:bg-slate-800/50 transition-colors">
              <input
                type="checkbox"
                checked={options.symbols}
                onChange={(e) => setOptions({ ...options, symbols: e.target.checked })}
                className="w-4 h-4 rounded text-indigo-600 accent-indigo-500"
              />
              <span>Symbols (!@#$)</span>
            </label>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-2.5 mt-1 pt-3 border-t border-slate-800">
          <button
            type="button"
            onClick={onClose}
            className="flex-1 py-2.5 px-4 rounded-xl border border-slate-700 hover:bg-slate-800 text-slate-300 font-medium text-sm transition-colors"
          >
            Cancel
          </button>
          {onSelectPassword ? (
            <button
              type="button"
              onClick={() => {
                onSelectPassword(password);
                onClose();
              }}
              className="flex-1 py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm transition-colors shadow-lg shadow-indigo-600/20"
            >
              Use Password
            </button>
          ) : (
            <button
              type="button"
              onClick={() => {
                copyToClipboard(password, 'Generated password');
                onClose();
              }}
              className="flex-1 py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm transition-colors shadow-lg shadow-indigo-600/20"
            >
              Copy Password
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
