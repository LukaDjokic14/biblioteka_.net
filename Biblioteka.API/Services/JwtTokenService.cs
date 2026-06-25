using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Biblioteka.API.Service
{
    public class JwtTokenService
    {
        public string CreateToken(string korisnickoIme, string uloga, string jmbg)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, jmbg),
                new Claim(ClaimTypes.Name, korisnickoIme),
                new Claim(ClaimTypes.Role, uloga) 
            };

            // Važno: Ovu tajnu reč inače čuvamo u appsettings.json, ali za projekat može i ovako direktno
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("OvoJeNekiDovoljnoDugacakStringZaPotpisBiblioteka"));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                 claims: claims,
                 signingCredentials: credentials,
                 issuer: "Biblioteka.API",
                 audience: "Biblioteka.Client",
                 expires: DateTime.Now.AddHours(2) // Dodali smo da token ističe za 2 sata
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
