using Models;

namespace Bll.@new;

public interface IFolderService
{
    Task<IEnumerable<Folder>> GetAllFoldersAsync(string username);
    Task<Folder> InsertFolderAsync(string username, Folder newFolder);
    Task<Folder> GetFolderByIdAsync(string username, int folderId);
    Task<Folder> UpdateFolderAsync(string username, Folder folder);
    Task RemoveFolderAsync(string username, int folderId);
}
