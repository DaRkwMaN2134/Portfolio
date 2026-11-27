using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.Interfaces
{
    public interface IBackgroundChannelTask<T>
    {
        public ValueTask ChannelWriteAsync(T task, CancellationToken cancellationToken);
        public IAsyncEnumerable<T> ChannelReadAsync(CancellationToken cancellationToken);
    }
}
