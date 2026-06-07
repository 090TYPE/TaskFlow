import type {
  AuthResponse,
  BoardDetail,
  BoardSummary,
  Card,
  Column,
} from './types';

const BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api';

const TOKEN_KEY = 'taskflow.token';

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (t: string) => localStorage.setItem(TOKEN_KEY, t),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = tokenStore.get();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(`${BASE}${path}`, { ...options, headers });

  if (res.status === 204) return undefined as T;

  const body = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new ApiError(res.status, body?.message ?? `Request failed (${res.status})`);
  }
  return body as T;
}

export const api = {
  // --- auth ---
  register: (email: string, password: string, displayName: string) =>
    request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password, displayName }),
    }),

  login: (email: string, password: string) =>
    request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  // --- boards ---
  listBoards: () => request<BoardSummary[]>('/boards'),
  getBoard: (id: string) => request<BoardDetail>(`/boards/${id}`),
  createBoard: (title: string) =>
    request<BoardDetail>('/boards', { method: 'POST', body: JSON.stringify({ title }) }),
  deleteBoard: (id: string) => request<void>(`/boards/${id}`, { method: 'DELETE' }),
  addColumn: (boardId: string, title: string) =>
    request<Column>(`/boards/${boardId}/columns`, {
      method: 'POST',
      body: JSON.stringify({ title }),
    }),

  // --- cards ---
  createCard: (columnId: string, title: string, description?: string) =>
    request<Card>(`/columns/${columnId}/cards`, {
      method: 'POST',
      body: JSON.stringify({ title, description }),
    }),
  updateCard: (cardId: string, title: string, description?: string) =>
    request<Card>(`/cards/${cardId}`, {
      method: 'PUT',
      body: JSON.stringify({ title, description }),
    }),
  moveCard: (cardId: string, targetColumnId: string, order: number) =>
    request<Card>(`/cards/${cardId}/move`, {
      method: 'PUT',
      body: JSON.stringify({ targetColumnId, order }),
    }),
  deleteCard: (cardId: string) => request<void>(`/cards/${cardId}`, { method: 'DELETE' }),
};

export { ApiError };
