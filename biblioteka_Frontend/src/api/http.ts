import axios from 'axios';


const apiBase = 'https://localhost:7271';

const http = axios.create({
  baseURL: apiBase,
  headers: { 'Content-Type': 'application/json' },
});

// Automatski ubacuje JWT token u svaki zahtev prema bekendu
http.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('jwt_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Ako bekend vrati 401 (token istekao/neispravan), automatski odjavi korisnika
http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      sessionStorage.clear();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default http;