import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { apiPost } from '../api/client';
import type { UserDto, AuthResponse } from '../types';

interface AuthContextType {
  user: UserDto | null;
  token: string | null;
  login: (phone: string, pin: string) => Promise<void>;
  register: (fullName: string, phone: string, pin: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

function persistSession(token: string, user: UserDto) {
  sessionStorage.setItem('token', token);
  sessionStorage.setItem('user', JSON.stringify(user));
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(() => {
    const stored = sessionStorage.getItem('user');
    return stored ? JSON.parse(stored) : null;
  });
  const [token, setToken] = useState<string | null>(() => sessionStorage.getItem('token'));

  useEffect(() => {
    if (token) {
      sessionStorage.setItem('token', token);
    } else {
      sessionStorage.removeItem('token');
    }
  }, [token]);

  useEffect(() => {
    if (user) {
      sessionStorage.setItem('user', JSON.stringify(user));
    } else {
      sessionStorage.removeItem('user');
    }
  }, [user]);

  const login = async (phone: string, pin: string) => {
    const res = await apiPost<AuthResponse>('/auth/login', { phone, pin });
    persistSession(res.token, res.user);
    setToken(res.token);
    setUser(res.user);
  };

  const register = async (fullName: string, phone: string, pin: string) => {
    const res = await apiPost<AuthResponse>('/auth/register', { fullName, phone, pin });
    persistSession(res.token, res.user);
    setToken(res.token);
    setUser(res.user);
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    sessionStorage.clear();
  };

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout, isAuthenticated: !!token }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
