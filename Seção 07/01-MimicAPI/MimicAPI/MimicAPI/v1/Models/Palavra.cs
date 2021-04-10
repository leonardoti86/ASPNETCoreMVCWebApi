using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.v1.Models
{
    public class Palavra
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório!")]
        [MaxLength(150)]
        public string Nome { get; set; }
        [Required(ErrorMessage = "A pontuação é obrigatória!")]
        public int Pontuacao { get; set; }
        public bool Ativo { get; set; }
        public DateTime Criado { get; set; }
        public DateTime? Atualizado { get; set; }
    }
}
