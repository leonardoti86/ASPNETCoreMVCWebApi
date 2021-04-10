using MinhasTarefasAPI.v1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.v1.Repositories.Contracts
{
    public interface ITokenRepository
    {
        void Cadastrar(Token token);
        Token Obter(string refreshToken); // banco de dados key-value é mais performartico nessa situação
        void Atualizar(Token token);
    }
}
