using MicroRabbit.Domain.Core.Event;

namespace MicroRabbit.Domain.Core.Commands;

public abstract class Command : Message
{
    public DateTime TimeStamp { get; protected set; }

    protected Command()
    {
        TimeStamp = DateTime.Now;
    }
}