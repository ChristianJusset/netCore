using _03.Helpers;
using System.ComponentModel.DataAnnotations;

namespace _03.Entities
{
    public class Libro
    {
        public int Id { get; set; }
        [PrimeraLetraMayuscula]
        [StringLength(10, ErrorMessage = "El campo Titulo debe tener {1} caracteres o menos")]
        public string Titulo { get; set; }
        public int AutorId { get; set; }
        public Autor Autor { get; set; }
    }
}
