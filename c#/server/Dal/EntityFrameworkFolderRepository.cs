using Microsoft.EntityFrameworkCore;
using Models;
using Npgsql;

namespace Dal;

public class EntityFrameworkFolderRepository(AppDbContext dbContext) : IFolderRepository
{
    public async Task<IEnumerable<Folder>> GetAllFoldersAsync(string username)
    {
        return await dbContext.Folders.AsNoTracking().Where(f => f.Username == username).ToListAsync();
    }

    public async Task<Folder?> GetFolderByIdAsync(string username, int folderId)
    {
        return await dbContext.Folders.SingleOrDefaultAsync(f => f.Username == username && f.Id == folderId);
    }

    public async Task InsertFolderAsync(string username, Folder folder)
    {
        folder.Username = username;

        try
        {
            dbContext.Folders.Add(folder);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            dbContext.ChangeTracker.Clear();
            throw new DuplicateKeyException($"Folder with ID '{folder.Id}' already exists", e);
        }
    }

    public async Task UpdateFolderAsync(string username, Folder folder)
    {
        var updateFolder = await GetFolderByIdAsync(username, folder.Id);
        if (updateFolder == null)
        {
            throw new KeyNotFoundException($"Folder with Id '{folder.Id}' for user '{username}' not found.");
        }

        updateFolder.Name = folder.Name;
        updateFolder.Description = folder.Description;
        updateFolder.ContainingFolder = folder.ContainingFolder;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new KeyNotFoundException($"Folder with Id '{folder.Id}' for user '{username}' not found.", e);
        }
    }

    public async Task<bool> DeleteFolderAsync(string username, int folderId)
    {
        var folder = await GetFolderByIdAsync(username, folderId);
        if (folder == null)
        {
            return false;
        }

        dbContext.Folders.Remove(folder);
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
