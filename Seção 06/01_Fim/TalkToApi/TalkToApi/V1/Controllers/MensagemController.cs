using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MensagemController : ControllerBase
    {
        //injeções
        private readonly IMensagemRepository _mensagemRepository;

        public MensagemController(IMensagemRepository mensagemRepository)
        {
            _mensagemRepository = mensagemRepository;
        }

        [Authorize]
        [HttpGet("{usuarioUmId}/{usuarioDoisId}")]
        public ActionResult Obter(string usuarioUmId, string usuarioDoisId)
        {
            if (usuarioUmId.Equals(usuarioDoisId))
            {
                return UnprocessableEntity();
            }

            return Ok(_mensagemRepository.ObterMensagens(usuarioUmId, usuarioDoisId));
        }

        [Authorize]
        [HttpPost("")]
        public ActionResult Cadastrar([FromBody] Mensagem mensagem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _mensagemRepository.Cadastrar(mensagem);
                    return Ok(mensagem);
                }
                catch (Exception e)
                {
                    return UnprocessableEntity(e);
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }

        }

        [Authorize]
        [HttpPatch("{id}")]
        public ActionResult AtualizacaoParcial(int id, [FromBody]JsonPatchDocument<Mensagem> jsonPatch)
        {
            /*
             * exemplo de chamada PATCH
             * JSONPATCH - [
             * { "op": "add|remove|replace", "path": "texto", "value": "Mensagem substituida!"},
             * { "op": "add|remove|replace", "path": "excluido", "value": true}
             * ]
             */

            if (jsonPatch == null)
            {
                return BadRequest();
            }

            var mensagem = _mensagemRepository.Obter(id);

            jsonPatch.ApplyTo(mensagem);
            mensagem.Atualizado = DateTime.UtcNow;
            _mensagemRepository.Atualizar(mensagem);
            return Ok(mensagem);
        }
    }
}
