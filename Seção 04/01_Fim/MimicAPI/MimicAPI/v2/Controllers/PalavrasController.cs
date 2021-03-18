﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.v2.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    [ApiVersion("2.0")]
    public class PalavrasController : ControllerBase
    {
        /// <summary>
        /// Operação que pega do banco todas as palavras existentes.
        /// </summary>
        /// <param name="query">Filtros de pesquisa</param>
        /// <returns>Listagem de palavras</returns>
        //[Route("")]
        [HttpGet("", Name = "ObterTodas")]
        public string ObterTodas()
        {
            
            return "Versão 2.0";
        }
    }
}
