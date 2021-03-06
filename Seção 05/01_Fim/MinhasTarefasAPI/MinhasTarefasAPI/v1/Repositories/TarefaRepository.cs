using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.v1.Models;
using MinhasTarefasAPI.v1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.v1.Repositories
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

            return query.ToList();
        }

        public List<Tarefa> Sincronizacao(List<Tarefa> tarefas)
        {
            //lista todas as tarefas que não foram persistidas no banco
            var tarefasNovas = tarefas.Where(t => t.IdTarefaApi == 0).ToList();
            //lista todas as tarefas que já foram persistidas no banco
            var tarefasExcluidasAtualizadas = tarefas.Where(t => t.IdTarefaApi != 0).ToList();

            // 1) cadastrar novos registros
            if (tarefasNovas.Count() > 0)
            {
                foreach (var tarefa in tarefasNovas)
                {
                    _banco.Tarefas.Add(tarefa);
                }
            }


            // 2) atualização de registro (excluido)
            if (tarefasExcluidasAtualizadas.Count() > 0)
            {
                foreach (var tarefa in tarefasExcluidasAtualizadas)
                {
                    _banco.Tarefas.Update(tarefa);
                }
            }

            _banco.SaveChanges();

            return tarefasNovas.ToList();
        }
    }
}
