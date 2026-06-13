import axios from 'axios';
import type { ApiResponse } from '../types';

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api/customer';

const BASE_INDEX = API_BASE.indexOf('/api/');
export const SERVER_BASE = BASE_INDEX >= 0 ? API_BASE.substring(0, BASE_INDEX) : API_BASE;

export const api = axios.create({
  baseURL: API_BASE,
});

api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    const status = err.response?.status;
    const requestUrl: string = err.config?.url ?? '';
    const isAuthRequest =
      requestUrl.includes('/auth/login') ||
      requestUrl.includes('/auth/register');

    if (status === 401 && !isAuthRequest && sessionStorage.getItem('token')) {
      sessionStorage.removeItem('token');
      sessionStorage.removeItem('user');
      if (!window.location.pathname.startsWith('/login')) {
        window.location.replace('/login');
      }
    }
    return Promise.reject(err);
  },
);

export async function apiGet<T>(url: string): Promise<T> {
  const res = await api.get<ApiResponse<T>>(url);
  return unwrapApiResponse(res.data);
}

function throwApiError(err: unknown): never {
  if (axios.isAxiosError(err) && err.response?.data) {
    const data = err.response.data as ApiResponse<unknown>;
    if (data.error) throw new Error(data.error);
  }
  throw err;
}

function unwrapApiResponse<T>(response: ApiResponse<T>): T {
  if (!response.success) throw new Error(response.error ?? 'Unknown error');
  return response.data as T;
}

async function apiSend<T>(
  method: 'post' | 'put',
  url: string,
  body?: unknown,
): Promise<T> {
  try {
    const res = await api[method]<ApiResponse<T>>(url, body);
    return unwrapApiResponse(res.data);
  } catch (err) {
    throwApiError(err);
  }
}

export async function apiPost<T>(url: string, body?: unknown): Promise<T> {
  return apiSend('post', url, body);
}

export async function apiPut<T>(url: string, body?: unknown): Promise<T> {
  return apiSend('put', url, body);
}

export async function apiDelete<T>(url: string): Promise<T> {
  const res = await api.delete<ApiResponse<T>>(url);
  return unwrapApiResponse(res.data);
}

/** Публичные эндпоинты без JWT (лендинг). */
export async function apiGetPublic<T>(path: string): Promise<T> {
  const url = `${SERVER_BASE}${path.startsWith('/') ? path : `/${path}`}`;
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const body = (await res.json()) as ApiResponse<T>;
  return unwrapApiResponse(body);
}
