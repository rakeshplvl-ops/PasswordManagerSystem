import { useEffect } from 'react';
import { Search, Plus } from 'lucide-react';

interface HeaderProps {
  searchQuery: string;
  onSearchChange: (query: string) => void;
  onOpenCreateItem: () => void;
  title: string;
}

export function Header({
  searchQuery,
  onSearchChange,
  onOpenCreateItem,
  title,
}: HeaderProps) {
  // Focus search input on Ctrl+K / Cmd+K
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        const searchInput = document.getElementById('vault-search-input');
        searchInput?.focus();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  return (
    <header className="h-16 px-6 border-b border-slate-800/80 bg-slate-950/40 flex items-center justify-between gap-4 backdrop-blur-md">
      <div className="flex items-center gap-3">
        <h2 className="text-base font-semibold text-white tracking-tight">{title}</h2>
      </div>

      <div className="flex items-center gap-3 flex-1 max-w-md">
        <div className="relative flex items-center w-full">
          <Search className="w-4 h-4 text-slate-500 absolute left-3.5 pointer-events-none" />
          <input
            id="vault-search-input"
            type="text"
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Search vault items... (Ctrl + K)"
            className="w-full pl-9 pr-12 py-2 bg-slate-900/80 border border-slate-800 rounded-xl text-xs text-slate-100 placeholder-slate-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all"
          />
          <kbd className="absolute right-3 px-1.5 py-0.5 rounded text-[10px] font-mono text-slate-500 bg-slate-800/80 border border-slate-700/60 pointer-events-none">
            Ctrl K
          </kbd>
        </div>

        <button
          id="header-create-item-btn"
          type="button"
          onClick={onOpenCreateItem}
          className="flex items-center gap-1.5 py-2 px-3.5 rounded-xl bg-gradient-to-r from-indigo-600 to-indigo-500 hover:from-indigo-500 hover:to-indigo-400 text-white font-medium text-xs transition-all shadow-md shadow-indigo-600/20 shrink-0 cursor-pointer"
        >
          <Plus className="w-4 h-4" />
          <span>New Item</span>
        </button>
      </div>
    </header>
  );
}
