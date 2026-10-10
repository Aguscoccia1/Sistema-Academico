
import { Bell } from 'lucide-react';

function Header({
  activeSection,
  showNotifications,
  showNotificationButton = true,
  onToggleNotifications,
}) {
  return (
    <header className="app-header">
      <div className="header-content">

        <div className="header-title">
          <h1>{activeSection}</h1>
        </div>

        <div className="header-actions">
          {showNotificationButton && (
            <button
              type="button"
              className="notifications-button"
              onClick={onToggleNotifications}
              aria-label="Notificaciones"
              aria-pressed={showNotifications}
            >
              <Bell size={20} />
              <span className="notification-badge">3</span>
            </button>
          )}
        </div>

      </div>
    </header>
  );
}

export default Header;
