import { type BibliotekarDTO, type ClanDTO } from './library';

const AUTH_KEY = 'library_app_user';
const TOKEN_KEY = 'jwt_token';

export const authService = {

  // Cuva JWT token koji dolazi sa bekenda nakon prijave
  saveToken: (token: string) => {
    sessionStorage.setItem(TOKEN_KEY, token);
  },

  getToken: (): string | null => sessionStorage.getItem(TOKEN_KEY),

  // Cuva podatke o korisniku (ime, jmbg...) za prikaz u UI-ju
  saveSession: (userData: BibliotekarDTO | ClanDTO) => {
    sessionStorage.setItem(AUTH_KEY, JSON.stringify(userData));
  },

  getCurrentUser: (): (BibliotekarDTO | ClanDTO) | null => {
    const data = sessionStorage.getItem(AUTH_KEY);
    if (!data) return null;
    try { return JSON.parse(data); } catch { return null; }
  },

  // Provjerava da li je korisnik ulogovan (token postoji u sesiji)
  isLoggedIn: (): boolean => sessionStorage.getItem(TOKEN_KEY) !== null,

  logout: () => {
    sessionStorage.removeItem(AUTH_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem('role');
    window.location.href = '/login';
  }
};