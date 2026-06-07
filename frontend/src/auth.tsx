import { createContext, useContext, useState, type ReactNode } from 'react';
import { api, tokenStore } from './api';
import type { AuthResponse } from './types';

interface AuthState {
  user: { id: string; email: string; displayName: string } | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

function decodeStoredUser(): AuthState['user'] {
  const token = tokenStore.get();
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    // exp is in seconds; drop expired tokens
    if (payload.exp && payload.exp * 1000 < Date.now()) {
      tokenStore.clear();
      return null;
    }
    return { id: payload.sub, email: payload.email, displayName: payload.name ?? '' };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthState['user']>(decodeStoredUser);

  const apply = (res: AuthResponse) => {
    tokenStore.set(res.token);
    setUser({ id: res.userId, email: res.email, displayName: res.displayName });
  };

  const value: AuthState = {
    user,
    login: async (email, password) => apply(await api.login(email, password)),
    register: async (email, password, displayName) =>
      apply(await api.register(email, password, displayName)),
    logout: () => {
      tokenStore.clear();
      setUser(null);
    },
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
