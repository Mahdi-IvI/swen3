using Models.Messaging;

namespace Bll.@new.Messaging;

public interface IDocumentMessagePublisher
{
    Task PublishAsync(ProcessDocumentMessage message);
}
