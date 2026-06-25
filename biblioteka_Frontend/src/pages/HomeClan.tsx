import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { libraryService, type KnjigaDTO, type IzdavanjeDTO } from '../services/library';
import { authService } from '../services/auth';
import SearchBar from '../components/SearchBar';

interface ClanDTO {
    jmbg: string;
    ime: string;
    prezime?: string;
    korisnickoIme?: string;
}

const HomeClan = () => {
    const [knjige, setKnjige] = useState<KnjigaDTO[]>([]);
    const [izdavanja, setIzdavanja] = useState<IzdavanjeDTO[]>([]);
    const [aktivniTab, setAktivniTab] = useState<'sve' | 'zauzete'>('sve');
    
    const user = authService.getCurrentUser() as ClanDTO | null;
    const navigate = useNavigate();

    useEffect(() => {
        ucitajPodatke();
    }, []);

    const ucitajPodatke = async () => {
        try {
            const [sveKnjige, svaIzdavanja] = await Promise.all([
                libraryService.getAllBooks(),
                libraryService.getAllIzdavanja()
            ]);
            setKnjige(sveKnjige);
            setIzdavanja(svaIzdavanja);
        } catch (err) {
            console.error("Greška pri učitavanju:", err);
        }
    };

    const osveziListuKnjiga = async (term: string) => {
        try {
            const result = term.trim() === '' 
                ? await libraryService.getAllBooks() 
                : await libraryService.searchBooks(term);
            setKnjige(result);
        } catch (err) {
            console.error("Greška pri pretrazi:", err);
        }
    };

    const handleVratiKnjigu = async (knjigaId: number) => {
        const izdavanje = izdavanja.find(i =>
            i.knjige.some(k => k.knjigaId === knjigaId) &&
            i.clanJmbg === user?.jmbg &&
            i.status === 'IZDATO'
        );

        if (izdavanje && window.confirm("Da li ste sigurni da želite da vratite knjigu?")) {
            try {
                await libraryService.returnBook(izdavanje);
                alert("Knjiga je uspešno vraćena!");
                await ucitajPodatke();
            } catch (err) {
                alert("Greška pri vraćanju knjige.");
            }
        }
    };

    const zauzeteKnjigaIds = izdavanja
        .filter(i => i.status === 'IZDATO' || i.status === 'REZERVISANO')
        .flatMap(i => i.knjige.map(k => k.knjigaId));

    const dostupneKnjige = knjige.filter(k => !zauzeteKnjigaIds.includes(k.knjigaId));
    const samoZauzeteKnjige = knjige.filter(k => zauzeteKnjigaIds.includes(k.knjigaId));
    const knjigeZaPrikaz = aktivniTab === 'sve' ? dostupneKnjige : samoZauzeteKnjige;

    const jeKodMene = (knjigaId: number) => {
        return izdavanja.some(i => 
            i.knjige.some(k => k.knjigaId === knjigaId) && 
            i.clanJmbg === user?.jmbg && 
            i.status === 'IZDATO'
        );
    };

    return (
        <div className="container mt-4 pb-5">
            <nav className="navbar navbar-light bg-white border-bottom mb-5 px-4 rounded-pill shadow-sm">
                <span className="navbar-brand mb-0 h1 text-primary fw-bold">E-Biblioteka</span>
                <div className="d-flex align-items-center">
                    <span className="me-3 fw-semibold text-secondary">Zdravo, <span className="text-dark">{user?.ime}</span></span>
                    <button className="btn btn-sm btn-outline-danger rounded-pill px-3" onClick={() => authService.logout()}>Odjavi se</button>
                </div>
            </nav>

            <div className="d-flex justify-content-center mb-4">
                <div className="bg-light p-1 rounded-pill shadow-sm d-inline-flex">
                    <button className={`btn rounded-pill px-4 py-2 fw-bold transition-all ${aktivniTab === 'sve' ? 'btn-primary shadow' : 'btn-light text-secondary'}`} onClick={() => setAktivniTab('sve')}>Dostupne knjige</button>
                    <button className={`btn rounded-pill px-4 py-2 fw-bold transition-all ${aktivniTab === 'zauzete' ? 'btn-primary shadow' : 'btn-light text-secondary'}`} onClick={() => setAktivniTab('zauzete')}>Zauzete knjige ({samoZauzeteKnjige.length})</button>
                </div>
            </div>

            <div className="d-flex justify-content-center mb-4">
                <SearchBar onSearch={osveziListuKnjiga} />
            </div>

            <div className="text-center mb-5">
                <h2 className="fw-bold text-dark mb-2">{aktivniTab === 'sve' ? 'Dostupne knjige za čitanje' : 'Knjige trenutno kod članova'}</h2>
            </div>
            
            <div className="row g-4">
                {knjigeZaPrikaz.map(k => {
                    const jeMoja = jeKodMene(k.knjigaId);
                    const aktivnoIzdavanje = izdavanja.find(i => 
                        i.knjige.some(item => item.knjigaId === k.knjigaId) && 
                        (i.status === 'IZDATO' || i.status === 'REZERVISANO')
                    );

                    return (
                        <div key={k.knjigaId} className="col-lg-3 col-md-4 col-sm-6">
                            <div className="card h-100 border-0 shadow-sm hover-card" style={{ borderRadius: '20px' }}>
                                <div className="position-relative">
                                    <img src={k.slika} className="card-img-top" alt={k.naslov} style={{ height: '320px', objectFit: 'cover', borderRadius: '20px 20px 0 0' }} />
                                    {jeMoja && <span className="position-absolute top-0 end-0 m-3 badge bg-success rounded-pill px-3 py-2 shadow-sm">Kod Vas</span>}
                                    
                                    {aktivniTab === 'zauzete' && !jeMoja && (
                                        aktivnoIzdavanje?.status === 'REZERVISANO' ? (
                                            <span className="position-absolute top-0 end-0 m-3 badge bg-warning text-dark rounded-pill px-3 py-2 shadow-sm">Rezervisano</span>
                                        ) : (
                                            <span className="position-absolute top-0 end-0 m-3 badge bg-danger rounded-pill px-3 py-2 shadow-sm">Zauzeta</span>
                                        )
                                    )}
                                </div>
                                <div className="card-body text-center p-3 d-flex flex-column justify-content-between">
                                    <div>
                                        <h6 className="card-title fw-bold text-truncate">{k.naslov}</h6>
                                        {jeMoja && aktivnoIzdavanje?.napomena && (
                                            <div className="mt-2 p-2 bg-warning bg-opacity-10 rounded text-start small border border-warning border-opacity-25" style={{ fontSize: '0.8rem' }}>
                                                <strong>Napomena:</strong> {aktivnoIzdavanje.napomena}
                                            </div>
                                        )}
                                    </div>
                                    
                                    <div className="mt-3">
                                        <button className="btn btn-primary btn-sm rounded-pill w-100 py-2 fw-bold" onClick={() => navigate(`/book/${k.knjigaId}`, { state: { knjiga: k } })}>Vidi detalje</button>
                                        {jeMoja && (
                                            <button className="btn btn-success btn-sm rounded-pill w-100 mt-2 py-2 fw-bold" onClick={() => handleVratiKnjigu(k.knjigaId)}>Vrati knjigu</button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

export default HomeClan;