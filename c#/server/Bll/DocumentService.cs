using Bll.@new.Exceptions;
using Dal;
using Models;

namespace Bll.@new;

public class DocumentService(IDocumentRepository documentRepository) : IDocumentService
{
    public async Task<Document> InsertDocumentAsync(string username, Document newDocument)
    {
        newDocument.Username = username;
        try
        {
            await documentRepository.InsertDocumentAsync(username, newDocument);
        }
        catch (DuplicateKeyException e)
        {
            throw new DocumentAlreadyExistsException(
                $"Document with ID '{newDocument.Id}' already exists for user '{username}'.",
                e);
        }

        return newDocument;
    }

    public async Task<Document> GetDocumentByIdAsync(string username, int documentId)
    {
        return await documentRepository.GetDocumentByIdAsync(username, documentId)
               ?? throw new DocumentNotFoundException(
                   $"Document with ID '{documentId}' not found for user '{username}'.");
    }

    public async Task<Document> UpdateDocumentAsync(string username, Document document)
    {
        document.Username = username;

        try
        {
            await documentRepository.UpdateDocumentAsync(username, document);
        }
        catch (KeyNotFoundException e)
        {
            throw new DocumentNotFoundException($"Document with ID '{document.Id}' not found for user '{username}'.",
                e);
        }

        return document;
    }

    public async Task RemoveDocumentAsync(string username, int documentId)
    {
        if (!await documentRepository.DeleteDocumentAsync(username, documentId))
        {
            throw new DocumentNotFoundException($"Document with ID '{documentId}' not found for user '{username}'.");
        }
    }

    public async Task<IEnumerable<Document>> FindMatchingDocumentsAsync(string username, string? searchText = null)
    {
        var documents = (await documentRepository.GetAllDocumentsAsync(username)).ToList();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return documents;
        }

        var cleanSearchText = searchText.Trim();

        return documents
            .Where(d => DocumentMatchesSearch(d, cleanSearchText));
    }


    private static bool DocumentMatchesSearch(Document document, string searchText)
    {
        return ContainsSearchText(document.Name, searchText)
               || ContainsSearchText(document.Description, searchText)
               || ContainsSearchText(document.FileName, searchText);
    }

    private static bool ContainsSearchText(string value, string searchText)
    {
        return value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}
