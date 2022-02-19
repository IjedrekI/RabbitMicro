using System.Text;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Event;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace MicroRabbit.Infra.Bus;

public sealed class RabbitMqBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;

    public RabbitMqBus(IMediator mediator)
    {
        _mediator = mediator;
        _handlers = new Dictionary<string, List<Type>>();
        _eventTypes = new List<Type>();
    }

    public Task SendCommand<T>(T command) where T : Command
    {
        return _mediator.Send(command);
    }

    public void Publish<T>(T @event) where T : Event
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var eventName = @event.GetType().Name;

        channel.QueueDeclare(eventName, false, false, false, null);

        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        channel.BasicPublish("", eventName, null, body);
    }

    public void Subscribe<T, THandler>() where T : Event where THandler : IEventHandler<T>
    {
        var eventType = typeof(T);
        var eventName = eventType.Name;
        var handlerType = typeof(THandler);
        
        if (!_eventTypes.Contains(eventType))
            _eventTypes.Add(eventType);
        
        if(!_handlers.ContainsKey(eventName))
            _handlers.Add(eventName, new List<Type>());

        if (_handlers[eventName].Any(x => x.GetType() == handlerType))
        {
            throw new ArgumentException();
        }
    }
}