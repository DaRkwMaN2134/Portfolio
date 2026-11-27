using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.Interfaces
{
    public interface IChannelHandler<T>
    {
        Task HandleAsync(T data, CancellationToken cancellationToken);
    }
}
