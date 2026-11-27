using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public record RedirectDTO(HeadersDTO HeadersDTO, Click click);
}
