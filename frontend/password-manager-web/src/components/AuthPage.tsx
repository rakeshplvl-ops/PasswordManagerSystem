import { useState, type FormEvent } from 'react';
import { ShieldCheck, Lock, Mail, Eye, EyeOff, Loader2, Sparkles } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { evaluatePasswordStrength } from '../generator/passwordGenerator';
import { useToast } from './useToast';

export function AuthPage() {
  const [isRegister, setIsRegister] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { login, register } = useAuth();
  const showToast = useToast((state) => state.showToast);

  const strength = evaluatePasswordStrength(password);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (isRegister && password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    if (isRegister && password.length < 8) {
      setError('Master Password must be at least 8 characters.');
      return;
    }

    setIsSubmitting(true);
    try {
      if (isRegister) {
        await register(email, password);
        showToast('Vault created and unlocked!', 'success');
      } else {
        await login(email, password);
        showToast('Welcome back!', 'success');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An error occurred during authentication.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-[#090d16] text-slate-100 relative overflow-hidden">
      {/* Background radial glow accents */}
      <div className="absolute -top-40 -left-40 w-96 h-96 bg-indigo-500/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none" />

      <div className="w-full max-w-md bg-slate-900/90 border border-slate-800 rounded-3xl shadow-2xl p-8 backdrop-blur-xl relative z-10">
        {/* Brand Header */}
        <div className="flex flex-col items-center text-center gap-3 mb-6">
          <div className="w-14 h-14 rounded-2xl bg-gradient-to-tr from-indigo-500 to-cyan-500 p-0.5 shadow-xl shadow-indigo-500/20">
            <div className="w-full h-full bg-slate-950 rounded-[14px] flex items-center justify-center text-indigo-400">
              <ShieldCheck className="w-8 h-8 text-cyan-400" />
            </div>
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-white flex items-center justify-center gap-1.5">
              <span>Enterprise Vault</span>
              <Sparkles className="w-4 h-4 text-cyan-400" />
            </h1>
            <p className="text-xs text-slate-400 mt-1">
              Zero-Knowledge AES-256-GCM & Argon2id Encryption
            </p>
          </div>
        </div>

        {/* Tab Switcher */}
        <div className="flex bg-slate-950/70 p-1 rounded-xl border border-slate-800 mb-6">
          <button
            type="button"
            id="auth-tab-login"
            onClick={() => {
              setIsRegister(false);
              setError(null);
            }}
            className={`flex-1 py-2 text-xs font-semibold rounded-lg transition-all ${
              !isRegister
                ? 'bg-indigo-600 text-white shadow-md'
                : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            Sign In
          </button>
          <button
            type="button"
            id="auth-tab-register"
            onClick={() => {
              setIsRegister(true);
              setError(null);
            }}
            className={`flex-1 py-2 text-xs font-semibold rounded-lg transition-all ${
              isRegister
                ? 'bg-indigo-600 text-white shadow-md'
                : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            Create Vault
          </button>
        </div>

        {error && (
          <div className="p-3 bg-red-950/70 border border-red-500/40 rounded-xl text-red-200 text-xs font-medium text-center mb-5">
            {error}
          </div>
        )}

        {/* Auth Form */}
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-medium text-slate-300">Email Address</label>
            <div className="relative flex items-center">
              <Mail className="w-4 h-4 text-slate-500 absolute left-3.5" />
              <input
                id="auth-email-input"
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@company.com"
                className="w-full pl-10 pr-4 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all"
              />
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-medium text-slate-300">
              {isRegister ? 'Master Password' : 'Password'}
            </label>
            <div className="relative flex items-center">
              <Lock className="w-4 h-4 text-slate-500 absolute left-3.5" />
              <input
                id="auth-password-input"
                type={showPassword ? 'text' : 'password'}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder={isRegister ? 'Choose a strong master password...' : 'Enter password...'}
                className="w-full pl-10 pr-10 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all font-mono"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="p-1 text-slate-500 hover:text-slate-300 absolute right-3"
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>

            {/* Password strength meter during registration */}
            {isRegister && password && (
              <div className="flex flex-col gap-1 mt-1">
                <div className="flex justify-between items-center text-[11px]">
                  <span className="text-slate-400">Strength: {strength.entropy} bits</span>
                  <span className="font-semibold text-slate-300">{strength.label}</span>
                </div>
                <div className="w-full h-1 bg-slate-800 rounded-full overflow-hidden">
                  <div
                    className={`h-full transition-all duration-300 ${strength.color}`}
                    style={{ width: `${strength.score}%` }}
                  />
                </div>
              </div>
            )}
          </div>

          {isRegister && (
            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-medium text-slate-300">Confirm Master Password</label>
              <div className="relative flex items-center">
                <Lock className="w-4 h-4 text-slate-500 absolute left-3.5" />
                <input
                  id="auth-confirm-password-input"
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  placeholder="Re-enter master password..."
                  className="w-full pl-10 pr-4 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all font-mono"
                />
              </div>
              <p className="text-[11px] text-amber-400/90 mt-1 leading-relaxed bg-amber-950/30 p-2 rounded-lg border border-amber-500/20">
                🔒 <strong>Zero-Knowledge:</strong> Your master password encrypts your vault and is never stored on the server. If lost, your vault cannot be recovered.
              </p>
            </div>
          )}

          <button
            id="auth-submit-btn"
            type="submit"
            disabled={isSubmitting || !email || !password}
            className="w-full mt-3 py-3 px-4 rounded-xl bg-gradient-to-r from-indigo-600 to-indigo-500 hover:from-indigo-500 hover:to-indigo-400 text-white font-medium text-sm transition-all shadow-lg shadow-indigo-600/25 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>{isRegister ? 'Creating Encrypted Vault...' : 'Signing in...'}</span>
              </>
            ) : (
              <span>{isRegister ? 'Create Vault & Get Started' : 'Sign In to Vault'}</span>
            )}
          </button>
        </form>
      </div>
    </div>
  );
}
