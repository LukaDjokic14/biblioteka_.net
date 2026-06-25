import { useEffect, useState } from 'react';
import { libraryService, type KnjigaDTO, type ClanDTO, type IzdavanjeDTO, type BibliotekarDTO } from '../services/library';
import { authService } from '../services/auth';
import { useNavigate } from 'react-router-dom';
import SearchBar from '../components/SearchBar';

const HomeBibliotekar = () => {
    const [books, setBooks] = useState<KnjigaDTO[]>([]);
    const [members, setMembers] = useState<ClanDTO[]>([]);
    const [izdavanja, setIzdavanja] = useState<IzdavanjeDTO[]>([]);
    
    const [showModal, setShowModal] = useState(false);
    const [selectedMember, setSelectedMember] = useState<ClanDTO | null>(null);
    const [view, setView] = useState<'knjige' | 'clanovi'>('knjige');
    
    const navigate = useNavigate();
    const user = authService.getCurrentUser();

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const [booksData, membersData, izdavanjaData] = await Promise.all([
                libraryService.getAllBooks(),
                libraryService.getAllMembers(),
                libraryService.getAllIzdavanja()
            ]);

            console.log("Stigla izdavanja sa servera:", izdavanjaData);
            
            const getBookPriority = (knjigaId: number) => {
                const aktivno = izdavanjaData.find(i => 
                    i.knjige?.some(k => k.knjigaId === knjigaId) && 
                    (i.status === 'REZERVISANO' || i.status === 'IZDATO')
                );
                
                if (aktivno?.status === 'REZERVISANO') return 1; 
                if (!aktivno) return 2;                            
                return 3;                                          
            };

            const sortedBooks = [...booksData].sort((a, b) => {
                return getBookPriority(a.knjigaId) - getBookPriority(b.knjigaId);
            });

            setBooks(sortedBooks);
            setMembers(membersData);
            setIzdavanja(izdavanjaData);
        } catch (error) {
            console.error("Greska: ", error);
        }
    };


    const handleBookSearch = async (naslov: string) => {
        if (!naslov.trim()) {
            await loadData(); 
            return;
        }
        try {
            const searchResults = await libraryService.searchBooks(naslov);
            setBooks(searchResults);
        } catch (error) {
            console.error("Greška pri pretrazi knjiga: ", error);
        }
    };

    const handleApprove = async (izdavanje: IzdavanjeDTO) => {
        const bibliotekar = authService.getCurrentUser() as BibliotekarDTO;
        if (window.confirm("Da li ste sigurni da želite da odobrite rezervaciju?")) {
            try {
                await libraryService.approveIzdavanje(izdavanje.izdavanjeId, bibliotekar.jmbg);
                alert("Pozajmica uspešno odobrena!");
                await loadData();
            } catch (e) {
                alert("Greška pri odobravanju.");
            }
        }
    };

    const handleDeleteBook = async (id: number) => {
        const jeIzdata = izdavanja.some(i => i.knjige?.some(k => k.knjigaId === id) && (i.status === 'IZDATO' || i.status === 'REZERVISANO'));
        if (jeIzdata) {
            alert("Nije moguće obrisati knjigu koja je trenutno zadužena/rezervisana!");
            return;
        }

        if (window.confirm("Da li ste sigurni da želite da obrišete ovu knjigu?")) {
            try {
                await libraryService.deleteBook(id);
                alert("Knjiga je uspesno obrisana!");
                await loadData();
            } catch (error) {
                alert("Greška pri brisanju knjige");
            }
        }
    };

    const handleDeleteMember = async (jmbg: string) => {
        const imaZaduzenja = izdavanja.some(i => i.clanJmbg === jmbg && (i.status === 'IZDATO'));
        if (imaZaduzenja) {
            alert("Nije moguće obrisati člana koji ima aktivna zaduženja!");
            return;
        }

        if (window.confirm("Da li ste sigurni da želite da obrišete ovog člana?")) {
            try {
                await libraryService.deleteMember(jmbg);
                alert("Uspešno brisanje člana!");
                await loadData();
            } catch (error) {
                alert("Greška pri brisanju člana.");
            }
        }
    };

    const proveriStatusKnjige = (knjigaId: number) => {
        const aktivnoIzdavanje = izdavanja.find(i => i.knjige?.some(k => k.knjigaId === knjigaId) && (i.status === 'IZDATO' || i.status === 'REZERVISANO'));
        
        if (aktivnoIzdavanje) {
            if (aktivnoIzdavanje.status === 'REZERVISANO') {
                return <span className="badge bg-primary">Rezervisano (čeka odobrenje)</span>;
            }
            // Ovde sada koristimo direktno clanIme
            return <span className="badge bg-danger">Izdata (kod: {aktivnoIzdavanje.clanIme})</span>;
        }
        return <span className="badge bg-success">Dostupna</span>;
    };

    return (
        <div className="container-fluid bg-light min-vh-100 p-0">
            <nav className="navbar navbar-dark bg-dark px-4 py-3 shadow-sm">
                <span className="navbar-brand fw-bold">E-Biblioteka | Panel Bibliotekara</span>
                <div className="d-flex align-items-center">
                    <span className="text-white me-3">Korisnik: <strong>{user?.ime}</strong></span>
                    <button className="btn btn-outline-light btn-sm" onClick={() => { authService.logout(); navigate('/login'); }}>Odjavi se</button>
                </div>
            </nav>

            <div className="container mt-4">
                <div className="row mb-4 align-items-center">
                    <div className="col-md-4 mb-3 mb-md-0">
                        <div className="btn-group shadow-sm">
                            <button className={`btn ${view === 'knjige' ? 'btn-primary' : 'btn-white'}`} onClick={() => setView('knjige')}>
                                Katalog Knjiga
                            </button>
                            <button className={`btn ${view === 'clanovi' ? 'btn-primary' : 'btn-white'}`} onClick={() => setView('clanovi')}>
                                Spisak Članova
                            </button>
                        </div>
                    </div>

                    <div className="col-md-4 mb-3 mb-md-0 d-flex justify-content-center">
                        {view === 'knjige' && (
                            <SearchBar onSearch={handleBookSearch} />
                        )}
                    </div>

                    <div className="col-md-4 text-end">
                        {view === 'knjige' && (
                            <button className="btn btn-success fw-bold shadow-sm" onClick={() => navigate('/add-book')}>
                                + Dodaj novu knjigu
                            </button>
                        )}
                    </div>
                </div>

                {view === 'knjige' ? (
                    <div className="row g-4">
                        {books.map(knjiga => (
                            <div key={knjiga.knjigaId} className="col-md-4 col-lg-3">
                                <div className="card h-100 border-0 shadow-sm overflow-hidden">
                                    <div className="position-relative">
                                        <img src={knjiga.slika} className="card-img-top" alt={knjiga.naslov} style={{ height: '250px', objectFit: 'cover' }} />
                                    </div>
                                    <div className="card-body d-flex flex-column">
                                        <h6 className="fw-bold mb-1 text-truncate" title={knjiga.naslov}>{knjiga.naslov}</h6>
                                        <p className="small text-muted mb-2">ISBN: {knjiga.isbn}</p>
                                        <div className="mb-3">
                                            {proveriStatusKnjige(knjiga.knjigaId)}
                                        </div>
                                        <div className="mt-auto d-flex gap-2">
                                            <button className="btn btn-outline-primary btn-sm flex-grow-1" onClick={() => navigate(`/book/${knjiga.knjigaId}`, { state: { knjiga } })}>
                                                Detalji knjige
                                            </button>
                                            <button className="btn btn-outline-danger btn-sm" onClick={() => handleDeleteBook(knjiga.knjigaId)}>
                                                Obriši
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                ) : (
                    <div className="card border-0 shadow-sm">
                        <div className="table-responsive">
                            <table className="table table-hover align-middle mb-0">
                                <thead className="table-light">
                                    <tr>
                                        <th className="ps-4">Ime i Prezime</th>
                                        <th>JMBG</th>
                                        <th>Grad</th>
                                        <th>Zaduženja</th>
                                        <th ></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {members.map(clan => {
                                        // AŽURIRANO: Sada koristimo clanJmbg iz izdavanja
                                        const zahtev = izdavanja.find(i => i.clanJmbg === clan.jmbg && i.status === 'REZERVISANO');
                                        const brojZaduzenja = izdavanja.filter(i => i.clanJmbg === clan.jmbg && i.status === 'IZDATO').length;
                                        
                                        return (
                                            <tr key={clan.jmbg}>
                                                <td className="ps-4 fw-bold">
                                                    {clan.ime} {clan.prezime}
                                                    {zahtev && <span className="badge bg-primary ms-2 animate-pulse">Čeka odobravanje rezervacije</span>}
                                                </td>
                                                <td><code>{clan.jmbg}</code></td>
                                                <td>{clan.grad?.naziv}</td>
                                                <td>
                                                    <span className={`badge rounded-pill ${brojZaduzenja > 0 ? 'bg-warning text-dark' : 'bg-light text-secondary border'}`}>
                                                        Zaduženo: {brojZaduzenja}
                                                    </span>
                                                </td>
                                                <td className="text-end pe-4">
                                                    {zahtev && (
                                                        <button className="btn btn-success btn-sm me-2 shadow-sm" onClick={() => handleApprove(zahtev)}>
                                                            Odobri: {zahtev.knjige?.map(k => k.naslov).join(', ')}
                                                        </button>
                                                    )}
                                                    <button className="btn btn-light btn-sm me-2 border" onClick={() => { setSelectedMember(clan); setShowModal(true); }}>
                                                        Istorija
                                                    </button>
                                                    <button className="btn btn-outline-danger btn-sm" onClick={() => handleDeleteMember(clan.jmbg)}>
                                                        Obriši
                                                    </button>
                                                </td>
                                            </tr>
                                        );
                                    })}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}
            </div>

            {showModal && selectedMember && (
                <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(4px)' }}>
                    <div className="modal-dialog modal-dialog-centered">
                        <div className="modal-content border-0 shadow-lg">
                            <div className="modal-header bg-dark text-white">
                                <h5 className="modal-title">Istorija: {selectedMember.ime}</h5>
                                <button type="button" className="btn-close btn-close-white" onClick={() => setShowModal(false)}></button>
                            </div>
                            <div className="modal-body p-0">
                                {(() => {
                                    // AŽURIRANO: Koristimo clanJmbg
                                    const aktivnaIzdavanja = izdavanja.filter(i => i.clanJmbg === selectedMember.jmbg && i.status === 'IZDATO');
                                    if (aktivnaIzdavanja.length === 0) {
                                        return <div className="p-5 text-center text-muted">Član trenutno nema zaduženih knjiga.</div>;
                                    }
                                    return (
                                        <ul className="list-group list-group-flush">
                                            {aktivnaIzdavanja.flatMap(izdavanje => 
                                                izdavanje.knjige?.map(knjiga => (
                                                    <li key={`${izdavanje.izdavanjeId}-${knjiga.knjigaId}`} className="list-group-item d-flex align-items-center py-3">
                                                        <img src={knjiga.slika} alt="" className="rounded me-3 shadow-sm" style={{ width: '40px', height: '55px', objectFit: 'cover' }} />
                                                        <div className="flex-grow-1">
                                                            <div className="fw-bold">{knjiga.naslov}</div>
                                                            <small className="text-muted">Preuzeto: {new Date(izdavanje.datumIzdavanja).toLocaleDateString('sr-RS')}</small>
                                                        </div>
                                                    </li>
                                                )) || []
                                            )}
                                        </ul>
                                    );
                                })()}
                            </div>
                            <div className="modal-footer bg-light">
                                <button type="button" className="btn btn-secondary w-100" onClick={() => setShowModal(false)}>Zatvori</button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default HomeBibliotekar;