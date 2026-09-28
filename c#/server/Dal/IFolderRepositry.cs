using Models;

namespace Dal;

public interface IFolderRepository
{
    Task<IEnumerable<Folder>> GetAllFoldersAsync(string username);
    Task<Folder?> GetFolderByIdAsync(string username, int folderId);
    Task InsertFolderAsync(string username, Folder folder);
    Task UpdateFolderAsync(string username, Folder folder);
    Task<bool> DeleteFolderAsync(string username, int folderId);
}