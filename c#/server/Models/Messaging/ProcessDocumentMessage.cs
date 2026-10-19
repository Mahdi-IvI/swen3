namespace Models.Messaging;

public record ProcessDocumentMessage(Guid MessageId, int DocumentId, string FileName);
