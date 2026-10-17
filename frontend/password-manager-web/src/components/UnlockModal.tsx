import { useState, type FormEvent } from 'react';
import { Lock, KeyRound, Loader2, LogOut } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { useToast } from './useToast';

export function UnlockModal() {
  const [masterPassword, setMasterPassword] = useState('');
  const [isUnlocking, setIsUnlocking] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { user, unlockVault, logout } = useAuth();
  const showToast = useToast((state) => state.showToast);

  const handleUnlock = async (e: FormEvent) => {
    e.preventDefault();
    if (!masterPassword) return;

    setIsUnlocking(true);
    setError(null);

    try {
      await unlockVault(masterPassword);
      showToast('Vault unlocked successfully!', 'success');
      setMasterPassword('');
    } catch {
      setError('Invalid Master Password. Unable to decrypt vault encryption key.');
    } finally {
      setIsUnlocking(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in">
      <div className="w-full max-w-md bg-slate-900 border border-slate-700/80 rounded-3xl shadow-2xl p-7 text-slate-100 flex flex-col gap-6">
        <div className="flex flex-col items-center text-center gap-3">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-tr from-indigo-500/20 to-cyan-500/20 border border-indigo-500/30 flex items-center justify-center text-indigo-400 shadow-xl shadow-indigo-500/10">
            <Lock className="w-8 h-8" />
          </div>
          <div>
            <h2 className="text-2xl font-bold text-white tracking-tight">Vault Locked</h2>
            <p className="text-sm text-slate-400 mt-1">
              Enter your Master Password to decrypt your zero-knowledge vault
            </p>
            <p className="text-xs font-mono text-indigo-400/90 mt-1 bg-indigo-950/40 py-1 px-2.5 rounded-full inline-block border border-indigo-800/40">
              {user?.email}
            </p>
          </div>
        </div>

        {error && (
          <div className="p-3.5 bg-red-950/60 border border-red-500/40 rounded-xl text-red-200 text-xs font-medium text-center">
            {error}
          </div>
        )}

        <form onSubmit={handleUnlock} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-semibold text-slate-300 uppercase tracking-wider">
              Master Password
            </label>
            <div className="relative flex items-center">
              <KeyRound className="w-4 h-4 text-slate-400 absolute left-3.5" />
              <input
                id="unlock-master-password"
                type="password"
                required
                autoFocus
                value={masterPassword}
                onChange={(e) => setMasterPassword(e.target.value)}
                placeholder="Enter master password..."
                className="w-full pl-10 pr-4 py-3 bg-slate-950/90 border border-slate-700/80 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all font-mono"
              />
            </div>
          </div>

          <button
            id="unlock-vault-btn"
            type="submit"
            disabled={isUnlocking || !masterPassword}
            className="w-full mt-2 py-3 px-4 rounded-xl bg-gradient-to-r from-indigo-600 to-indigo-500 hover:from-indigo-500 hover:to-indigo-400 text-white font-medium text-sm transition-all shadow-lg shadow-indigo-600/25 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
          >
            {isUnlocking ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Deriving Key & Decrypting...</span>
              </>
            ) : (
              <span>Unlock Vault</span>
            )}
          </button>
        </form>

        <div className="pt-2 border-t border-slate-800 flex justify-center">
          <button
            type="button"
            onClick={logout}
            className="flex items-center gap-2 text-xs text-slate-400 hover:text-slate-200 transition-colors py-1 px-3 rounded-lg hover:bg-slate-800/60"
          >
            <LogOut className="w-3.5 h-3.5" />
            <span>Sign out / Switch account</span>
          </button>
        </div>
      </div>
    </div>
  );
}
