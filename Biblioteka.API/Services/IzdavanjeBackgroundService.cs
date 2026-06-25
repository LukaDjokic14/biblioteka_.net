using System;
using System.Linq;
using Biblioteka.Domain.Repozitorijumi;

namespace Biblioteka.API.Service
{
    public class IzdavanjeBackgroundService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IzdavanjeBackgroundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void ProveriStatusIzdavanja()
        {
            
            var granicaZaIstek = DateTime.Now.AddMinutes(-2);

            
            var istekleRezervacije = _unitOfWork.Izdavanja
                .Find(i => i.Status == "REZERVISANO" && i.DatumIzdavanja <= granicaZaIstek)
                .ToList();

            foreach (var rez in istekleRezervacije)
            {
                rez.Status = "ODBIJENO"; 
            }

            
            var isteklaIzdavanja = _unitOfWork.Izdavanja
                .Find(i => i.Status == "IZDATO" && i.DatumIzdavanja <= granicaZaIstek)
                .ToList();

            foreach (var izd in isteklaIzdavanja)
            {
                izd.Status = "VRACENO";
                izd.DatumVracanja = DateTime.Now; 
            }

            
            if (istekleRezervacije.Any() || isteklaIzdavanja.Any())
            {
                _unitOfWork.SaveChanges();
            }
        }
    }
}
