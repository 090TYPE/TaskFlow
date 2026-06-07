import { useState } from 'react';
import { useAuth } from './auth';
import { AuthForm } from './components/AuthForm';
import { BoardList } from './components/BoardList';
import { BoardView } from './components/BoardView';
import './App.css';

export default function App() {
  const { user, logout } = useAuth();
  const [openBoardId, setOpenBoardId] = useState<string | null>(null);

  if (!user) return <AuthForm />;

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand" onClick={() => setOpenBoardId(null)}>
          TaskFlow
        </div>
        <div className="user">
          <span className="muted">{user.displayName || user.email}</span>
          <button className="link" onClick={logout}>
            Log out
          </button>
        </div>
      </header>

      <main>
        {openBoardId ? (
          <BoardView boardId={openBoardId} onBack={() => setOpenBoardId(null)} />
        ) : (
          <BoardList onOpen={setOpenBoardId} />
        )}
      </main>
    </div>
  );
}
