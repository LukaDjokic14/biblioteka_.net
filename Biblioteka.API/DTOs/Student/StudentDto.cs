namespace Biblioteka.API.DTOs.Student
{
    public class StudentDto
    {
        public int StudentID { get; set; }
        public string Ime { get; set; } = string.Empty;

        public string Prezime { get; set; } = string.Empty;
    }
}
