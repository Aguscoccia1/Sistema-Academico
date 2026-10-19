
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
<<<<<<< HEAD
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
=======
          <button
            type="button"
            className="notifications-button"
            onClick={onToggleNotifications}
            aria-label="Notificaciones"
            aria-pressed={showNotifications}
          >
            <Bell size={20} />
          </button>
>>>>>>> 8e032f25645166d7dc5245e9d31310f5accbef4d
        </div>

      </div>
    </header>
  );
}

export default Header;
