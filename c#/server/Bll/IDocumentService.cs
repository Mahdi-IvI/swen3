using Models;

namespace Bll.@new;

public interface IDocumentService
{
    Task<Document> InsertDocumentAsync(string username, Document newDocument);
    Task<Document> GetDocumentByIdAsync(string username, int documentId);
    Task<Document> UpdateDocumentAsync(string username, Document document);
    Task RemoveDocumentAsync(string username, int documentId);
    Task<IEnumerable<Document>> FindMatchingDocumentsAsync(string username, string? searchText = null);
}
