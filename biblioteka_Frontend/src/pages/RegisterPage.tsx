import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { libraryService, type GradDTO, type ClanDTO } from '../services/library';

const RegisterPage = () => {
    const navigate = useNavigate();
    const [gradovi, setGradovi] = useState<GradDTO[]>([]); // Lokalni niz koji čuva sve gradove povučene iz baze za prikaz у select listi
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    // --- NOVA STANJA ZA GRAD ---
    const [isCustomCity, setIsCustomCity] = useState(false); // Fleg koji prebacuje mod interfejsa sa padajuće liste (false) na ručni unos (true)
    const [noviGrad, setNoviGrad] = useState(''); // Pamti tekstualni naziv novog grada ukoliko korisnik odluči da ga ručno upiše

    // Stanja za formu
    const [formData, setFormData] = useState({
        jmbg: '',
        ime: '',
        prezime: '',
        brojTelefona: '',
        korisnickoIme: '',
        lozinka: '',
        gradId: 0
    });

    useEffect(() => {
    // Pokretanje servisne metode odmah nakon što se komponenta montira na ekran
    libraryService.getAllCities()
        // U slučaju uspešnog odgovora, добијени низ градова се уписује у локално стање 'gradovi'
        .then(res => setGradovi(res))
        // u slučaju greške, ona se ispisuje
        .catch(() => setError("Greška pri učitavanju gradova."));
    }, []); // Prazan niz zavisnosti osigurava da se efekat okine samo jednom

    const handleRegister = async (e: React.FormEvent) => {
    // Sprečava podrazumevano ponašanje forme (osvežavanje stranice prilikom submita)
    e.preventDefault();
    // Poništava prethodne poruke o greškama
    setError(null);

    // --- VALIDACIJE ---
    // Proverava da li JMBG ima tačno 13 cifara pomoću regularnog izraza (Regex)
    if (formData.jmbg.length !== 13 || !/^\d+$/.test(formData.jmbg)) {
        setError("JMBG mora imati tačno 13 cifara!");
        return; // Prekida dalje izvršavanje funkcije ukoliko validacija ne prođe
    }
    
    // Validacija izbora postojećeg grada: ako nismo u "custom" modu, ID selektovanog grada ne sme ostati na nuli
    if (!isCustomCity && formData.gradId === 0) {
        setError("Morate izabrati grad!");
        return;
    }
    // Validacija ručnog unosa: ako je aktiviran "custom" mod, tekstualno polje za naziv grada ne sme biti prazno
    if (isCustomCity && !noviGrad.trim()) {
        setError("Morate uneti naziv novog grada!");
        return;
    }

    try {
        let izabraniGrad: GradDTO;

        // --- LOGIKA ZA UPIS NOVOG GRADA ---
        if (isCustomCity) {
            // Ako korisnik kuca grad koji ne postoji, kreiramo POST zahtev servisu da ga upiše i čekamo kompletan kreirani objekat sa novim ID-jem
            izabraniGrad = await libraryService.addCity({ naziv: noviGrad.trim() });
        } else {
            // Ako bira postojeći grad, pronalazimo ga u već učitanom lokalnom nizu preko ID-ja koji je odabran u select elementu
            izabraniGrad = gradovi.find(g => g.gradId === formData.gradId)!;
        }
        
        // Formiranje konačnog objekta novog člana koji odgovara strukturi koju očekuje backend
        const noviClan: ClanDTO = {
            jmbg: formData.jmbg,
            ime: formData.ime,
            prezime: formData.prezime,
            brojTelefona: formData.brojTelefona,
            korisnickoIme: formData.korisnickoIme,
            lozinka: formData.lozinka,
            grad: izabraniGrad // Prosleđivanje kompletnog DTO objekta grada 
        };

        // Slanje podataka servisu za registraciju i čekanje tekstualnog odgovora sa servera
        const responseMessage = await libraryService.registerMember(noviClan);
        // Postavljanje tekstualne poruke o uspehu u državu (state) radi prikaza korisniku
        setSuccess(responseMessage);
        
        // Nakon 2 sekunde (2000 milisekundi) uspešне регистрације, SPA navigacija vraća korisnika na login ekran
        setTimeout(() => navigate('/login'), 2000);

    } catch (err: any) {
        // Ukoliko dođe do greške (npr. zauzeto korisničko ime ili JMBG), izvlači se poruka sa backenda
        const porukaSaBackenda = err.response?.data?.message || "Registracija nije uspela.";
        setError(porukaSaBackenda);
    }
};

    return (
        <div className="container py-5 d-flex justify-content-center">
            <div className="card shadow-lg p-4" style={{ maxWidth: '500px', width: '100%', borderRadius: '15px' }}>
                <h2 className="text-center fw-bold mb-4">Registracija Člana </h2>
                
                {error && <div className="alert alert-danger p-2 small">{error}</div>}
                {success && <div className="alert alert-success p-2 small">{success}</div>}

                <form onSubmit={handleRegister}>
                    <div className="row">
                        <div className="col-md-6 mb-3">
                            <label className="form-label small fw-bold">Ime</label>
                            <input className="form-control" type="text" required onChange={e => setFormData({...formData, ime: e.target.value})} />
                        </div>
                        <div className="col-md-6 mb-3">
                            <label className="form-label small fw-bold">Prezime</label>
                            <input className="form-control" type="text" required onChange={e => setFormData({...formData, prezime: e.target.value})} />
                        </div>
                    </div>

                    <div className="mb-3">
                        <label className="form-label small fw-bold">JMBG (13 cifara)</label>
                        <input className="form-control" type="text" maxLength={13} required onChange={e => setFormData({...formData, jmbg: e.target.value})} />
                    </div>

                    {/*KOMBINOVANO POLJE ZA GRAD*/}
                    <div className="mb-3">
                        <div className="d-flex justify-content-between align-items-center mb-2">
                            <label className="form-label small fw-bold m-0">Grad</label>
                            {/* Dugme koje služi kao prekida između modova: menja vrednost isCustomCity pri kliku */}
                            <button
                                type="button"
                                onClick={() => setIsCustomCity(!isCustomCity)}
                                className="btn btn-link p-0 text-decoration-none small fw-semibold"
                                style={{ fontSize: '13px' }}
                            >
                                {isCustomCity ? "Izaberite iz liste..." : "Vaš grad nije na listi?"}
                            </button>
                        </div>

                        {/* Uslovno renderovanje: Ako je isCustomCity false, prikazuje se padajuća lista sa postojećim gradovima  */}
                        {!isCustomCity ? (
                            <select className="form-select" required={!isCustomCity} onChange={e => setFormData({...formData, gradId: Number(e.target.value)})}>
                                <option value="0">Izaberite grad...</option>
                                {gradovi.map(g => (
                                    <option key={g.gradId} value={g.gradId}>{g.naziv}</option>
                                ))}
                            </select>
                        ) : (
                            
                            <input 
                                className="form-control" 
                                type="text" 
                                placeholder="Unesite naziv grada"
                                required={isCustomCity}
                                value={noviGrad}
                                onChange={e => setNoviGrad(e.target.value)}
                            />
                        )}
                    </div>

                    <div className="mb-3">
                        <label className="form-label small fw-bold">Broj telefona</label>
                        <input className="form-control" type="text" onChange={e => setFormData({...formData, brojTelefona: e.target.value})} />
                    </div>

                    <div className="mb-3">
                        <label className="form-label small fw-bold">Korisničko ime</label>
                        <input className="form-control" type="text" required onChange={e => setFormData({...formData, korisnickoIme: e.target.value})} />
                    </div>

                    <div className="mb-4">
                        <label className="form-label small fw-bold">Lozinka</label>
                        <input className="form-control" type="password" required onChange={e => setFormData({...formData, lozinka: e.target.value})} />
                    </div>

                    <button className="btn btn-primary w-100 fw-bold py-2" type="submit">
                        Potvrdi registraciju
                    </button>
                    
                    <button className="btn btn-link w-100 mt-2 text-muted small" type="button" onClick={() => navigate('/login')}>
                        Povratak na prijavu
                    </button>
                </form>
            </div>
        </div>
    );
};

export default RegisterPage;