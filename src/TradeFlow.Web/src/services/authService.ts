import { apiClient } from './apiClient';

export interface AuthApiResponse {
  accessToken: string | null;
  accessTokenExpiresUtc: string | null;
  requiresTwoFactor: boolean;
  email: string | null;
}

export interface UserSession {
  email: string;
  name: string;
  token: string;
}

export type AuthOutcome =
  | { requiresTwoFactor: true; email: string }
  | { requiresTwoFactor: false; session: UserSession };

export class AuthService {
  async login(email: string, password: string): Promise<AuthOutcome> {
    const response = await apiClient.post<AuthApiResponse>(
      '/api/auth/login',
      { email, password },
      { withCredentials: true },
    );

    return this.createOutcome(response.data, email, email.split('@')[0]);
  }

  async register(
    email: string,
    password: string,
    name: string,
    companyName: string,
  ): Promise<AuthOutcome> {
    const response = await apiClient.post<AuthApiResponse>(
      '/api/auth/register',
      {
        companyName,
        fullName: name,
        email,
        password,
      },
      { withCredentials: true },
    );

    return this.createOutcome(response.data, email, name);
  }

  async verifyOtp(email: string, code: string): Promise<AuthOutcome> {
    const response = await apiClient.post<AuthApiResponse>(
      '/api/auth/verify-otp',
      { email, code },
      { withCredentials: true },
    );

    return this.createOutcome(response.data, email, email.split('@')[0]);
  }

  async googleLogin(idToken: string): Promise<AuthOutcome> {
    const response = await apiClient.post<AuthApiResponse>(
      '/api/auth/google-login',
      { idToken },
      { withCredentials: true },
    );

    return this.createOutcome(response.data, response.data.email ?? '', response.data.email?.split('@')[0] ?? '');
  }

  private createOutcome(response: AuthApiResponse, email: string, name: string): AuthOutcome {
    if (response.requiresTwoFactor) {
      if (!response.email) {
        throw new Error('The API did not return the email required for OTP verification.');
      }

      return { requiresTwoFactor: true, email: response.email };
    }

    if (!response.accessToken || !response.accessTokenExpiresUtc || !email) {
      throw new Error('The API response did not contain a complete authentication result.');
    }

    const session = { email, name, token: response.accessToken };
    localStorage.setItem('tradeflow_access_token', session.token);
    localStorage.setItem('tradeflow_user_email', session.email);
    localStorage.setItem('tradeflow_user_name', session.name);

    return { requiresTwoFactor: false, session };
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
