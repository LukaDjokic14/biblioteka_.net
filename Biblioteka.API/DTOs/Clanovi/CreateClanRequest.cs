namespace Biblioteka.API.DTOs.Clanovi
{
    public class CreateClanRequest
    {
        public string Jmbg { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string BrojTelefona { get; set; } = string.Empty;
        public string KorisnickoIme { get; set; } = string.Empty;
        public string Lozinka { get; set; } = string.Empty;
        public string GradNaziv { get; set; } = string.Empty;
    }
}
