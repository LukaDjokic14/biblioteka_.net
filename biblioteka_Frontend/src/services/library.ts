import http from '../api/http';
import { authService } from './auth';

export interface GradDTO { gradId: number; naziv: string; }
export interface AutorDTO { autorId: number; ime: string; prezime: string; biografija: string; }
export interface BibliotekarDTO { jmbg: string; ime: string; prezime: string; korisnickoIme: string; lozinka?: string; }
export interface ClanDTO { jmbg: string; ime: string; prezime: string; brojTelefona: string; korisnickoIme: string; lozinka?: string; grad: GradDTO; }
export interface KnjigaDTO { knjigaId: number; naslov: string; godinaIzdanja: number; isbn: string; zanr: string; slika: string; brojStrana: number; opis: string; }

// AŽURIRANO: IzdavanjeDTO nema više 'clan' objekat, već flat polja
export interface IzdavanjeDTO { 
  izdavanjeId: number; 
  datumIzdavanja: string; 
  datumVracanja: string | null; 
  status: string; 
  napomena: string; 
  knjige: KnjigaDTO[]; 
  clanJmbg: string;
  clanIme: string;
  clanPrezime: string;
  bibliotekarImePrezime: string;
}

export interface PisanjeDTO { knjiga: KnjigaDTO; autor: AutorDTO; }

function mapClan(raw: any): ClanDTO {
  return {
    jmbg: raw.jmbg,
    ime: raw.ime,
    prezime: raw.prezime,
    brojTelefona: raw.brojTelefona ?? '',
    korisnickoIme: raw.korisnickoIme ?? '',
    grad: { gradId: raw.gradId ?? 0, naziv: raw.gradNaziv ?? '' }
  };
}

// AŽURIRANO: Mapiranje za novi format podataka
function mapIzdavanje(raw: any): IzdavanjeDTO {
  return {
    izdavanjeId: raw.izdavanjeId,
    datumIzdavanja: raw.datumIzdavanja,
    datumVracanja: raw.datumVracanja ?? null,
    status: raw.status,
    napomena: raw.napomena ?? '',
    knjige: raw.knjige ? raw.knjige.map((k: any) => ({
      knjigaId: k.knjigaId ?? 0,
      naslov: k.naslov ?? '',
      slika: k.slika ?? '',
      godinaIzdanja: k.godinaIzdanja ?? 0,
      isbn: k.isbn ?? '',
      zanr: k.zanr ?? '',
      brojStrana: k.brojStrana ?? 0,
      opis: k.opis ?? ''
    })) : [], 
    clanJmbg: raw.clanJmbg ?? '',
    clanIme: raw.clanIme ?? '',
    clanPrezime: raw.clanPrezime ?? '',
    bibliotekarImePrezime: raw.bibliotekarImePrezime ?? ''
  };
}

export const libraryService = {
  loginBibliotekar: async (korisnickoIme: string, lozinka: string): Promise<BibliotekarDTO> => {
    const res = await http.post('/api/auth/login', { korisnickoIme, lozinka });
    const d = res.data;
    if (d.uloga !== 'Bibliotekar') throw new Error('Korisnik nije bibliotekar.');
    authService.saveToken(d.token);
    return { jmbg: d.jmbg, ime: d.ime, prezime: d.prezime, korisnickoIme: d.korisnickoIme };
  },

  loginClan: async (korisnickoIme: string, lozinka: string): Promise<ClanDTO> => {
    const res = await http.post('/api/auth/login', { korisnickoIme, lozinka });
    const d = res.data;
    if (d.uloga !== 'Clan') throw new Error('Korisnik nije clan.');
    authService.saveToken(d.token);
    return {
      jmbg: d.jmbg, ime: d.ime, prezime: d.prezime,
      brojTelefona: d.brojTelefona ?? '', korisnickoIme: d.korisnickoIme,
      grad: { gradId: 0, naziv: d.gradNaziv ?? '' }
    };
  },

  getAllBooks: async (): Promise<KnjigaDTO[]> => {
    const res = await http.get('/api/knjige');
    return res.data as KnjigaDTO[];
  },

  searchBooks: async (naslov: string): Promise<KnjigaDTO[]> => {
    const res = await http.get(`/api/knjige/search/${naslov}`);
    return res.data as KnjigaDTO[];
  },

  addBook: async (podaci: { knjiga: any; autor: any }): Promise<KnjigaDTO> => {
    const delovi = (podaci.autor.ime as string).trim().split(' ');
    const request = {
      naslov: podaci.knjiga.naslov,
      godinaIzdanja: podaci.knjiga.godinaIzdanja,
      isbn: podaci.knjiga.isbn,
      slika: podaci.knjiga.slika,
      brojStrana: podaci.knjiga.brojStrana,
      opis: podaci.knjiga.opis,
      zanr: podaci.knjiga.zanr,
      autorIme: delovi[0] ?? '',
      autorPrezime: delovi.slice(1).join(' '),
      autorBiografija: podaci.autor.biografija ?? ''
    };
    const res = await http.post('/api/knjige', request);
    return res.data as KnjigaDTO;
  },

  updateBook: async (knjiga: KnjigaDTO): Promise<string> => {
    await http.put(`/api/knjige/${knjiga.knjigaId}`, {
      naslov: knjiga.naslov,
      godinaIzdanja: knjiga.godinaIzdanja,
      isbn: knjiga.isbn,
      slika: knjiga.slika,
      brojStrana: knjiga.brojStrana,
      opis: knjiga.opis,
      zanr: knjiga.zanr
    });
    return 'Uspešno ažurirano';
  },

  deleteBook: async (id: number): Promise<void> => {
    try {
        await http.delete(`/api/knjige/${id}`);
    } catch (error: any) {
        if (error.response && error.response.status === 400) {
            throw new Error(error.response.data || "Knjiga se ne može obrisati jer je trenutno izdata ili rezervisana!");
        }
        throw error;
    }
  },

  getAuthorsForBook: async (knjigaId: number): Promise<AutorDTO[]> => {
    const res = await http.get(`/api/knjige/${knjigaId}/autori`);
    return res.data as AutorDTO[];
  },

  getAllMembers: async (): Promise<ClanDTO[]> => {
    const res = await http.get('/api/clanovi');
    return (res.data as any[]).map(mapClan);
  },

  registerMember: async (clan: ClanDTO): Promise<string> => {
    await http.post('/api/clanovi', {
      jmbg: clan.jmbg,
      ime: clan.ime,
      prezime: clan.prezime,
      brojTelefona: clan.brojTelefona,
      korisnickoIme: clan.korisnickoIme,
      lozinka: clan.lozinka,
      gradNaziv: clan.grad.naziv
    });
    return 'Registracija uspešna!';
  },

  updateMember: async (clan: ClanDTO): Promise<string> => {
    await http.put(`/api/clanovi/${clan.jmbg}`, {
      ime: clan.ime,
      prezime: clan.prezime,
      brojTelefona: clan.brojTelefona,
      gradNaziv: clan.grad.naziv
    });
    return 'Uspešno ažurirano';
  },

  deleteMember: async (jmbg: string): Promise<string> => {
    await http.delete(`/api/clanovi/${jmbg}`);
    return 'Uspešno obrisano';
  },

  getAllIzdavanja: async (): Promise<IzdavanjeDTO[]> => {
    const res = await http.get('/api/izdavanja');
    return (res.data as any[]).map(mapIzdavanje);
  },

  reserveBook: async (podaci: any): Promise<string> => {
    const request = {
      clanJmbg: podaci.clan.jmbg,
      nasloviKnjiga: podaci.knjige.map((k: any) => k.naslov), 
      napomena: podaci.napomena ?? ''
    };
    await http.post('/api/izdavanja/rezervisi', request);
    return 'Rezervacija uspešna';
  },

  returnBook: async (izdavanje: IzdavanjeDTO): Promise<string> => {
    await http.put(`/api/izdavanja/${izdavanje.izdavanjeId}/vrati`);
    return 'Knjiga uspešno vraćena';
  },

  approveIzdavanje: async (izdavanjeId: number, bibliotekarJmbg: string): Promise<string> => {
    await http.put(`/api/izdavanja/${izdavanjeId}/odobri`, { bibliotekarJmbg });
    return 'Odobreno';
  },

  getAllAuthors: async (): Promise<AutorDTO[]> => {
    const res = await http.get('/api/autori');
    return res.data as AutorDTO[];
  },

  addAuthor: async (autor: AutorDTO): Promise<AutorDTO> => {
    const res = await http.post('/api/autori', {
      ime: autor.ime,
      prezime: autor.prezime,
      biografija: autor.biografija
    });
    return res.data as AutorDTO;
  },

  getAllCities: async (): Promise<GradDTO[]> => {
    const res = await http.get('/api/gradovi');
    return res.data as GradDTO[];
  },

  addCity: async (gradData: { naziv: string }): Promise<GradDTO> => {
    const res = await http.post('/api/gradovi', gradData);
    return res.data as GradDTO;
  },

  linkAuthorAndBook: async (_knjigaId: number, _autorId: number): Promise<string> => '',
  getBooksByAuthor: async (_autorId: number): Promise<KnjigaDTO[]> => []
};