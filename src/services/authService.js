import { request } from './httpClient';

// AuthController: /api/auth (público)
export const authService = {
  login: (email, password) =>
    request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
};
