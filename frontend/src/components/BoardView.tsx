import { useEffect, useState, type DragEvent, type FormEvent } from 'react';
import { api } from '../api';
import type { BoardDetail } from '../types';

export function BoardView({ boardId, onBack }: { boardId: string; onBack: () => void }) {
  const [board, setBoard] = useState<BoardDetail | null>(null);
  const [newColumn, setNewColumn] = useState('');
  const [dragCardId, setDragCardId] = useState<string | null>(null);

  async function refresh() {
    setBoard(await api.getBoard(boardId));
  }

  useEffect(() => {
    refresh();
  }, [boardId]);

  async function addCard(columnId: string, title: string) {
    if (!title.trim()) return;
    await api.createCard(columnId, title.trim());
    await refresh();
  }

  async function deleteCard(cardId: string) {
    await api.deleteCard(cardId);
    await refresh();
  }

  async function addColumn(e: FormEvent) {
    e.preventDefault();
    if (!newColumn.trim()) return;
    await api.addColumn(boardId, newColumn.trim());
    setNewColumn('');
    await refresh();
  }

  async function onDrop(columnId: string) {
    if (!dragCardId) return;
    const targetCount = board?.columns.find((c) => c.id === columnId)?.cards.length ?? 0;
    await api.moveCard(dragCardId, columnId, targetCount);
    setDragCardId(null);
    await refresh();
  }

  if (!board) return <p className="muted">Loading board…</p>;

  return (
    <div className="board-view">
      <div className="board-head">
        <button className="link" onClick={onBack}>
          ← Boards
        </button>
        <h2>{board.title}</h2>
        <form className="new-column" onSubmit={addColumn}>
          <input
            placeholder="+ column"
            value={newColumn}
            onChange={(e) => setNewColumn(e.target.value)}
          />
        </form>
      </div>

      <div className="columns">
        {board.columns.map((col) => (
          <div
            key={col.id}
            className="column"
            onDragOver={(e: DragEvent) => e.preventDefault()}
            onDrop={() => onDrop(col.id)}
          >
            <div className="column-head">
              <span>{col.title}</span>
              <span className="count">{col.cards.length}</span>
            </div>

            <div className="cards">
              {col.cards.map((card) => (
                <div
                  key={card.id}
                  className="card"
                  draggable
                  onDragStart={() => setDragCardId(card.id)}
                >
                  <span>{card.title}</span>
                  <button className="card-del" onClick={() => deleteCard(card.id)}>
                    ✕
                  </button>
                </div>
              ))}
            </div>

            <AddCard onAdd={(t) => addCard(col.id, t)} />
          </div>
        ))}
      </div>
    </div>
  );
}

function AddCard({ onAdd }: { onAdd: (title: string) => void }) {
  const [title, setTitle] = useState('');
  return (
    <form
      className="add-card"
      onSubmit={(e) => {
        e.preventDefault();
        onAdd(title);
        setTitle('');
      }}
    >
      <input placeholder="+ Add a card" value={title} onChange={(e) => setTitle(e.target.value)} />
    </form>
  );
}
