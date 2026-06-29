using Biblioteka.Domain;
using Microsoft.EntityFrameworkCore;


namespace Biblioteka.Infrastructure
{
    public class BibliotekaContext : DbContext
    {
        public BibliotekaContext(DbContextOptions<BibliotekaContext> options)
            : base(options)
        {
        }

        public DbSet<Knjiga> Knjige { get; set; }
        public DbSet<Autor> Autori { get; set; }
        public DbSet<Clan> Clanovi { get; set; }
        public DbSet<Bibliotekar> Bibliotekari { get; set; }
        public DbSet<Izdavanje> Izdavanja { get; set; }
        public DbSet<Grad> Gradovi { get; set; }

        public DbSet<Student> Studenti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Podesavanje primarnih kljuceva koji odstupaju od konvencije
            modelBuilder.Entity<Clan>().HasKey(c => c.Jmbg);
            modelBuilder.Entity<Bibliotekar>().HasKey(b => b.Jmbg);

            //Knjiga i Autor (preko tabele Pisanje)

            // 1. Definisanje kompozitnog primarnog ključa
            modelBuilder.Entity<Pisanje>()
                .HasKey(p => new { p.KnjigaId, p.AutorId });

            // 2. Knjiga -> Pisanje
            modelBuilder.Entity<Pisanje>()
                .HasOne(p => p.Knjiga)
                .WithMany(k => k.Pisanja) // Povezujemo se na listu Pisanja u klasi Knjiga
                .HasForeignKey(p => p.KnjigaId);

            // 3. Relacija Autor -> Pisanje 
            modelBuilder.Entity<Pisanje>()
                .HasOne(p => p.Autor)
                .WithMany(a => a.Pisanja) 
                .HasForeignKey(p => p.AutorId);


            // Knjiga i Izdavanje (preko tabele StavkaIzdavanja)

            //Definisanje kompozitnog primarnog ključa
            modelBuilder.Entity<StavkaIzdavanja>().HasKey(si => new { si.IzdavanjeId, si.KnjigaId });

            //Relacija Izdavanje -> StavkaIzdavanja
            modelBuilder.Entity<StavkaIzdavanja>()
                .HasOne(si => si.Izdavanje)
                .WithMany(i => i.StavkeIzdavanja)
                .HasForeignKey(si => si.IzdavanjeId);

            // Relacija Knjiga -> StavkaIzdavanja
            modelBuilder.Entity<StavkaIzdavanja>()
                .HasOne(si => si.Knjiga)
                .WithMany(k => k.StavkeIzdavanja)
                .HasForeignKey(si => si.KnjigaId);
        }
    }
}
