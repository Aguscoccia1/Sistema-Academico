import { Bell, Menu } from 'lucide-react';

function Header({ activeSection, rolSimulado, showNotifications, onToggleNotifications, onOpenSidebar }) {
  return (
    <header className="app-header">
      <div className="header-content">
        <button type="button" className="menu-toggle" onClick={onOpenSidebar} aria-label="Abrir menú">
          <Menu size={24} />
        </button>
        <div className="header-title">
          <h1>{activeSection}</h1>
        </div>
        <div className="header-actions">
          <button
            type="button"
            className="notifications-button"
            onClick={onToggleNotifications}
            aria-label="Notificaciones"
            aria-pressed={showNotifications}
          >
            <Bell size={20} />
          </button>
        </div>
      </div>
    </header>
  );
}

export default Header;
