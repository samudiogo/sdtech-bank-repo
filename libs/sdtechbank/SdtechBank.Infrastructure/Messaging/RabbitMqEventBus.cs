using SdtechBank.Application.Ports;
using System;
using System.Collections.Generic;
using System.Text;

namespace SdtechBank.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus, IDisposable
{   

    public Task PublishAsync<T>(T @event)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
