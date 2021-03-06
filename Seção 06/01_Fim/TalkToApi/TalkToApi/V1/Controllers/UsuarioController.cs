using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TalkToApi.V1.Repositories.Contracts;
using TalkToApi.V1.Models;
using TalkToApi.V1.Models.DTO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using TalkToApi.Helpers.Constants;
using Microsoft.AspNetCore.Cors;

namespace TalkToApi.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AnyOrigin")]
    public class UsuarioController : ControllerBase
    {
        //injeções
        private readonly IMapper _mapper;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;


        public UsuarioController(IMapper mapper, IUsuarioRepository usuarioRepository, ITokenRepository tokenRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _usuarioRepository = usuarioRepository;
            _tokenRepository = tokenRepository;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("", Name = "UsuarioObterTodos")]
        [DisableCors]
        public ActionResult ObterTodos([FromHeader(Name = "Accept")] string mediaType)
        {
            var usuariosAppUser = _userManager.Users.ToList();

            //utilizando content negociation
            if (mediaType.Equals(CustomMediaType.HATEOAS))
            {
                var listaUsuarioDTO = _mapper.Map<List<ApplicationUser>, List<UsuarioDTO>>(usuariosAppUser);

                foreach (var usuarioDTO in listaUsuarioDTO)
                {
                    usuarioDTO.Links.Add(new LinkDTO("_self", Url.Link("UsuarioObter", new { id = usuarioDTO.Id }), "GET"));
                }

                var lista = new ListaDTO<UsuarioDTO>() { Lista = listaUsuarioDTO };
                lista.Links.Add(new LinkDTO("_self", Url.Link("UsuarioObterTodos", null), "GET"));

                return Ok(lista);
            }
            else
            {
                var usuarioResult = _mapper.Map<List<ApplicationUser>, List<UsuarioDTOSemHyperLink>>(usuariosAppUser);
                return Ok(usuarioResult);
            }
        }

        [Authorize]
        [HttpGet("{id}", Name = "UsuarioObter")]
        public ActionResult ObterUsuario(string id, [FromHeader(Name = "Accept")] string mediaType)
        {
            var usuario = _userManager.FindByIdAsync(id).Result;
            if (usuario == null)
            {
                return NotFound();
            }

            //utilizando content negociation
            if (mediaType.Equals(CustomMediaType.HATEOAS))
            {
                var usuarioDTOdb = _mapper.Map<ApplicationUser, UsuarioDTO>(usuario);
                usuarioDTOdb.Links.Add(new LinkDTO("_self", Url.Link("UsuarioObter", new { id = usuario.Id }), "GET"));
                usuarioDTOdb.Links.Add(new LinkDTO("_atualizar", Url.Link("UsuarioAtualizar", new { id = usuario.Id }), "PUT"));

                return Ok(usuarioDTOdb);
            }
            else
            {
                var usuarioResult = _mapper.Map<ApplicationUser, UsuarioDTOSemHyperLink>(usuario);
                return Ok(usuarioResult);
            }
        }

        [HttpPost("", Name = "UsuarioCadastrar")]
        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (ModelState.IsValid)
            {
                //ApplicationUser usuario = new ApplicationUser()
                //{
                //    FullName = usuarioDTO.Nome,
                //    UserName = usuarioDTO.Email, //tipo nome de usuario para fazer login. deve ser único.
                //    Email = usuarioDTO.Email
                //};
                ApplicationUser usuario = _mapper.Map<UsuarioDTO, ApplicationUser>(usuarioDTO); // faz o passo de cima

                var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

                if (!resultado.Succeeded)
                {
                    List<string> erros = new List<string>();
                    foreach (var erro in resultado.Errors)
                    {
                        erros.Add(erro.Description);
                    }
                    return UnprocessableEntity(erros);
                }
                else
                {
                    //utilizando content negociation
                    if (mediaType.Equals(CustomMediaType.HATEOAS))
                    {
                        var usuarioDTOdb = _mapper.Map<ApplicationUser, UsuarioDTO>(usuario);

                        usuarioDTOdb.Links.Add(new LinkDTO("_self", Url.Link("UsuarioCadastrar", new { id = usuario.Id }), "POST"));
                        usuarioDTOdb.Links.Add(new LinkDTO("_obter", Url.Link("UsuarioObter", new { id = usuario.Id }), "GET"));
                        usuarioDTOdb.Links.Add(new LinkDTO("_atualizar", Url.Link("UsuarioAtualizar", new { id = usuario.Id }), "PUT"));

                        return Ok(usuarioDTOdb);
                    }
                    else
                    {
                        var usuarioResult = _mapper.Map<ApplicationUser, UsuarioDTOSemHyperLink>(usuario);
                        return Ok(usuarioResult);
                    }
                }

            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [Authorize]
        [HttpPut("{id}", Name = "UsuarioAtualizar")]
        public ActionResult Atualizar(string id, [FromBody] UsuarioDTO usuarioDTO, [FromHeader(Name = "Accept")] string mediaType)
        {
            ApplicationUser usuario = _userManager.GetUserAsync(HttpContext.User).Result;
            if (usuario.Id != id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                usuario.FullName = usuarioDTO.Nome;
                usuario.UserName = usuarioDTO.Email; //tipo nome de usuario para fazer login. deve ser único.
                usuario.Email = usuarioDTO.Email;
                usuario.Slogan = usuarioDTO.Slogan;

                var resultado = _userManager.UpdateAsync(usuario).Result;
                _userManager.RemovePasswordAsync(usuario);
                _userManager.AddPasswordAsync(usuario, usuarioDTO.Senha);

                if (!resultado.Succeeded)
                {
                    List<string> erros = new List<string>();
                    foreach (var erro in resultado.Errors)
                    {
                        erros.Add(erro.Description);
                    }
                    return UnprocessableEntity(erros);
                }
                else
                {
                    //utilizando content negociation
                    if (mediaType.Equals(CustomMediaType.HATEOAS))
                    {
                        var usuarioDTOdb = _mapper.Map<ApplicationUser, UsuarioDTO>(usuario);

                        usuarioDTOdb.Links.Add(new LinkDTO("_self", Url.Link("UsuarioAtualizar", new { id = usuario.Id }), "PUT"));
                        usuarioDTOdb.Links.Add(new LinkDTO("_obter", Url.Link("UsuarioObter", new { id = usuario.Id }), "GET"));

                        return Ok(usuarioDTOdb);
                    }
                    else
                    {
                        var usuarioResult = _mapper.Map<ApplicationUser, UsuarioDTOSemHyperLink>(usuario);
                        return Ok(usuarioResult );
                    }
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] UsuarioDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmacaoSenha");

            if (ModelState.IsValid)
            {
                ApplicationUser usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);

                if (usuario != null)
                {
                    //comando que faz o login
                    //_signInManager.SignInAsync(usuario, false); //não usar o login do Identity pq guarda estado no cookie. o JWT não guarda.

                    //retornar o token (JWT)
                    return GerarToken(usuario);
                }
                else
                {
                    return NotFound("Usuario não localizado.");
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }


        }

        [HttpPost("renovar")]
        public ActionResult Renovar([FromBody] TokenDTO tokenDTO)
        {
            var refreshTokenDB = _tokenRepository.Obter(tokenDTO.RefreshToken);

            if (refreshTokenDB == null)
                return NotFound();

            //pegar o refreshToken antigo e atualizar(desativar esse refreshToken)
            refreshTokenDB.Atualizado = DateTime.UtcNow;
            refreshTokenDB.Utilizado = true;
            _tokenRepository.Atualizar(refreshTokenDB);

            //gerar um novo token com seu refresh token e salvar no banco
            var usuario = _usuarioRepository.Obter(refreshTokenDB.UsuarioId);
            return GerarToken(usuario);
        }

        private TokenDTO BuildToken(ApplicationUser usuario)
        {
            //cria as claims do JWT. existem iss, aud, exp etc
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            //chave
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas")); //recomendado definir a chave no appsettings.json

            //assinatura
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //data expiração do token
            var exp = DateTime.UtcNow.AddHours(1); //UtcNow pega o fuso do cliente

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: exp,
                signingCredentials: sign
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();

            var expRefreshToken = DateTime.UtcNow.AddHours(2); //precisa ser maior que o prazo do token anterior

            var tokenDTO = new TokenDTO { Token = tokenString, Expiration = exp, ExpirationRefreshToken = expRefreshToken, RefreshToken = refreshToken };

            return tokenDTO;
        }

        private ActionResult GerarToken(ApplicationUser usuario)
        {
            TokenDTO token = BuildToken(usuario);

            //Salvar o token no banco
            var tokenModel = new Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                ExpirationToken = token.Expiration,
                Usuario = usuario,
                Criado = DateTime.Now,
                Utilizado = false
            };

            _tokenRepository.Cadastrar(tokenModel);

            return Ok(token);
        }
    }
}
