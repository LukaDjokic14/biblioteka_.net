namespace Biblioteka.API.DTOs.Auth
{
    public class LoginResultDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public string? Uloga { get; set; }
        public string? Jmbg { get; set; }
        public string? Ime { get; set; }
        public string? Prezime { get; set; }
        public string? KorisnickoIme { get; set; }
        public string? BrojTelefona { get; set; }
        public string? GradNaziv { get; set; }
    }
}
