import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { libraryService } from '../services/library.ts';

const BookAdd: React.FC = () => {
    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        naslov: '',
        isbn: '',
        brojStrana: '',
        godinaIzdanja: '', 
        opis: '',
        slika: '',
        zanr: 'Roman', // Postavljen default da ne bude prazno
        autori: '',     // Spojeno polje za više autora (odvojeni zarezom)
        biografija: ''
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;

        if (name === 'brojStrana' || name === 'godinaIzdanja') {
            const onlyNums = value.replace(/[^0-9]/g, '');
            setFormData(prev => ({ ...prev, [name]: onlyNums }));
            return;
        }

        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault(); // Sprecava podrazumevano osvezavanje stranice pri slanju forme
    try {
        // Pakovanje svih unetih podataka u slozeni DTO objekat koji backend ocekuje
        const podaciZaSlanje = {
            knjiga: {
                knjigaId: 0, // 0 jer baza automatski generise novi ID pri upisu
                naslov: formData.naslov,
                isbn: formData.isbn,
                brojStrana: parseInt(formData.brojStrana as string), // Pretvaranje stringa iz forme u ceo broj
                godinaIzdanja: parseInt(formData.godinaIzdanja as string), // Pretvaranje godine u ceo broj
                opis: formData.opis,
                slika: formData.slika,
                zanr: formData.zanr
            },
            autor: {
                autorId: 0, // Baza automatski dodeljuje ID novom autoru
                ime: formData.autori, // Salje se ceo string sa zarezima koji ce backend sam da razbije na pojedinacne autore
                prezime: '', // Ostavlja se prazno jer se kompletno parsiranje imena radi na serveru
                biografija: formData.biografija
            }
        };

        // Poziv servisne metode za slanje pripremljenog objekta na server
        await libraryService.addBook(podaciZaSlanje);
        alert("Knjiga je uspešno dodata!"); // Prikaz potvrdne poruke korisniku
        navigate('/home-bibliotekar'); // Automatsko preusmeravanje na pocetnu stranicu bibliotekara
    } catch (error: any) {
        // Hvatanje tacne poruke o greski koja je stigla sa backend servera
        const serverPoruka = error.response?.data?.message || error.response?.data;

        // Specificna provera ukoliko u bazi vec postoji knjiga sa istim ISBN brojem
        if (serverPoruka === "Ova knjiga je vec u biblioteci!") {
            alert("Knjiga sa ovim ISBN-om vec postoji u biblioteci, ne mozete da imate 2 iste knjige u katalogu knjiga!");
        } else {
            alert("Greška: " + (typeof serverPoruka === 'string' ? serverPoruka : "Neuspešno dodavanje"));
        }
    }
};

    return (
        <div className="container py-5">
            <div className="row justify-content-center">
                <div className="col-lg-8">
                    <div className="card border-0 shadow-lg overflow-hidden">
                        <div className="bg-dark p-4 text-white d-flex align-items-center">        
                            <div>
                                <h3 className="m-0 fw-bold">Dodavanje nove knjige u biblioteku</h3>
                            </div>
                        </div>

                        <div className="card-body p-4 p-md-5">
                            <form onSubmit={handleSubmit}>
                                <div className="row g-4">
                                    <div className="col-12">
                                        <label className="form-label fw-bold small">NASLOV KNJIGE</label>
                                        <input
                                            type="text"
                                            name="naslov"
                                            value={formData.naslov}
                                            onChange={handleChange}
                                            className="form-control form-control-lg bg-light border-0"
                                            required
                                        />
                                    </div>

                                    <div className="col-12">
                                        <label className="form-label fw-bold small">AUTOR(I) KNJIGE</label>
                                        <input
                                            type="text"
                                            name="autori"
                                            value={formData.autori}
                                            onChange={handleChange}
                                            className="form-control bg-light border-0"
                                            required
                                        />
                                        <small className="text-muted d-block mt-1">Ako knjiga ima više autora, odvojite ih zarezom.</small>
                                    </div>

                                    <div className="col-12">
                                        <label className="form-label fw-bold small">BIOGRAFIJA AUTORA</label>
                                        <textarea
                                            name="biografija"
                                            value={formData.biografija}
                                            onChange={handleChange}
                                            rows={2}
                                            className="form-control bg-light border-0"
                                        />
                                    </div>

                                    <div className="col-md-6">
                                        <label className="form-label fw-bold small">ISBN</label>
                                        <input
                                            type="text"
                                            name="isbn"
                                            value={formData.isbn}
                                            onChange={handleChange}
                                            className="form-control bg-light border-0"
                                            required
                                        />
                                    </div>

                                    <div className="col-md-6">
                                        <label className="form-label fw-bold small">ŽANR</label>
                                        <select
                                            name="zanr"
                                            value={formData.zanr}
                                            onChange={handleChange}
                                            className="form-select bg-light border-0"
                                        >
                                            <option value="Roman">Roman</option>
                                            <option value="Drama">Drama</option>
                                            <option value="Pripovetka">Pripovetka</option>
                                            <option value="Naucna fantastika">Naucna fantastika</option>
                                            <option value="Biografija">Biografija</option>
                                            <option value="Autobiografija">Autobiografija</option>
                                        </select>
                                    </div>

                                    <div className="col-md-6">
                                        <label className="form-label fw-bold small">BROJ STRANA</label>
                                        <input
                                            type="text" 
                                            name="brojStrana"
                                            value={formData.brojStrana}
                                            onChange={handleChange}
                                            className="form-control bg-light border-0"
                                            placeholder="Unesite broj strana"
                                            required
                                        />
                                    </div>

                                    <div className="col-md-6">
                                        <label className="form-label fw-bold small">GODINA IZDANJA</label>
                                        <input
                                            type="text"
                                            name="godinaIzdanja"
                                            value={formData.godinaIzdanja}
                                            onChange={handleChange}
                                            className="form-control bg-light border-0"
                                            required
                                        />
                                    </div>

                                    <div className="col-12">
                                        <label className="form-label fw-bold small">URL SLIKE</label>
                                        <input
                                            type="text"
                                            name="slika"
                                            value={formData.slika}
                                            onChange={handleChange}
                                            className="form-control bg-light border-0"
                                        />
                                        <small className="text-muted mt-1 d-block">  Pronađite sliku na internetu, levi klik na sliku, 
                            kad vam se sa na desnoj strani ekrana pojavi knjiga onda desni klik na knjigu i izaberete opciju "open image in new tab", 
                            zatim link iz novog taba iskopirajte ovde.</small>

                                    </div>

                                    <div className="col-12">
                                        <label className="form-label fw-bold small">OPIS</label>
                                        <textarea
                                            name="opis"
                                            value={formData.opis}
                                            onChange={handleChange}
                                            rows={4}
                                            className="form-control bg-light border-0"
                                            placeholder="Kratak sadržaj knjige..."
                                        />
                                    </div>
                                </div>

                                <div className="d-flex gap-3 mt-5">
                                    <button
                                        type="button"
                                        onClick={() => navigate('/home-bibliotekar')} 
                                        className="btn btn-outline-secondary px-4 fw-bold"
                                    >
                                        Poništi
                                    </button>
                                    <button
                                        type="submit"
                                        className="btn btn-dark px-5 fw-bold flex-grow-1 shadow-sm"
                                    >
                                        Dodaj knjigu
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BookAdd;