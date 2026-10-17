import { useState } from 'react';
import {
  KeyRound,
  FileText,
  Copy,
  ExternalLink,
  Edit2,
  Trash2,
  RotateCcw,
  Eye,
  EyeOff,
  Folder as FolderIcon,
  Globe,
  Clock,
  Sparkles,
} from 'lucide-react';
import type { DecryptedFolder, DecryptedVaultEntry } from '../vault/types';
import { copyToClipboard } from './useToast';

interface VaultListProps {
  entries: DecryptedVaultEntry[];
  folders: DecryptedFolder[];
  isTrashView: boolean;
  onEdit: (entry: DecryptedVaultEntry) => void;
  onSoftDelete: (id: string) => void;
  onRestore: (id: string) => void;
  onPurge: (id: string) => void;
  onOpenCreate: () => void;
}

export function VaultList({
  entries,
  folders,
  isTrashView,
  onEdit,
  onSoftDelete,
  onRestore,
  onPurge,
  onOpenCreate,
}: VaultListProps) {
  const [selectedId, setSelectedId] = useState<string | null>(
    entries.length > 0 ? entries[0].id : null
  );
  const [showDetailPassword, setShowDetailPassword] = useState(false);

  // If selected item was deleted or changed, fall back gracefully
  const selectedEntry = entries.find((e) => e.id === selectedId) || entries[0] || null;

  const getFolderName = (folderId?: string | null) => {
    if (!folderId) return null;
    return folders.find((f) => f.id === folderId)?.name || null;
  };

  if (entries.length === 0) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center p-8 text-center">
        <div className="w-16 h-16 rounded-2xl bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center text-indigo-400 mb-4 shadow-xl">
          <KeyRound className="w-8 h-8" />
        </div>
        <h3 className="text-base font-semibold text-white">No items found</h3>
        <p className="text-xs text-slate-400 max-w-sm mt-1 mb-5">
          {isTrashView
            ? 'Your trash is completely empty.'
            : 'Your zero-knowledge vault is ready. Add your first encrypted login, note, or credential.'}
        </p>
        {!isTrashView && (
          <button
            type="button"
            id="empty-state-add-btn"
            onClick={onOpenCreate}
            className="flex items-center gap-2 py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-xs transition-all shadow-lg shadow-indigo-600/20 cursor-pointer"
          >
            <Sparkles className="w-4 h-4" />
            <span>Add First Item</span>
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="flex-1 flex overflow-hidden">
      {/* Items Column */}
      <div className="w-80 md:w-96 border-r border-slate-800/80 overflow-y-auto flex flex-col p-3 gap-2">
        {entries.map((entry) => {
          const isSelected = selectedEntry?.id === entry.id;
          const folderName = getFolderName(entry.folderId);
          const isNote = !entry.data.password && entry.data.notes;

          return (
            <div
              key={entry.id}
              onClick={() => {
                setSelectedId(entry.id);
                setShowDetailPassword(false);
              }}
              className={`p-3.5 rounded-xl border transition-all cursor-pointer flex flex-col gap-2 ${
                isSelected
                  ? 'bg-slate-900/90 border-indigo-500/50 shadow-md'
                  : 'bg-slate-950/40 border-slate-800/60 hover:bg-slate-900/50 hover:border-slate-700/60'
              }`}
            >
              <div className="flex items-start justify-between gap-2">
                <div className="flex items-center gap-2.5 truncate">
                  <div className="w-8 h-8 rounded-lg bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center text-indigo-400 shrink-0">
                    {isNote ? <FileText className="w-4 h-4 text-cyan-400" /> : <KeyRound className="w-4 h-4" />}
                  </div>
                  <div className="truncate">
                    <h4 className="text-xs font-semibold text-slate-100 truncate">{entry.data.title}</h4>
                    <p className="text-[11px] text-slate-400 truncate">
                      {entry.data.username || (isNote ? 'Secure Note' : 'No username')}
                    </p>
                  </div>
                </div>

                {folderName && (
                  <span className="flex items-center gap-1 text-[10px] text-indigo-400 font-medium px-2 py-0.5 rounded-md bg-indigo-950/50 border border-indigo-500/20 shrink-0">
                    <FolderIcon className="w-2.5 h-2.5" />
                    <span>{folderName}</span>
                  </span>
                )}
              </div>

              {/* Quick Copy bar */}
              <div className="flex items-center justify-between pt-1 text-[11px] text-slate-500 border-t border-slate-800/60">
                <span className="text-[10px]">
                  {new Date(entry.updatedAt).toLocaleDateString()}
                </span>
                <div className="flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
                  {entry.data.username && (
                    <button
                      type="button"
                      onClick={() => copyToClipboard(entry.data.username, 'Username')}
                      className="p-1 hover:text-slate-200 hover:bg-slate-800 rounded transition-colors"
                      title="Copy username"
                    >
                      <Copy className="w-3 h-3" />
                    </button>
                  )}
                  {entry.data.password && (
                    <button
                      type="button"
                      onClick={() => copyToClipboard(entry.data.password, 'Password')}
                      className="p-1 hover:text-slate-200 hover:bg-slate-800 rounded transition-colors"
                      title="Copy password"
                    >
                      <KeyRound className="w-3 h-3 text-indigo-400" />
                    </button>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Item Detail Column */}
      {selectedEntry ? (
        <div className="flex-1 overflow-y-auto p-6 flex flex-col gap-6 bg-slate-950/20">
          <div className="flex items-start justify-between border-b border-slate-800/80 pb-4">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-xl bg-gradient-to-tr from-indigo-500/20 to-cyan-500/20 border border-indigo-500/30 flex items-center justify-center text-indigo-400 shadow-md">
                <KeyRound className="w-6 h-6" />
              </div>
              <div>
                <h3 className="text-lg font-bold text-white">{selectedEntry.data.title}</h3>
                <p className="text-xs text-slate-400">
                  Updated {new Date(selectedEntry.updatedAt).toLocaleString()}
                </p>
              </div>
            </div>

            {/* Action buttons */}
            <div className="flex items-center gap-2">
              {isTrashView ? (
                <>
                  <button
                    type="button"
                    onClick={() => onRestore(selectedEntry.id)}
                    className="flex items-center gap-1.5 py-1.5 px-3 rounded-xl bg-indigo-600/20 hover:bg-indigo-600/30 text-indigo-300 border border-indigo-500/30 text-xs font-medium transition-colors cursor-pointer"
                  >
                    <RotateCcw className="w-3.5 h-3.5" />
                    <span>Restore</span>
                  </button>
                  <button
                    type="button"
                    onClick={() => onPurge(selectedEntry.id)}
                    className="flex items-center gap-1.5 py-1.5 px-3 rounded-xl bg-red-950/40 hover:bg-red-950/60 text-red-300 border border-red-500/30 text-xs font-medium transition-colors cursor-pointer"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                    <span>Purge Forever</span>
                  </button>
                </>
              ) : (
                <>
                  <button
                    type="button"
                    onClick={() => onEdit(selectedEntry)}
                    className="flex items-center gap-1.5 py-1.5 px-3 rounded-xl bg-slate-900 hover:bg-slate-800 text-slate-200 border border-slate-700/80 text-xs font-medium transition-colors cursor-pointer"
                  >
                    <Edit2 className="w-3.5 h-3.5 text-slate-400" />
                    <span>Edit</span>
                  </button>
                  <button
                    type="button"
                    onClick={() => onSoftDelete(selectedEntry.id)}
                    className="p-2 rounded-xl bg-slate-900 hover:bg-red-950/40 text-slate-400 hover:text-red-400 border border-slate-700/80 transition-colors cursor-pointer"
                    title="Move to trash"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </>
              )}
            </div>
          </div>

          {/* Details Body */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Username Field */}
            {selectedEntry.data.username && (
              <div className="p-3.5 bg-slate-900/60 rounded-xl border border-slate-800/80 flex items-center justify-between">
                <div className="flex flex-col gap-0.5 truncate mr-2">
                  <span className="text-[10px] font-semibold text-slate-400 uppercase tracking-wider">
                    Username / Email
                  </span>
                  <span className="text-xs text-slate-100 font-mono truncate">
                    {selectedEntry.data.username}
                  </span>
                </div>
                <button
                  type="button"
                  onClick={() => copyToClipboard(selectedEntry.data.username, 'Username')}
                  className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer shrink-0"
                  title="Copy username"
                >
                  <Copy className="w-3.5 h-3.5" />
                </button>
              </div>
            )}

            {/* Password Field */}
            {selectedEntry.data.password && (
              <div className="p-3.5 bg-slate-900/60 rounded-xl border border-slate-800/80 flex items-center justify-between">
                <div className="flex flex-col gap-0.5 truncate mr-2">
                  <span className="text-[10px] font-semibold text-slate-400 uppercase tracking-wider">
                    Password
                  </span>
                  <span className="text-xs text-indigo-300 font-mono truncate">
                    {showDetailPassword ? selectedEntry.data.password : '••••••••••••••••'}
                  </span>
                </div>
                <div className="flex items-center gap-1 shrink-0">
                  <button
                    type="button"
                    onClick={() => setShowDetailPassword(!showDetailPassword)}
                    className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
                    title={showDetailPassword ? 'Hide password' : 'Show password'}
                  >
                    {showDetailPassword ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
                  </button>
                  <button
                    type="button"
                    onClick={() => copyToClipboard(selectedEntry.data.password, 'Password')}
                    className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer"
                    title="Copy password"
                  >
                    <Copy className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            )}

            {/* Website URL */}
            {selectedEntry.data.website && (
              <div className="p-3.5 bg-slate-900/60 rounded-xl border border-slate-800/80 flex items-center justify-between md:col-span-2">
                <div className="flex items-center gap-2.5 truncate mr-2">
                  <Globe className="w-4 h-4 text-slate-400 shrink-0" />
                  <div className="flex flex-col gap-0.5 truncate">
                    <span className="text-[10px] font-semibold text-slate-400 uppercase tracking-wider">
                      Website
                    </span>
                    <a
                      href={selectedEntry.data.website}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-xs text-indigo-400 hover:underline truncate"
                    >
                      {selectedEntry.data.website}
                    </a>
                  </div>
                </div>
                <a
                  href={selectedEntry.data.website}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="p-1.5 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors cursor-pointer shrink-0"
                >
                  <ExternalLink className="w-3.5 h-3.5" />
                </a>
              </div>
            )}
          </div>

          {/* Custom Fields */}
          {selectedEntry.data.customFields && Object.keys(selectedEntry.data.customFields).length > 0 && (
            <div className="flex flex-col gap-2 pt-2 border-t border-slate-800/80">
              <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
                Custom Fields
              </span>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {Object.entries(selectedEntry.data.customFields).map(([key, value]) => (
                  <div
                    key={key}
                    className="p-3 bg-slate-900/50 rounded-xl border border-slate-800/80 flex items-center justify-between"
                  >
                    <div className="flex flex-col truncate mr-2">
                      <span className="text-[10px] text-slate-400 font-medium">{key}</span>
                      <span className="text-xs text-slate-200 font-mono truncate">{value}</span>
                    </div>
                    <button
                      type="button"
                      onClick={() => copyToClipboard(value, key)}
                      className="p-1 text-slate-400 hover:text-white rounded hover:bg-slate-800 transition-colors"
                    >
                      <Copy className="w-3 h-3" />
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Notes */}
          {selectedEntry.data.notes && (
            <div className="flex flex-col gap-2 pt-2 border-t border-slate-800/80">
              <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
                Encrypted Notes
              </span>
              <div className="p-3.5 bg-slate-900/50 rounded-xl border border-slate-800/80 text-xs text-slate-200 whitespace-pre-wrap font-sans leading-relaxed">
                {selectedEntry.data.notes}
              </div>
            </div>
          )}

          {/* Encryption Metadata */}
          <div className="mt-auto pt-4 border-t border-slate-800/60 flex items-center justify-between text-[11px] text-slate-500 font-mono">
            <span className="flex items-center gap-1.5">
              <Clock className="w-3 h-3" />
              <span>Created {new Date(selectedEntry.createdAt).toLocaleDateString()}</span>
            </span>
            <span className="flex items-center gap-1 text-emerald-400/80">
              <span>AES-256-GCM Schema v{selectedEntry.schemaVersion}</span>
            </span>
          </div>
        </div>
      ) : null}
    </div>
  );
}
