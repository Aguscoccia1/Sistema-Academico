import { request } from './httpClient';

// AuthController: /api/auth (público)
export const authService = {
  login: (email, password, remember = false) =>
    request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password, remember }),
    }),
};
