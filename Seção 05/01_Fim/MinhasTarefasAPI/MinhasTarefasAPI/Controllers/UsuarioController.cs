using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        //injeções
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;


        public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _usuarioRepository = usuarioRepository;
            _signInManager = signInManager;
            _userManager = userManager;
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
                    _signInManager.SignInAsync(usuario, false);

                    //retornar o token (JWT)
                    return Ok();
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

        [HttpPost("")]
        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO)   
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usuario = new ApplicationUser()
                {
                    FullName = usuarioDTO.Nome,
                    UserName = usuarioDTO.Email, //tipo nome de usuario para fazer login. deve ser único.
                    Email = usuarioDTO.Email
                };

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
                    return Ok(usuario);
                }

            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }
    }
}
