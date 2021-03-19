using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        //a classe herdada "IdentityUser" é pre requisito para se trabalhar com o Identity. 

        public string FullName { get; set; }

        [ForeignKey("UsuarioId")] // a chave estrangeira vai para a tabela mais fraca
        public virtual ICollection<Tarefa> Tarefas { get; set; }
    }
}
