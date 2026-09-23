import { apiClient } from './apiClient';

export interface LoginResponse {
  accessToken: string;
  accessTokenExpiresUtc: string;
}

export interface RegisterResponse {
  accessToken: string;
  accessTokenExpiresUtc: string;
}

export interface UserSession {
  email: string;
  name: string;
  token: string;
}

export class AuthService {
  async login(email: string, password: string): Promise<UserSession> {
    const response = await apiClient.post<LoginResponse>('/api/auth/login', {
      email,
      password,
    });

    const token = response.data.accessToken;
    localStorage.setItem('tradeflow_access_token', token);
    localStorage.setItem('tradeflow_user_email', email);

    return {
      email,
      name: email.split('@')[0],
      token,
    };
  }

  async register(email: string, password: string, name: string, companyName: string): Promise<UserSession> {
    const response = await apiClient.post<RegisterResponse>('/api/auth/register', {
      companyName,
      fullName: name,
      name,
      email,
      password,
    });

    const token = response.data.accessToken;
    localStorage.setItem('tradeflow_access_token', token);
    localStorage.setItem('tradeflow_user_email', email);
    localStorage.setItem('tradeflow_user_name', name);

    return { email, name, token };
  }

  logout(): void {
    localStorage.removeItem('tradeflow_access_token');
    localStorage.removeItem('tradeflow_user_email');
    localStorage.removeItem('tradeflow_user_name');
    window.dispatchEvent(new Event('tradeflow_auth_logout'));
  }

  clearSession(): void {
    localStorage.removeItem('tradeflow_access_token');
    localStorage.removeItem('tradeflow_user_email');
    localStorage.removeItem('tradeflow_user_name');
  }

  getCurrentSession(): UserSession | null {
    const token = localStorage.getItem('tradeflow_access_token');
    const email = localStorage.getItem('tradeflow_user_email');
    if (!token || !email) return null;
    const savedName = localStorage.getItem('tradeflow_user_name');

    return {
      email,
      name: savedName || email.split('@')[0],
      token,
    };
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('tradeflow_access_token');
  }
}

export const authService = new AuthService();
