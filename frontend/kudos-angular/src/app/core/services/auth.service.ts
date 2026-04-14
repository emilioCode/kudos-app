import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  userId?: string;
  id?: string;
  name: string;
  email: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7162/api';

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, {
      email,
      password
    });
  }

  register(name: string, email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register`, {
      name,
      email,
      password
    });
  }

  logout(): void {
    if (!this.canUseStorage()) {
      return;
    }

    localStorage.removeItem('kudos_token');
    localStorage.removeItem('kudos_user');
  }

  saveSession(response: AuthResponse): void {
    if (!this.canUseStorage()) {
      return;
    }

    localStorage.setItem('kudos_token', response.token);
    localStorage.setItem(
      'kudos_user',
      JSON.stringify({
        userId: response.userId ?? response.id ?? '',
        name: response.name,
        email: response.email,
        role: response.role
      } satisfies AuthUser)
    );
  }

  getToken(): string | null {
    if (!this.canUseStorage()) {
      return null;
    }

    return localStorage.getItem('kudos_token');
  }

  getUser(): AuthUser | null {
    if (!this.canUseStorage()) {
      return null;
    }

    const rawUser = localStorage.getItem('kudos_user');
    if (!rawUser) {
      return null;
    }

    try {
      const parsed = JSON.parse(rawUser) as Partial<AuthUser>;
      if (
        typeof parsed.userId === 'string' &&
        typeof parsed.name === 'string' &&
        typeof parsed.email === 'string' &&
        typeof parsed.role === 'string'
      ) {
        return {
          userId: parsed.userId,
          name: parsed.name,
          email: parsed.email,
          role: parsed.role
        };
      }
    } catch {
      return null;
    }

    return null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.getUser()?.role === 'Admin';
  }

  private canUseStorage(): boolean {
    return typeof window !== 'undefined' && !!window.localStorage;
  }
}
