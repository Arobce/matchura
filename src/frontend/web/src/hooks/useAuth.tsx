"use client";

import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from "react";
import { getToken, setToken as saveToken, removeToken, getUserFromToken, isTokenExpired } from "@/lib/auth";
import { api } from "@/lib/api";
import type { AuthResponse, LoginRequest, RegisterRequest } from "@/lib/types";

interface User {
  userId: string;
  email: string;
  role: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = getToken();
    if (token && !isTokenExpired(token)) {
      const userData = getUserFromToken(token);
      if (userData) {
        setUser({ userId: userData.userId, email: userData.email, role: userData.role });
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const response = await api.post<AuthResponse>("/api/auth/login", data);
    saveToken(response.token);
    const userData = getUserFromToken(response.token);
    if (userData) {
      setUser({ userId: userData.userId, email: userData.email, role: userData.role });
    }
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    await api.post("/api/auth/register", data);
  }, []);

  const logout = useCallback(() => {
    removeToken();
    setUser(null);
    window.location.href = "/login";
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
