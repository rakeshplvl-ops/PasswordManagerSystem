import { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ShieldCheck, Loader2 } from 'lucide-react';
import { AuthProvider, useAuth } from './auth/AuthContext';
import { useVaultStore } from './vault/vaultStore';
import { AuthPage } from './components/AuthPage';
import { Dashboard } from './components/Dashboard';
import { UnlockModal } from './components/UnlockModal';
import { ToastContainer } from './components/ToastContainer';

function MainAppContent() {
  const { isAuthenticated, isLoading } = useAuth();
  const isUnlocked = useVaultStore((state) => state.isUnlocked);

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-[#090d16] text-slate-100 gap-3">
        <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-indigo-500/20 to-cyan-500/20 border border-indigo-500/30 flex items-center justify-center text-cyan-400">
          <ShieldCheck className="w-6 h-6 animate-pulse" />
        </div>
        <div className="flex items-center gap-2 text-xs text-slate-400 font-mono">
          <Loader2 className="w-3.5 h-3.5 animate-spin text-indigo-400" />
          <span>Initializing secure environment...</span>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <>
        <AuthPage />
        <ToastContainer />
      </>
    );
  }

  return (
    <>
      <Dashboard />
      {!isUnlocked && <UnlockModal />}
      <ToastContainer />
    </>
  );
}

export default function App() {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            refetchOnWindowFocus: false,
            retry: 1,
            staleTime: 1000 * 60, // 1 minute
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <MainAppContent />
      </AuthProvider>
    </QueryClientProvider>
  );
}
