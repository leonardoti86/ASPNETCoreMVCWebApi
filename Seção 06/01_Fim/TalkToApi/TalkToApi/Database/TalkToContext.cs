using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalkToApi.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TalkToApi.Database
{
    //public class TalkToContext : DbContext
    public class TalkToContext : IdentityDbContext<ApplicationUser>

    {
        public TalkToContext(DbContextOptions<TalkToContext> options) : base(options)
        {

        }

        public DbSet<Mensagem> Mensagem { get; set; }
        public DbSet<Token> Tokens { get; set; }

    }
}
