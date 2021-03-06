using Microsoft.EntityFrameworkCore;
using MimicAPI.v1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.Database
{
    public class MimicContext : DbContext
    {
        public MimicContext(DbContextOptions<MimicContext> options) : base(options)
        {

        }

        //criado construtor vazio para realização de teste unitário
        public MimicContext()
        {

        }

        public virtual DbSet<Palavra> Palavras { get; set; }
    }
}
