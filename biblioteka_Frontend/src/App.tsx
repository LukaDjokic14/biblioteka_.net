import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import HomeBibliotekar from './pages/HomeBibliotekar';
import HomeClan from './pages/HomeClan';
import { authService } from './services/auth';
import RegisterPage from './pages/RegisterPage';
import BookDetails from './pages/BookDetails';
import BookAdd from './pages/BookAdd';

// Funkcionalna komponenta PrivateRoute koja prihvata 'children' (komponente koje štiti)
const PrivateRoute = ({ children }: { children: React.ReactNode }) => {
    // Proverava kroz authService da li u sessionStorage-u postoje validni podaci o ulogovanom korisniku
    // Ako je korisnik ulogovan, vraća React Fragment sa prosleđenim sadržajem
    // Ako nije ulogovan, preusmerava nazad na stranicu za login
    return authService.isLoggedIn() ? <>{children}</> : <Navigate to="/login" />;
};

function App() {
    const user = authService.getCurrentUser();

    const getHomeRedirect = () => {
        if (!user) return "/login";
        // Bibliotekar ima polje korisnickoIme, Član u tvom DTO-u ima jmbg
        return user.korisnickoIme ? "/home-bibliotekar" : "/home-clan";
    };

    return (
        <Routes>
            {/*Ruta koja se otvara prilikom pokretanja aplikacije */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            
            <Route path="/home-bibliotekar" element={<PrivateRoute><HomeBibliotekar /></PrivateRoute>} />
            <Route path="/home-clan" element={<PrivateRoute><HomeClan /></PrivateRoute>} />
            <Route path="/book/:id" element={<PrivateRoute><BookDetails /></PrivateRoute>} />
            <Route path = "/add-book" element = {<PrivateRoute><BookAdd /></PrivateRoute>}/>

            <Route path="/" element={<Navigate to={getHomeRedirect()} />} />
            <Route path="*" element={<Navigate to="/" />} />
        </Routes>
    );
}

export default App;