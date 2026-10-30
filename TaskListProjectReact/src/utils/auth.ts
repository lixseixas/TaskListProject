const TOKEN_KEY = 'jwt_token';
const USER_KEY = 'user';

export const authUtils = {
  setToken: (token: string) => {
    localStorage.setItem(TOKEN_KEY, token);
  },

  getToken: (): string | null => {
    return localStorage.getItem(TOKEN_KEY);
  },

  removeToken: () => {
    localStorage.removeItem(TOKEN_KEY);
  },

  setUser: (user: string) => {
    localStorage.setItem(USER_KEY, user);
  },

  getUser: (): string | null => {
    return localStorage.getItem(USER_KEY);
  },

  removeUser: () => {
    localStorage.removeItem(USER_KEY);
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem(TOKEN_KEY);
  },

  logout: () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },
};
