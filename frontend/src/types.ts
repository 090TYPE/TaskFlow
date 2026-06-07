export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  displayName: string;
}

export interface BoardSummary {
  id: string;
  title: string;
  createdAt: string;
  columnCount: number;
}

export interface Card {
  id: string;
  title: string;
  description: string;
  order: number;
  columnId: string;
  createdAt: string;
}

export interface Column {
  id: string;
  title: string;
  order: number;
  cards: Card[];
}

export interface BoardDetail {
  id: string;
  title: string;
  createdAt: string;
  columns: Column[];
}
