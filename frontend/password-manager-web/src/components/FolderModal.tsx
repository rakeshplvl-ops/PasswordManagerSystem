import { useState, useEffect, type FormEvent } from 'react';
import { Folder, X, Loader2 } from 'lucide-react';
import { useVaultStore } from '../vault/vaultStore';
import { VaultService } from '../vault/vaultService';
import { vaultApi } from '../vault/api';
import type { DecryptedFolder } from '../vault/types';
import { useToast } from './useToast';

interface FolderModalProps {
  isOpen: boolean;
  onClose: () => void;
  folder?: DecryptedFolder | null;
  onSaved: () => void;
}

export function FolderModal({ isOpen, onClose, folder, onSaved }: FolderModalProps) {
  const [name, setName] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const vek = useVaultStore((state) => state.vek);
  const showToast = useToast((state) => state.showToast);

  useEffect(() => {
    if (folder) {
      setName(folder.name);
    } else {
      setName('');
    }
    setError(null);
  }, [folder, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    if (!vek) {
      setError('Vault is locked. Unlock to manage folders.');
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      const encrypted = await VaultService.encryptFolder(name.trim(), vek);

      if (folder) {
        await vaultApi.updateFolder(folder.id, encrypted);
        showToast('Folder renamed successfully!', 'success');
      } else {
        await vaultApi.createFolder(encrypted);
        showToast('Folder created successfully!', 'success');
      }

      onSaved();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save folder.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-sm animate-in fade-in">
      <div className="w-full max-w-md bg-slate-900 border border-slate-700/80 rounded-2xl shadow-2xl p-6 text-slate-100 flex flex-col gap-5">
        <div className="flex items-center justify-between border-b border-slate-800 pb-3">
          <div className="flex items-center gap-2">
            <div className="p-2 rounded-lg bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
              <Folder className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-white">
                {folder ? 'Rename Folder' : 'New Folder'}
              </h2>
              <p className="text-xs text-slate-400">Folder names are encrypted zero-knowledge</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {error && (
          <div className="p-3 bg-red-950/70 border border-red-500/40 rounded-xl text-red-200 text-xs font-medium text-center">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-medium text-slate-300">Folder Name</label>
            <input
              id="folder-name-input"
              type="text"
              required
              autoFocus
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Work, Financial, Personal"
              className="w-full px-3.5 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all"
            />
          </div>

          <div className="flex gap-2.5 pt-2 border-t border-slate-800">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-2.5 px-4 rounded-xl border border-slate-700 hover:bg-slate-800 text-slate-300 font-medium text-sm transition-colors cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSaving || !name.trim()}
              className="flex-1 py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm transition-colors shadow-lg shadow-indigo-600/20 flex items-center justify-center gap-2 disabled:opacity-50 cursor-pointer"
            >
              {isSaving ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Save Folder'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
