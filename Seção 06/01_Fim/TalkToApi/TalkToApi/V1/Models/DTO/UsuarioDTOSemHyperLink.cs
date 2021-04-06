﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TalkToApi.V1.Models.DTO
{
    public class UsuarioDTOSemHyperLink
    {
        public string Id { get; set; } // string pq o Id vem do ApplicationUser

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Slogan { get; set; }
    }
}
