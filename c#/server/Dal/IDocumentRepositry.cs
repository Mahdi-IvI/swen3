using Models;

namespace Dal;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetAllDocumentsAsync(string username);
    Task<Document?> GetDocumentByIdAsync(string username, int documentId);
    Task InsertDocumentAsync(string username, Document document);
    Task UpdateDocumentAsync(string username, Document document);
    Task<bool> DeleteDocumentAsync(string username, int documentId);
}