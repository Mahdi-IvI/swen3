using Bll.@new.Exceptions;
using Dal;
using Models;

namespace Bll.@new;

public class FolderService(IFolderRepository folderRepository) : IFolderService
{
    public async Task<Folder> InsertFolderAsync(string username, Folder newFolder)
    {
        newFolder.Username = username;
        await ValidateParentAsync(username, newFolder.ContainingFolder);
        try
        {
            await folderRepository.InsertFolderAsync(username, newFolder);
        }
        catch (DuplicateKeyException e)
        {
            throw new FolderAlreadyExistsException(
                $"Folder with ID '{newFolder.Id}' already exists for user '{username}'.",
                e);
        }

        return newFolder;
    }

    public Task<IEnumerable<Folder>> GetAllFoldersAsync(string username)
    {
        return folderRepository.GetAllFoldersAsync(username);
    }

    public async Task<Folder> GetFolderByIdAsync(string username, int folderId)
    {
        return await folderRepository.GetFolderByIdAsync(username, folderId)
               ?? throw new FolderNotFoundException(
                   $"Folder with ID '{folderId}' not found for user '{username}'.");
    }

    public async Task<Folder> UpdateFolderAsync(string username, Folder folder)
    {
        folder.Username = username;
        await ValidateParentAsync(username, folder.ContainingFolder, folder.Id);

        try
        {
            await folderRepository.UpdateFolderAsync(username, folder);
        }
        catch (KeyNotFoundException e)
        {
            throw new FolderNotFoundException($"Folder with ID '{folder.Id}' not found for user '{username}'.",
                e);
        }

        return folder;
    }

    public async Task RemoveFolderAsync(string username, int folderId)
    {
        if (!await folderRepository.DeleteFolderAsync(username, folderId))
        {
            throw new FolderNotFoundException($"Folder with ID '{folderId}' not found for user '{username}'.");
        }
    }

    private async Task ValidateParentAsync(string username, int? parentId, int? folderId = null)
    {
        var visited = new HashSet<int>();
        while (parentId is int id)
        {
            if (id == folderId || !visited.Add(id))
            {
                throw new ArgumentException("A folder cannot be placed inside itself or one of its descendants.");
            }

            var parent = await folderRepository.GetFolderByIdAsync(username, id)
                         ?? throw new FolderNotFoundException(
                             $"Parent folder with ID '{id}' not found for user '{username}'.");
            parentId = parent.ContainingFolder;
        }
    }
}
