interface TokenPayload {
  sub: string;
  email: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;
  exp: number;
  iss: string;
  aud: string;
}

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("token");
}

export function setToken(token: string): void {
  localStorage.setItem("token", token);
  document.cookie = `token=${token}; path=/; max-age=${60 * 60 * 24 * 7}; SameSite=Lax`;
}

export function removeToken(): void {
  localStorage.removeItem("token");
  document.cookie = "token=; path=/; max-age=0";
}

export function decodeToken(token: string): TokenPayload | null {
  try {
    const payload = token.split(".")[1];
    const decoded = atob(payload);
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

export function getUserFromToken(token: string) {
  const payload = decodeToken(token);
  if (!payload) return null;
  return {
    userId: payload.sub,
    email: payload.email,
    role: payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    exp: payload.exp,
  };
}

export function isTokenExpired(token: string): boolean {
  const payload = decodeToken(token);
  if (!payload) return true;
  return Date.now() >= payload.exp * 1000;
}
