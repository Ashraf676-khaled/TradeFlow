import axios from 'axios';

// Configurable API base URL, defaulting to the ASP.NET Core backend port
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:57679';
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // Never let a request hang forever (30s max)
  headers: {
    'Content-Type': 'application/json',
  },
});

// Extracts a human-friendly Arabic message from a backend ProblemDetails/ValidationProblem response.
export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { detail?: string; title?: string; errors?: Record<string, string[]> }
      | undefined;

    if (data?.detail) return data.detail;

    // Surface the specific field messages before the generic ProblemDetails
    // title (e.g. "One or more validation errors occurred.") so the user sees
    // the real reason instead of a generic sentence.
    const validationMessages = data?.errors ? Object.values(data.errors).flat() : [];
    if (validationMessages.length > 0 && validationMessages[0]) return validationMessages[0];

    if (data?.title) return data.title;

    if (!error.response) return 'تعذر الاتصال بالخادم. تحقق من اتصالك بالإنترنت وحاول مرة أخرى.';
    return fallback;
  }

  if (error instanceof Error && error.message && !error.message.startsWith('Request failed')) {
    return error.message;
  }

  return fallback;
}

// Interceptor to attach Authorization Bearer token to all outgoing backend requests
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('tradeflow_access_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor to catch 401 Unauthorized responses and trigger logout
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('tradeflow_access_token');
      if (localStorage.getItem('tradeflow_user_email')) {
        window.dispatchEvent(new Event('tradeflow_auth_logout'));
      }
    }
    return Promise.reject(error);
  }
);
