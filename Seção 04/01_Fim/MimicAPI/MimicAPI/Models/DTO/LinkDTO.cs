using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.Models.DTO
{
    public class LinkDTO
    {
        public string Rel { get; set; } //significado
        public string Href { get; set; } //o link
        public string Method { get; set; }

        public LinkDTO(string rel, string href, string method)
        {
            Rel = rel;
            Href = href;
            Method = method;               
        }
    }
}
