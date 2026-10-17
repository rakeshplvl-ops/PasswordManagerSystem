import {
  ShieldCheck,
  KeyRound,
  FileText,
  Trash2,
  Folder as FolderIcon,
  Lock,
  LogOut,
  FolderPlus,
  Edit2,
  Sparkles,
} from 'lucide-react';
import type { DecryptedFolder, DecryptedVaultEntry } from '../vault/types';
import { useAuth } from '../auth/AuthContext';
import { useVaultStore } from '../vault/vaultStore';

export type FilterCategory = 'all' | 'logins' | 'notes' | 'trash';

interface SidebarProps {
  selectedCategory: FilterCategory;
  selectedFolderId: string | null;
  onSelectCategory: (category: FilterCategory) => void;
  onSelectFolder: (folderId: string | null) => void;
  entries: DecryptedVaultEntry[];
  folders: DecryptedFolder[];
  onOpenCreateFolder: () => void;
  onOpenEditFolder: (folder: DecryptedFolder) => void;
  onDeleteFolder: (folderId: string) => void;
  onOpenGenerator: () => void;
}

export function Sidebar({
  selectedCategory,
  selectedFolderId,
  onSelectCategory,
  onSelectFolder,
  entries,
  folders,
  onOpenCreateFolder,
  onOpenEditFolder,
  onDeleteFolder,
  onOpenGenerator,
}: SidebarProps) {
  const { user, logout } = useAuth();
  const lockVault = useVaultStore((state) => state.lockVault);

  const activeEntries = entries.filter((e) => !e.deletedAt);
  const trashCount = entries.filter((e) => e.deletedAt).length;

  return (
    <aside className="w-64 bg-slate-950/70 border-r border-slate-800/80 flex flex-col justify-between h-screen shrink-0 backdrop-blur-md">
      {/* Brand & App Title */}
      <div className="flex flex-col gap-6 p-4">
        <div className="flex items-center gap-3 px-2 pt-2">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-indigo-500 to-cyan-500 p-0.5 shadow-md shadow-indigo-500/20">
            <div className="w-full h-full bg-slate-950 rounded-[10px] flex items-center justify-center text-cyan-400">
              <ShieldCheck className="w-5 h-5" />
            </div>
          </div>
          <div>
            <h1 className="text-sm font-bold text-white tracking-tight flex items-center gap-1">
              <span>Enterprise Vault</span>
            </h1>
            <span className="text-[10px] text-emerald-400 font-mono flex items-center gap-1">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
              Zero-Knowledge
            </span>
          </div>
        </div>

        {/* Primary Views */}
        <nav className="flex flex-col gap-1">
          <button
            type="button"
            id="nav-all-items"
            onClick={() => {
              onSelectFolder(null);
              onSelectCategory('all');
            }}
            className={`flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all cursor-pointer ${
              selectedCategory === 'all' && selectedFolderId === null
                ? 'bg-indigo-600/20 text-indigo-300 border border-indigo-500/30'
                : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/60'
            }`}
          >
            <div className="flex items-center gap-2.5">
              <KeyRound className="w-4 h-4 text-indigo-400" />
              <span>All Items</span>
            </div>
            <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-slate-900 text-slate-400 font-mono">
              {activeEntries.length}
            </span>
          </button>

          <button
            type="button"
            id="nav-notes"
            onClick={() => {
              onSelectFolder(null);
              onSelectCategory('notes');
            }}
            className={`flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all cursor-pointer ${
              selectedCategory === 'notes' && selectedFolderId === null
                ? 'bg-indigo-600/20 text-indigo-300 border border-indigo-500/30'
                : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/60'
            }`}
          >
            <div className="flex items-center gap-2.5">
              <FileText className="w-4 h-4 text-cyan-400" />
              <span>Secure Notes</span>
            </div>
            <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-slate-900 text-slate-400 font-mono">
              {activeEntries.filter((e) => !e.data.password && e.data.notes).length}
            </span>
          </button>

          <button
            type="button"
            id="nav-trash"
            onClick={() => {
              onSelectFolder(null);
              onSelectCategory('trash');
            }}
            className={`flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all cursor-pointer ${
              selectedCategory === 'trash'
                ? 'bg-red-950/40 text-red-300 border border-red-500/30'
                : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/60'
            }`}
          >
            <div className="flex items-center gap-2.5">
              <Trash2 className="w-4 h-4 text-red-400" />
              <span>Trash</span>
            </div>
            {trashCount > 0 && (
              <span className="px-1.5 py-0.5 rounded-full text-[10px] bg-red-950 text-red-400 font-mono">
                {trashCount}
              </span>
            )}
          </button>

          <button
            type="button"
            id="nav-generator"
            onClick={onOpenGenerator}
            className="flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-medium text-slate-400 hover:text-indigo-300 hover:bg-slate-900/60 transition-all cursor-pointer mt-1"
          >
            <Sparkles className="w-4 h-4 text-indigo-400" />
            <span>Password Generator</span>
          </button>
        </nav>

        {/* Folders Section */}
        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between px-2 text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
            <span>Folders</span>
            <button
              type="button"
              id="create-folder-btn"
              onClick={onOpenCreateFolder}
              className="p-1 rounded-md text-slate-400 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer"
              title="Create new folder"
            >
              <FolderPlus className="w-3.5 h-3.5" />
            </button>
          </div>

          <div className="flex flex-col gap-1 max-h-48 overflow-y-auto pr-1">
            {folders.length === 0 ? (
              <p className="text-[11px] text-slate-500 px-3 py-1">No folders created yet</p>
            ) : (
              folders.map((folder) => {
                const count = activeEntries.filter((e) => e.folderId === folder.id).length;
                const isSelected = selectedFolderId === folder.id;

                return (
                  <div
                    key={folder.id}
                    className={`group flex items-center justify-between px-3 py-1.5 rounded-xl text-xs font-medium transition-all ${
                      isSelected
                        ? 'bg-indigo-600/20 text-indigo-300 border border-indigo-500/30'
                        : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/60'
                    }`}
                  >
                    <button
                      type="button"
                      onClick={() => onSelectFolder(folder.id)}
                      className="flex items-center gap-2 flex-1 text-left truncate cursor-pointer"
                    >
                      <FolderIcon className="w-3.5 h-3.5 text-indigo-400 shrink-0" />
                      <span className="truncate">{folder.name}</span>
                    </button>

                    <div className="flex items-center gap-1">
                      <span className="text-[10px] text-slate-500 font-mono group-hover:hidden">
                        {count}
                      </span>
                      <div className="hidden group-hover:flex items-center gap-0.5">
                        <button
                          type="button"
                          onClick={() => onOpenEditFolder(folder)}
                          className="p-1 text-slate-400 hover:text-white rounded transition-colors"
                          title="Rename"
                        >
                          <Edit2 className="w-3 h-3" />
                        </button>
                        <button
                          type="button"
                          onClick={() => onDeleteFolder(folder.id)}
                          className="p-1 text-slate-400 hover:text-red-400 rounded transition-colors"
                          title="Delete folder"
                        >
                          <Trash2 className="w-3 h-3" />
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>

      {/* User profile & Lock Vault */}
      <div className="p-4 border-t border-slate-800/80 flex flex-col gap-2">
        <div className="flex items-center gap-2.5 px-2 py-1">
          <div className="w-8 h-8 rounded-full bg-indigo-500/20 border border-indigo-500/40 flex items-center justify-center text-xs font-bold text-indigo-300 font-mono">
            {user?.email?.charAt(0).toUpperCase() || 'U'}
          </div>
          <div className="flex-1 truncate">
            <p className="text-xs font-medium text-slate-200 truncate">{user?.email}</p>
            <p className="text-[10px] text-slate-400 truncate">Standard Session</p>
          </div>
        </div>

        <div className="flex gap-1.5 mt-1">
          <button
            type="button"
            id="lock-vault-sidebar-btn"
            onClick={lockVault}
            className="flex-1 py-1.5 px-2 rounded-lg bg-slate-900 hover:bg-slate-800 text-slate-300 text-xs font-medium transition-colors border border-slate-800 flex items-center justify-center gap-1.5 cursor-pointer"
            title="Lock vault immediately"
          >
            <Lock className="w-3.5 h-3.5 text-amber-400" />
            <span>Lock</span>
          </button>
          <button
            type="button"
            id="logout-sidebar-btn"
            onClick={logout}
            className="py-1.5 px-2.5 rounded-lg bg-slate-900 hover:bg-red-950/40 text-slate-400 hover:text-red-300 text-xs font-medium transition-colors border border-slate-800 flex items-center justify-center cursor-pointer"
            title="Sign out"
          >
            <LogOut className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </aside>
  );
}
