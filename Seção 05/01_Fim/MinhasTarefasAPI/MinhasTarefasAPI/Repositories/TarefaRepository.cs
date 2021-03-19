using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Repositories
{
    public class TarefaRepository : ITarefaRepository
    {
        private readonly MinhasTarefasContext _banco;

        public TarefaRepository(MinhasTarefasContext banco)
        {
            _banco = banco;
        }

        public List<Tarefa> Restauracao(ApplicationUser usuario, DateTime dataUltimaRestauracao)
        {
            var query = _banco.Tarefas.Where(a => a.UsuarioId == usuario.Id).AsQueryable();

            if (dataUltimaRestauracao == null)
            {
                query.Where(a => a.Criado >= dataUltimaRestauracao || a.Atualizado >= dataUltimaRestauracao);
            }

            return query.ToList<Tarefa>();
        }

        public void Sincronizacao(List<Tarefa> tarefas)
        {
            //cadastrar novos registros

            //atualização de registro (excluido)



            throw new NotImplementedException();
        }
    }
}
