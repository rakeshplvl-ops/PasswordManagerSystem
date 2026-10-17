import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useVaultStore } from '../vault/vaultStore';
import { VaultService } from '../vault/vaultService';
import { vaultApi } from '../vault/api';
import type { DecryptedFolder, DecryptedVaultEntry } from '../vault/types';
import { Sidebar, type FilterCategory } from './Sidebar';
import { Header } from './Header';
import { VaultList } from './VaultList';
import { VaultItemModal } from './VaultItemModal';
import { FolderModal } from './FolderModal';
import { PasswordGeneratorModal } from './PasswordGeneratorModal';
import { useToast } from './useToast';

export function Dashboard() {
  const [selectedCategory, setSelectedCategory] = useState<FilterCategory>('all');
  const [selectedFolderId, setSelectedFolderId] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // Modals state
  const [isItemModalOpen, setIsItemModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<DecryptedVaultEntry | null>(null);
  const [isFolderModalOpen, setIsFolderModalOpen] = useState(false);
  const [editingFolder, setEditingFolder] = useState<DecryptedFolder | null>(null);
  const [isGeneratorOpen, setIsGeneratorOpen] = useState(false);

  const vek = useVaultStore((state) => state.vek);
  const showToast = useToast((state) => state.showToast);
  const queryClient = useQueryClient();

  // Query: Fetch and decrypt vault entries
  const { data: entries = [], isLoading: isLoadingEntries } = useQuery({
    queryKey: ['vault-entries', !!vek],
    queryFn: async () => {
      if (!vek) return [];
      const rawEntries = await vaultApi.getEntries(undefined, true); // fetch both active & deleted
      return await VaultService.decryptEntries(rawEntries, vek);
    },
    enabled: !!vek,
  });

  // Query: Fetch and decrypt folders
  const { data: folders = [] } = useQuery({
    queryKey: ['vault-folders', !!vek],
    queryFn: async () => {
      if (!vek) return [];
      const rawFolders = await vaultApi.getFolders();
      return await VaultService.decryptFolders(rawFolders, vek);
    },
    enabled: !!vek,
  });

  // Mutations
  const softDeleteMutation = useMutation({
    mutationFn: (id: string) => vaultApi.softDeleteEntry(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vault-entries'] });
      showToast('Item moved to trash', 'info');
    },
    onError: (err: Error) => showToast(err.message, 'error'),
  });

  const restoreMutation = useMutation({
    mutationFn: (id: string) => vaultApi.restoreEntry(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vault-entries'] });
      showToast('Item restored from trash', 'success');
    },
    onError: (err: Error) => showToast(err.message, 'error'),
  });

  const purgeMutation = useMutation({
    mutationFn: (id: string) => vaultApi.purgeEntry(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vault-entries'] });
      showToast('Item permanently deleted', 'info');
    },
    onError: (err: Error) => showToast(err.message, 'error'),
  });

  const deleteFolderMutation = useMutation({
    mutationFn: (folderId: string) => vaultApi.deleteFolder(folderId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vault-folders'] });
      if (selectedFolderId) setSelectedFolderId(null);
      showToast('Folder deleted', 'info');
    },
    onError: (err: Error) => showToast(err.message, 'error'),
  });

  // Filter and search entries
  const filteredEntries = useMemo(() => {
    return entries.filter((entry) => {
      // 1. Trash status
      if (selectedCategory === 'trash') {
        if (!entry.deletedAt) return false;
      } else {
        if (entry.deletedAt) return false;
      }

      // 2. Folder filter
      if (selectedFolderId !== null && entry.folderId !== selectedFolderId) {
        return false;
      }

      // 3. Category filter
      if (selectedCategory === 'notes') {
        if (entry.data.password || !entry.data.notes) return false;
      }

      // 4. Search query
      if (searchQuery.trim()) {
        const query = searchQuery.toLowerCase();
        const titleMatch = entry.data.title.toLowerCase().includes(query);
        const userMatch = (entry.data.username || '').toLowerCase().includes(query);
        const websiteMatch = (entry.data.website || '').toLowerCase().includes(query);
        const notesMatch = (entry.data.notes || '').toLowerCase().includes(query);

        if (!titleMatch && !userMatch && !websiteMatch && !notesMatch) {
          return false;
        }
      }

      return true;
    });
  }, [entries, selectedCategory, selectedFolderId, searchQuery]);

  // Compute view title
  const activeTitle = useMemo(() => {
    if (selectedFolderId) {
      const folder = folders.find((f) => f.id === selectedFolderId);
      return folder ? `Folder: ${folder.name}` : 'Folder';
    }
    switch (selectedCategory) {
      case 'trash':
        return 'Trash';
      case 'notes':
        return 'Secure Notes';
      case 'logins':
        return 'Logins';
      default:
        return 'All Items';
    }
  }, [selectedCategory, selectedFolderId, folders]);

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-[#090d16] text-slate-100">
      <Sidebar
        selectedCategory={selectedCategory}
        selectedFolderId={selectedFolderId}
        onSelectCategory={(cat) => {
          setSelectedCategory(cat);
          setSelectedFolderId(null);
        }}
        onSelectFolder={(folderId) => {
          setSelectedFolderId(folderId);
          setSelectedCategory('all');
        }}
        entries={entries}
        folders={folders}
        onOpenCreateFolder={() => {
          setEditingFolder(null);
          setIsFolderModalOpen(true);
        }}
        onOpenEditFolder={(folder) => {
          setEditingFolder(folder);
          setIsFolderModalOpen(true);
        }}
        onDeleteFolder={(folderId) => deleteFolderMutation.mutate(folderId)}
        onOpenGenerator={() => setIsGeneratorOpen(true)}
      />

      <div className="flex-1 flex flex-col min-w-0">
        <Header
          searchQuery={searchQuery}
          onSearchChange={setSearchQuery}
          onOpenCreateItem={() => {
            setEditingItem(null);
            setIsItemModalOpen(true);
          }}
          title={activeTitle}
        />

        <main className="flex-1 flex overflow-hidden">
          {isLoadingEntries ? (
            <div className="flex-1 flex items-center justify-center text-slate-500 text-sm">
              Decrypting zero-knowledge vault items...
            </div>
          ) : (
            <VaultList
              entries={filteredEntries}
              folders={folders}
              isTrashView={selectedCategory === 'trash'}
              onEdit={(item) => {
                setEditingItem(item);
                setIsItemModalOpen(true);
              }}
              onSoftDelete={(id) => softDeleteMutation.mutate(id)}
              onRestore={(id) => restoreMutation.mutate(id)}
              onPurge={(id) => purgeMutation.mutate(id)}
              onOpenCreate={() => {
                setEditingItem(null);
                setIsItemModalOpen(true);
              }}
            />
          )}
        </main>
      </div>

      {/* Modals */}
      <VaultItemModal
        isOpen={isItemModalOpen}
        onClose={() => setIsItemModalOpen(false)}
        item={editingItem}
        folders={folders}
        defaultFolderId={selectedFolderId}
        onSaved={() => queryClient.invalidateQueries({ queryKey: ['vault-entries'] })}
      />

      <FolderModal
        isOpen={isFolderModalOpen}
        onClose={() => setIsFolderModalOpen(false)}
        folder={editingFolder}
        onSaved={() => queryClient.invalidateQueries({ queryKey: ['vault-folders'] })}
      />

      <PasswordGeneratorModal
        isOpen={isGeneratorOpen}
        onClose={() => setIsGeneratorOpen(false)}
      />
    </div>
  );
}
