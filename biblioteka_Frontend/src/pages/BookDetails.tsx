import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { libraryService, type KnjigaDTO, type AutorDTO, type ClanDTO } from '../services/library';
import { authService } from '../services/auth';

const BookDetails = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { id } = useParams();
    
    const ulogaIzStoragea = sessionStorage.getItem('role');
    const isBibliotekar = ulogaIzStoragea === 'bibliotekar';
    const currentMember = authService.getCurrentUser();

    const [knjiga, setKnjiga] = useState<KnjigaDTO | null>(location.state?.knjiga || null);
    const [autori, setAutori] = useState<AutorDTO[]>([]);
    const [isLoadingAutori, setIsLoadingAutori] = useState(true);
    const [isEditing, setIsEditing] = useState(false);
    
    const [isAvailable, setIsAvailable] = useState(true);
    const [isRequestedByMe, setIsRequestedByMe] = useState(false);

    useEffect(() => {
        if (!knjiga && id) { 
            libraryService.getAllBooks().then(res => { 
                const target = res.find(b => b.knjigaId === Number(id)); 
                if (target) setKnjiga(target); 
            });
        }

        if (knjiga?.knjigaId) { 
            libraryService.getAuthorsForBook(knjiga.knjigaId) 
                .then(res => {
                    setAutori(res);
                    setIsLoadingAutori(false);
                });

            libraryService.getAllIzdavanja().then(svaIzdavanja => { 
                const aktivno = svaIzdavanja.find(i => 
                    i.knjige.some(k => k.knjigaId === knjiga.knjigaId) && 
                    (i.status === 'IZDATO' || i.status === 'REZERVISANO')
                );
                
                if (aktivno) { 
                    setIsAvailable(false);
                    if (aktivno.status === 'REZERVISANO' && currentMember && 'jmbg' in currentMember && aktivno.clanJmbg === currentMember.jmbg) {
                        setIsRequestedByMe(true);
                    }
                } else { 
                    setIsAvailable(true);
                    setIsRequestedByMe(false);
                }
            });
        }
    }, [knjiga?.knjigaId, id]);

    const handleBorrow = async () => {
        if (!currentMember || !('jmbg' in currentMember) || !knjiga) return;

        try {
            const rezervacijaDTO = {
                knjige: [knjiga], // Šaljemo niz jer bekend očekuje listu
                clan: currentMember as ClanDTO,
                status: 'REZERVISANO',
                datumIzdavanja: new Date().toISOString().split('T')[0]
            };

            await libraryService.reserveBook(rezervacijaDTO);
            
            alert("Zahtev za pozajmicu je uspešno poslat!");
            setIsRequestedByMe(true);
            setIsAvailable(false);
        } catch (e) { 
            console.error("Greška na bekendu:", e);
            alert("Greška pri slanju zahteva. Proverite da li su sva polja ispravna.");
        }
    };

    const handleUpdate = async () => {
        if (!knjiga) return;
        try {
            await libraryService.updateBook(knjiga);
            alert("Uspešno ažurirano!");
            setIsEditing(false);
        } catch (e) { 
            alert("Greška pri čuvanju."); 
        }
    };

    if (!knjiga) return <div className="text-center mt-5">Učitavanje podataka...</div>;

    return (
        <div className="container py-5">
            <button className="btn btn-outline-secondary mb-4 shadow-sm" onClick={() => navigate(-1)}>← Nazad</button>
            
            <div className="card border-0 shadow-lg p-4">
                <div className="row">
                    <div className="col-md-4">
                        <img src={knjiga.slika} className="img-fluid rounded shadow" alt={knjiga.naslov} />
                    </div>
                    <div className="col-md-8">
                        {isEditing ? (
                            <input
                                className="form-control form-control-lg mb-3"
                                value={knjiga.naslov}
                                onChange={e => setKnjiga({ ...knjiga, naslov: e.target.value })}
                            />
                        ) : (
                            <h1 className="display-5 fw-bold">{knjiga.naslov}</h1>
                        )}

                        <p className="mb-0 text-primary fw-bold">
                            Autori: {isLoadingAutori ? "..." : (autori.map(a => `${a.ime} ${a.prezime}`).join(', ') || "Nije navedeno")}
                        </p>

                        <hr />

                        <h6 className="fw-bold text-muted text-uppercase small">Opis:</h6>
                        {isEditing ? (
                            <textarea
                                className="form-control mb-3"
                                rows={5}
                                value={knjiga.opis}
                                onChange={e => setKnjiga({ ...knjiga, opis: e.target.value })}
                            />
                        ) : (
                            <p className="lead">{knjiga.opis || "Nema dostupnog opisa za ovu knjigu."}</p>
                        )}

                        <div className="mt-4">
                            {isBibliotekar ? (
                                isEditing ? (
                                    <button className="btn btn-success px-4 me-2" onClick={handleUpdate}>Sačuvaj</button>
                                ) : (
                                    <button className="btn btn-warning px-4" onClick={() => setIsEditing(true)}>Izmeni detalje</button>
                                )
                            ) : (
                                isAvailable ? (
                                    <button className="btn btn-primary btn-lg px-5" onClick={handleBorrow}>Pozajmi knjigu</button>
                                ) : (
                                    <button className="btn btn-secondary btn-lg px-5 shadow-none" disabled>
                                        {isRequestedByMe ? "Zahtev na čekanju" : "Trenutno na izdavanju"}
                                    </button>
                                )
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BookDetails;