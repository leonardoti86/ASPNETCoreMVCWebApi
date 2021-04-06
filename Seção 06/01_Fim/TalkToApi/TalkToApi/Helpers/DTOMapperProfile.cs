using AutoMapper;
using System.Collections.Generic;
using TalkToApi.V1.Models;
using TalkToApi.V1.Models.DTO;

namespace TalkToApi.Helpers
{
    public class DTOMapperProfile : Profile
    {
        public DTOMapperProfile()
        {
            //CreateMap<Palavra, PalavraDTO>();
            //CreateMap<PaginationList<Palavra>, PaginationList<PalavraDTO>>();

            CreateMap<ApplicationUser, UsuarioDTO>()
                .ForMember(dest => dest.Nome, ori => ori.MapFrom(src => src.FullName)); //aqui setamos qual propriedade da origem irá para a destino

            CreateMap<ApplicationUser, UsuarioDTOSemHyperLink>()
                .ForMember(dest => dest.Nome, ori => ori.MapFrom(src => src.FullName)); //aqui setamos qual propriedade da origem irá para a destino
            

            CreateMap<Mensagem, MensagemDTO>();

        }
    }
}
