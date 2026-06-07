import { useEffect, useState, type FormEvent } from 'react';
import { api } from '../api';
import type { BoardSummary } from '../types';

export function BoardList({ onOpen }: { onOpen: (id: string) => void }) {
  const [boards, setBoards] = useState<BoardSummary[]>([]);
  const [title, setTitle] = useState('');
  const [loading, setLoading] = useState(true);

  async function refresh() {
    setBoards(await api.listBoards());
    setLoading(false);
  }

  useEffect(() => {
    refresh();
  }, []);

  async function create(e: FormEvent) {
    e.preventDefault();
    if (!title.trim()) return;
    const board = await api.createBoard(title.trim());
    setTitle('');
    await refresh();
    onOpen(board.id);
  }

  async function remove(id: string) {
    if (!confirm('Delete this board?')) return;
    await api.deleteBoard(id);
    await refresh();
  }

  return (
    <div className="board-list">
      <form className="new-board" onSubmit={create}>
        <input
          placeholder="New board title…"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <button type="submit">Create board</button>
      </form>

      {loading ? (
        <p className="muted">Loading…</p>
      ) : boards.length === 0 ? (
        <p className="muted">No boards yet — create your first one above.</p>
      ) : (
        <ul className="boards">
          {boards.map((b) => (
            <li key={b.id} className="board-tile">
              <button className="tile-open" onClick={() => onOpen(b.id)}>
                <strong>{b.title}</strong>
                <span className="muted">{b.columnCount} columns</span>
              </button>
              <button className="danger" onClick={() => remove(b.id)} title="Delete">
                ✕
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
