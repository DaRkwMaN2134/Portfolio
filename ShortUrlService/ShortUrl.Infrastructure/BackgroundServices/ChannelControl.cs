using ShortUrl.Application.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShortUrl.Infrastructure.BackgroundServices
{
    public class ChannelControl<T> : IBackgroundChannelTask<T>
    {
        private readonly Channel<T> _channel;

        public ChannelControl()
        {
            var options = new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateUnbounded<T>(options);
        }

        public ValueTask ChannelWriteAsync(T task, CancellationToken cancellationToken)
        {
            return _channel.Writer.WriteAsync(task, cancellationToken);
        }
        public IAsyncEnumerable<T> ChannelReadAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
