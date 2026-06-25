import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { libraryService } from '../services/library';
import { authService } from '../services/auth';

const LoginPage = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [role, setRole] = useState<'bibliotekar' | 'clan'>('bibliotekar');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
    // Sprečava podrazumevano osvežavanje cele stranice prilikom slanja forme
    e.preventDefault();
    // Aktivira stanje učitavanja
    setIsLoading(true);
    // Resetuje prethodne poruke o greškama i uspehu pre novog pokušaja
    setError(null);
    setSuccess(null);

    try {
        // Provera da li se korisnik prijavljuje kao bibliotekar ili član
        if (role === 'bibliotekar') {
            // Šalje asinhroni zahtev bekendu za proveru kredencijala bibliotekara
            const user = await libraryService.loginBibliotekar(username, password);
            if (user) {
                // Čuva ulogu u ugrađenom sessionStorage-u pretraživača
                sessionStorage.setItem('role', 'bibliotekar');
                // Čuva kompletne podatke o korisniku koristeći prilagođeni authService
                authService.saveSession(user);
                // Postavlja poruku o uspešnoj prijavi za korisnički interfejs
                setSuccess("Uspešna prijava!");
                // Preusmerava bibliotekara na njegovu stranicu nakon 1 sekunde
                setTimeout(() => {
                    navigate('/home-bibliotekar');
                }, 1000);
            }
        } else {
            // Šalje asinhroni zahtev bekendu za proveru kredencijala člana
            const clan = await libraryService.loginClan(username, password);
            if (clan) {
                // Čuva ulogu člana u sessionStorage-u
                sessionStorage.setItem('role', 'clan');
                // Čuva podatke o prijavljenom članu 
                authService.saveSession(clan);
                // Prikazuje poruku o uspehu
                setSuccess("Uspešna prijava!");
                // Preusmerava člana na njegovu stranicu nakon 1 sekunde
                setTimeout(() => {
                    navigate('/home-clan');
                }, 1000);
            }
        }
    } catch (err) {
        // Ukoliko bekend vrati grešku (npr. pogrešna lozinka), hvata se ovde
        setSuccess(null);
        setError("Neuspešna prijava. Proverite kredencijale.");
    } finally {
        // Izvršava se na kraju bez obzira na ishod (uspeh ili greška)
        setIsLoading(false);
    }
};

    return (
        <div className="min-vh-100 d-flex align-items-center justify-content-center" 
             style={{ 
                 background: 'linear-gradient(135deg, #1b7ec0 0%, #1b7ec0 0%)',
                 padding: '20px' 
             }}>
            
            <div className="card shadow-lg p-5 border-0" 
                 style={{ width: '480px', borderRadius: '25px', backgroundColor: '#ffffff' }}>
                
                <div className="text-center mb-4">
                    <h1 className="fw-bold text-primary mb-1">E-Biblioteka </h1>
                    <p className="text-muted small">Sistem za upravljanje radom biblioteke</p>
                </div>
                
                <div className="btn-group w-100 mb-4 shadow-sm" style={{ borderRadius: '12px'}}>
                    <button 
                        className={`btn py-2 fw-bold ${role === 'bibliotekar' ? 'btn-primary' : 'btn-outline-primary'}`}
                        onClick={() => setRole('bibliotekar')}
                    >
                        Bibliotekar
                    </button>
                    <button 
                        className={`btn py-2 fw-bold ${role === 'clan' ? 'btn-primary' : 'btn-outline-primary'}`}
                        onClick={() => setRole('clan')}
                    >
                        Član
                    </button>
                </div>

                {error && <div className="alert alert-danger p-2 text-center small fw-bold">{error}</div>}
                {success && <div className="alert alert-success p-2 text-center small fw-bold">{success}</div>}

                <form onSubmit={handleLogin}>
                    <div className="mb-3 text-start">
                        <label className="form-label small fw-bold text-secondary">Korisničko ime</label>
                        <input 
                            className="form-control form-control-lg border-2 shadow-sm" 
                            style={{ borderRadius: '12px' }}
                            type="text" 
                            value={username} 
                            onChange={(e) => setUsername(e.target.value)} 
                            placeholder="Unesite korisničko ime"
                            required 
                        />
                    </div>
                    <div className="mb-4 text-start">
                        <label className="form-label small fw-bold text-secondary">Lozinka</label>
                        <input 
                            className="form-control form-control-lg border-2 shadow-sm" 
                            style={{ borderRadius: '12px' }}
                            type="password" 
                            value={password} 
                            onChange={(e) => setPassword(e.target.value)} 
                            placeholder="••••••••"
                            required 
                        />
                    </div>
                    
                    <button className="btn btn-dark w-100 py-3 fw-bold rounded-pill shadow hover-scale" 
                            disabled={isLoading}
                            style={{ transition: '0.3s' }}>
                        {isLoading ? (
                            <span className="spinner-border spinner-border-sm me-2"></span>
                        ) : `Prijavi se kao ${role === 'bibliotekar' ? 'Bibliotekar' : 'Član'}`}
                    </button>
                </form>

                {role === 'clan' && (
                    <div className="text-center mt-4 pt-3 border-top">
                        <p className="small mb-0 text-muted">Nemaš nalog?</p>
                        <button 
                            className="btn btn-link text-decoration-none fw-bold p-0"
                            onClick={() => navigate('/register')}
                        >
                            Registruj se kao novi član
                        </button>
                    </div>
                )}
            </div>

            <style>{`
                .hover-scale:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(0,0,0,0.3); }
                .form-control:focus { border-color: #0d6efd; box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.1); }
            `}</style>
        </div>
    );
};

export default LoginPage;