const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5010";

interface ApiError {
  error?: string;
  title?: string;
  detail?: string;
  errors?: string[];
}

class ApiClient {
  private getToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem("token");
  }

  private async request<T>(
    path: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.getToken();
    const headers: Record<string, string> = {
      ...(options.headers as Record<string, string>),
    };

    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    // Don't set Content-Type for FormData (browser sets it with boundary)
    if (!(options.body instanceof FormData)) {
      headers["Content-Type"] = "application/json";
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers,
    });

    if (response.status === 401) {
      const errorData: ApiError = await response.json().catch(() => ({}));
      const message = errorData.detail || errorData.error || errorData.title || "Unauthorized";
      // Only auto-redirect for expired tokens on non-auth routes
      const isAuthRoute = path.includes("/auth/login") || path.includes("/auth/register") || path.includes("/auth/verify");
      if (!isAuthRoute && typeof window !== "undefined") {
        localStorage.removeItem("token");
        window.location.href = "/login";
      }
      throw new Error(message);
    }

    if (!response.ok) {
      const errorData: ApiError = await response.json().catch(() => ({}));
      const message =
        errorData.error ||
        errorData.detail ||
        errorData.title ||
        errorData.errors?.join(", ") ||
        `Request failed with status ${response.status}`;
      throw new Error(message);
    }

    if (response.status === 204) return {} as T;
    return response.json();
  }

  async get<T>(path: string): Promise<T> {
    return this.request<T>(path);
  }

  async post<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, {
      method: "POST",
      body: body instanceof FormData ? body : JSON.stringify(body),
    });
  }

  async put<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, {
      method: "PUT",
      body: JSON.stringify(body),
    });
  }

  async patch<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, {
      method: "PATCH",
      body: JSON.stringify(body),
    });
  }

  async del<T>(path: string): Promise<T> {
    return this.request<T>(path, { method: "DELETE" });
  }

  async upload<T>(path: string, formData: FormData): Promise<T> {
    return this.request<T>(path, {
      method: "POST",
      body: formData,
    });
  }
}

export const api = new ApiClient();
