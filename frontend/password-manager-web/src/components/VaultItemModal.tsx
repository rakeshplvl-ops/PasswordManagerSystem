import { useState, useEffect, type FormEvent } from 'react';
import {
  KeyRound,
  Eye,
  EyeOff,
  Sparkles,
  Copy,
  Plus,
  Trash2,
  X,
  Loader2,
  Folder as FolderIcon,
  Globe,
  FileText,
  Star,
} from 'lucide-react';
import { useVaultStore } from '../vault/vaultStore';
import { VaultService } from '../vault/vaultService';
import { vaultApi } from '../vault/api';
import type { DecryptedFolder, DecryptedVaultEntry } from '../vault/types';
import type { CredentialPayload } from '../crypto/types';
import { PasswordGeneratorModal } from './PasswordGeneratorModal';
import { copyToClipboard, useToast } from './useToast';

interface VaultItemModalProps {
  isOpen: boolean;
  onClose: () => void;
  item?: DecryptedVaultEntry | null;
  folders: DecryptedFolder[];
  defaultFolderId?: string | null;
  onSaved: () => void;
}

export function VaultItemModal({
  isOpen,
  onClose,
  item,
  folders,
  defaultFolderId,
  onSaved,
}: VaultItemModalProps) {
  const [title, setTitle] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [website, setWebsite] = useState('');
  const [folderId, setFolderId] = useState<string | null>(null);
  const [notes, setNotes] = useState('');
  const [customFields, setCustomFields] = useState<{ key: string; value: string }[]>([]);
  const [isFavorite, setIsFavorite] = useState(false);

  const [showPassword, setShowPassword] = useState(false);
  const [isGeneratorOpen, setIsGeneratorOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const vek = useVaultStore((state) => state.vek);
  const showToast = useToast((state) => state.showToast);

  useEffect(() => {
    if (item) {
      setTitle(item.data.title || '');
      setUsername(item.data.username || '');
      setPassword(item.data.password || '');
      setWebsite(item.data.website || '');
      setFolderId(item.folderId || null);
      setNotes(item.data.notes || '');
      setIsFavorite(!!item.data.isFavorite);

      const fields = Object.entries(item.data.customFields || {}).map(([key, value]) => ({
        key,
        value,
      }));
      setCustomFields(fields);
    } else {
      setTitle('');
      setUsername('');
      setPassword('');
      setWebsite('');
      setFolderId(defaultFolderId || null);
      setNotes('');
      setIsFavorite(false);
      setCustomFields([]);
    }
    setError(null);
  }, [item, defaultFolderId, isOpen]);

  if (!isOpen) return null;

  const handleAddField = () => {
    setCustomFields([...customFields, { key: '', value: '' }]);
  };

  const handleRemoveField = (index: number) => {
    setCustomFields(customFields.filter((_, i) => i !== index));
  };

  const handleFieldChange = (index: number, key: string, value: string) => {
    const updated = [...customFields];
    updated[index] = { key, value };
    setCustomFields(updated);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('Title is required.');
      return;
    }

    if (!vek) {
      setError('Vault is locked. Please unlock to save items.');
      return;
    }

    setIsSaving(true);
    setError(null);

    const customFieldsMap: Record<string, string> = {};
    for (const f of customFields) {
      if (f.key.trim()) {
        customFieldsMap[f.key.trim()] = f.value;
      }
    }

    const payload: CredentialPayload = {
      title: title.trim(),
      username: username.trim(),
      password,
      website: website.trim() || undefined,
      notes: notes.trim() || undefined,
      folderId: folderId || null,
      customFields: Object.keys(customFieldsMap).length > 0 ? customFieldsMap : undefined,
      isFavorite: isFavorite || undefined,
    };

    try {
      if (item) {
        const prepared = await VaultService.encryptAndPrepareUpdate(payload, vek, folderId);
        await vaultApi.updateEntry(item.id, prepared);
        showToast('Item updated successfully!', 'success');
      } else {
        const prepared = await VaultService.encryptAndPrepareCreate(payload, vek, folderId);
        await vaultApi.createEntry(prepared);
        showToast('Item created successfully!', 'success');
      }

      onSaved();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save item.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in overflow-y-auto">
        <div className="w-full max-w-xl bg-slate-900 border border-slate-700/80 rounded-2xl shadow-2xl p-6 text-slate-100 flex flex-col gap-5 my-8">
          <div className="flex items-center justify-between border-b border-slate-800 pb-3">
            <div className="flex items-center gap-2">
              <div className="p-2 rounded-lg bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
                <KeyRound className="w-5 h-5" />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-white">
                  {item ? 'Edit Vault Item' : 'New Vault Item'}
                </h2>
                <p className="text-xs text-slate-400">Zero-knowledge AES-256-GCM encrypted</p>
              </div>
            </div>
            <button
              onClick={onClose}
              className="p-1 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
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
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3.5">
              {/* Title */}
              <div className="flex flex-col gap-1.5 md:col-span-2">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-medium text-slate-300">Title</label>
                  <button
                    type="button"
                    onClick={() => setIsFavorite(!isFavorite)}
                    className={`flex items-center gap-1.5 px-2.5 py-1 rounded-lg border text-[11px] font-medium transition-colors cursor-pointer ${
                      isFavorite
                        ? 'bg-amber-500/15 border-amber-500/40 text-amber-300'
                        : 'bg-slate-950/60 border-slate-800 text-slate-400 hover:text-slate-200'
                    }`}
                  >
                    <Star className={`w-3.5 h-3.5 ${isFavorite ? 'fill-amber-400 text-amber-400' : ''}`} />
                    <span>{isFavorite ? 'Favorited' : 'Favorite'}</span>
                  </button>
                </div>
                <input
                  id="item-title-input"
                  type="text"
                  required
                  autoFocus
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="e.g. GitHub Enterprise, Amazon, Bank"
                  className="w-full px-3.5 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all"
                />
              </div>

              {/* Username */}
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-medium text-slate-300">Username or Email</label>
                <input
                  id="item-username-input"
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  placeholder="user@example.com"
                  className="w-full px-3.5 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all"
                />
              </div>

              {/* Folder */}
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-medium text-slate-300 flex items-center gap-1">
                  <FolderIcon className="w-3.5 h-3.5 text-slate-400" />
                  <span>Folder</span>
                </label>
                <select
                  id="item-folder-select"
                  value={folderId || ''}
                  onChange={(e) => setFolderId(e.target.value || null)}
                  className="w-full px-3.5 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all cursor-pointer"
                >
                  <option value="">(No Folder)</option>
                  {folders.map((f) => (
                    <option key={f.id} value={f.id}>
                      {f.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Password */}
              <div className="flex flex-col gap-1.5 md:col-span-2">
                <div className="flex justify-between items-center">
                  <label className="text-xs font-medium text-slate-300">Password</label>
                  <button
                    type="button"
                    onClick={() => setIsGeneratorOpen(true)}
                    className="flex items-center gap-1 text-xs text-indigo-400 hover:text-indigo-300 font-medium transition-colors cursor-pointer"
                  >
                    <Sparkles className="w-3.5 h-3.5" />
                    <span>Generate Secure</span>
                  </button>
                </div>
                <div className="relative flex items-center">
                  <input
                    id="item-password-input"
                    type={showPassword ? 'text' : 'password'}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Enter password or generate..."
                    className="w-full pl-3.5 pr-20 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all font-mono"
                  />
                  <div className="absolute right-2 flex items-center gap-1">
                    {password && (
                      <button
                        type="button"
                        onClick={() => copyToClipboard(password, 'Password')}
                        className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
                        title="Copy password"
                      >
                        <Copy className="w-3.5 h-3.5" />
                      </button>
                    )}
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
                      title={showPassword ? 'Hide password' : 'Show password'}
                    >
                      {showPassword ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
                    </button>
                  </div>
                </div>
              </div>

              {/* Website */}
              <div className="flex flex-col gap-1.5 md:col-span-2">
                <label className="text-xs font-medium text-slate-300 flex items-center gap-1">
                  <Globe className="w-3.5 h-3.5 text-slate-400" />
                  <span>Website URL</span>
                </label>
                <input
                  id="item-website-input"
                  type="url"
                  value={website}
                  onChange={(e) => setWebsite(e.target.value)}
                  placeholder="https://example.com"
                  className="w-full px-3.5 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all"
                />
              </div>

              {/* Notes */}
              <div className="flex flex-col gap-1.5 md:col-span-2">
                <label className="text-xs font-medium text-slate-300 flex items-center gap-1">
                  <FileText className="w-3.5 h-3.5 text-slate-400" />
                  <span>Notes</span>
                </label>
                <textarea
                  id="item-notes-input"
                  rows={3}
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Encrypted notes, recovery codes, or security notes..."
                  className="w-full px-3.5 py-2.5 bg-slate-950/80 border border-slate-800 rounded-xl text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all resize-none"
                />
              </div>
            </div>

            {/* Custom Fields */}
            <div className="flex flex-col gap-2 pt-2 border-t border-slate-800/80">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-slate-300 uppercase tracking-wider">
                  Custom Fields
                </span>
                <button
                  type="button"
                  onClick={handleAddField}
                  className="flex items-center gap-1 text-xs text-indigo-400 hover:text-indigo-300 font-medium transition-colors cursor-pointer"
                >
                  <Plus className="w-3.5 h-3.5" />
                  <span>Add Field</span>
                </button>
              </div>

              {customFields.map((field, idx) => (
                <div key={idx} className="flex items-center gap-2">
                  <input
                    type="text"
                    value={field.key}
                    onChange={(e) => handleFieldChange(idx, e.target.value, field.value)}
                    placeholder="Field name (e.g. PIN)"
                    className="flex-1 px-3 py-1.5 bg-slate-950/80 border border-slate-800 rounded-lg text-xs text-slate-100 focus:outline-none focus:border-indigo-500"
                  />
                  <input
                    type="text"
                    value={field.value}
                    onChange={(e) => handleFieldChange(idx, field.key, e.target.value)}
                    placeholder="Value"
                    className="flex-1 px-3 py-1.5 bg-slate-950/80 border border-slate-800 rounded-lg text-xs text-slate-100 font-mono focus:outline-none focus:border-indigo-500"
                  />
                  <button
                    type="button"
                    onClick={() => handleRemoveField(idx)}
                    className="p-1.5 text-slate-500 hover:text-red-400 rounded-lg hover:bg-slate-800 transition-colors"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              ))}
            </div>

            {/* Buttons */}
            <div className="flex gap-2.5 pt-3 border-t border-slate-800">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 py-2.5 px-4 rounded-xl border border-slate-700 hover:bg-slate-800 text-slate-300 font-medium text-sm transition-colors cursor-pointer"
              >
                Cancel
              </button>
              <button
                id="save-item-btn"
                type="submit"
                disabled={isSaving || !title.trim()}
                className="flex-1 py-2.5 px-4 rounded-xl bg-gradient-to-r from-indigo-600 to-indigo-500 hover:from-indigo-500 hover:to-indigo-400 text-white font-medium text-sm transition-all shadow-lg shadow-indigo-600/25 flex items-center justify-center gap-2 disabled:opacity-50 cursor-pointer"
              >
                {isSaving ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Save Encrypted Item'}
              </button>
            </div>
          </form>
        </div>
      </div>

      <PasswordGeneratorModal
        isOpen={isGeneratorOpen}
        onClose={() => setIsGeneratorOpen(false)}
        onSelectPassword={(generated) => setPassword(generated)}
      />
    </>
  );
}
