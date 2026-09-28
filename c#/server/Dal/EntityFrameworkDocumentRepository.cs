using Microsoft.EntityFrameworkCore;
using Models;
using Npgsql;

namespace Dal;

public class EntityFrameworkDocumentRepository(AppDbContext dbContext) : IDocumentRepository
{
    public async Task<IEnumerable<Document>> GetAllDocumentsAsync(string username)
    {
        return await dbContext.Documents.AsNoTracking().Where(f => f.Username == username).ToListAsync();
    }

    public async Task<Document?> GetDocumentByIdAsync(string username, int documentId)
    {
        return await dbContext.Documents.SingleOrDefaultAsync(f => f.Username == username && f.Id == documentId);
    }

    public async Task InsertDocumentAsync(string username, Document document)
    {
        document.Username = username;

        try
        {
            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            dbContext.ChangeTracker.Clear();
            throw new DuplicateKeyException($"Document with ID '{document.Id}' already exists", e);
        }
    }

    public async Task UpdateDocumentAsync(string username, Document document)
    {
        var updateDocument = await GetDocumentByIdAsync(username, document.Id);
        if (updateDocument == null)
        {
            throw new KeyNotFoundException($"Document with Id '{document.Id}' for user '{username}' not found.");
        }

        updateDocument.Name = document.Name;
        updateDocument.Description = document.Description;
        updateDocument.FileName = document.FileName;
        updateDocument.ContainingFolder = document.ContainingFolder;
        updateDocument.Summery = document.Summery;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new KeyNotFoundException($"Document with Id '{document.Id}' for user '{username}' not found.", e);
        }
    }

    public async Task<bool> DeleteDocumentAsync(string username, int documentId)
    {
        var document = await GetDocumentByIdAsync(username, documentId);
        if (document == null)
        {
            return false;
        }

        dbContext.Documents.Remove(document);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }

        return true;
    }
}
