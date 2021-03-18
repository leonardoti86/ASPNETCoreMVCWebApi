using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Helpers;
using MimicAPI.v1.Models;
using MimicAPI.v1.Models.DTO;
using MimicAPI.v1.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.v1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.1")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;


        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Operação que pega do banco todas as palavras existentes.
        /// </summary>
        /// <param name="query">Filtros de pesquisa</param>
        /// <returns>Listagem de palavras</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //[Route("")]
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult ObterTodas([FromQuery] PalavraUrlQuery query)
        {
            var item = _repository.ObterPalavras(query);

            if (item.Results.Count.Equals(0))
            {
                if (query.PagNumero > item.Paginacao.TotalPaginas)
                    return NotFound();
            }

            PaginationList<PalavraDTO> lista = CriarLinksListPalavraDTO(query, item);

            return Ok(lista);
        }


        /// <summary>
        /// Operação que pega uma única palavra da base de dados
        /// </summary>
        /// <param name="id">codigo identificador da palavra</param>
        /// <returns>objeto de palavra</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //[Route("{id}")] //usado no parametro do HttpGet em baixo. isso corrige o problema de enviar via queryString o id.
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")); //Url.Link() é bom pra efeito de implantação. deixa flexível o link
            palavraDTO.Links.Add(new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id }), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.Link("ExcluirPalavra", new { id = palavraDTO.Id }), "DELETE"));

            //palavraDTO.Links

            return Ok(palavraDTO);
        }

        /// <summary>
        /// Operação que realiza o cadastro da palavra
        /// </summary>
        /// <param name="palavra">um objeto palavra</param>
        /// <returns>um objeto palavra com seu Id</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody] Palavra palavra)
        {
            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;
            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")); //Url.Link() é bom pra efeito de implantação. deixa flexível o link 

            return Created($"/api/palavras/{palavra.Id}", palavraDTO);
        }

        /// <summary>
        /// Operação que realiza a substituição de dados de uma palavra específica.
        /// </summary>
        /// <param name="id">Codigo identificador da palavra a ser alterada</param>
        /// <param name="palavra"></param>
        /// <returns></returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //[Route("{id}")]
        [HttpPut("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody] Palavra palavra)
        {
            var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState); 

            palavra.Id = id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;
            _repository.Atualizar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")); //Url.Link() é bom pra efeito de implantação. deixa flexível o link 

            return Ok();
        }

        /// <summary>
        /// Operação que desativa uma palavra do sistema
        /// </summary>
        /// <param name="id">Codigo identificador da palavra</param>
        /// <returns></returns>
        [MapToApiVersion("1.1")]
        //[Route("{id}")]
        [HttpDelete("{id}", Name = "ExcluirPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);

            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);
            //_banco.Palavras.Remove(_banco.Palavras.Find(id));

            return NoContent();
        }

        private PaginationList<PalavraDTO> CriarLinksListPalavraDTO(PalavraUrlQuery query, PaginationList<Palavra> item)
        {
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            foreach (var palavra in lista.Results)
            {                
                palavra.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavra.Id }), "GET"));
            }

            lista.Links.Add(new LinkDTO("self", Url.Link("ObterTodas", query), "GET"));

            if (item.Paginacao != null)
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));

                if (query.PagNumero + 1 <= item.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }

                if (query.PagNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("prev", Url.Link("ObterTodas", queryString), "GET"));
                }
            }

            return lista;
        }
    }
}
