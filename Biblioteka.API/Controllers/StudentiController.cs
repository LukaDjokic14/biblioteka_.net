using Biblioteka.API.DTOs.Autori;
using Biblioteka.API.DTOs.Student;
using Biblioteka.Domain;
using Biblioteka.Domain.Repozitorijumi;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteka.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentiController: ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public StudentiController(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public ActionResult<StudentDto> Create([FromBody] CreateStudentRequest request)
        {
            var student = new Student
            {
                Ime = request.Ime,
                Prezime = request.Prezime,
            };

            _unitOfWork.Studenti.Add(student);
            _unitOfWork.SaveChanges();

            var dto = new StudentDto
            {
                StudentID = student.StudentID,
                Ime = student.Ime,
                Prezime = student.Prezime
            };

            return Ok(dto);
        }
    }
}
