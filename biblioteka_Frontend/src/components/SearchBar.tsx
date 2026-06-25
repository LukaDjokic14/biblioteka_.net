import React, { useState, useEffect } from 'react';

interface SearchBarProps {
    onSearch: (searchTerm: string) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({ onSearch }) => {
    const [term, setTerm] = useState('');

    // Ovaj useEffect prati promenu lokalnog stanja 'term' (ono što korisnik kuca)
    useEffect(() => {
        // Postavljamo tajmer koji će okinuti pretragu i proslediti tekst roditelju nakon 200ms
        const delayDebounceFn = setTimeout(() => {
            onSearch(term);
        }, 200);

        // Cleanup funkcija: Ako korisnik ukuca sledeće slovo pre nego što istekne 200ms,
        // prethodni tajmer se briše i kreće novi
        return () => clearTimeout(delayDebounceFn);
    }, [term, onSearch]);

    const handleClear = () => {
        // Resetovanjem stanja automatski se okida useEffect sa praznim stringom
        setTerm(''); 
    };

    return (
        <div className="w-100" style={{ maxWidth: '400px' }}>
            <div className="input-group shadow-sm">
                <input
                    type="text"
                    className="form-control border-start-0 ps-0"
                    placeholder="Pretražite knjige..."
                    value={term}
                    // Svaka promena unosa ažurira lokalno stanje 'term' i pokreće Debounce tajmer
                    onChange={(e) => setTerm(e.target.value)}
                />
                {term && (
                    // Dugme za brisanje unosa se prikazuje samo kada u polju postoji tekst
                    <button type="button" className="btn btn-light border" onClick={handleClear}>
                        ✕
                    </button>
                )}
            </div>
        </div>
    );
};

export default SearchBar;