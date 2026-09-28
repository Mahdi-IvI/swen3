using Bll.@new;
using Bll.@new.Exceptions;
using Dal;
using Models;
using Moq;
using NUnit.Framework;

namespace Tests;

public class FolderServiceTests
{
    [Test]
    public async Task InsertFolder_StoresUsername()
    {
        var service = CreateService();

        var folder = await service.InsertFolderAsync("mahdi", CreateFolder());

        Assert.That(folder.Username, Is.EqualTo("mahdi"));
    }

    [Test]
    public void InsertFolder_WhenDuplicate()
    {
        var repository = CreateFolderRepository();
        repository.Setup(r => r.InsertFolderAsync(It.IsAny<string>(), It.IsAny<Folder>()))
            .ThrowsAsync(new DuplicateKeyException("duplicate"));
        var service = CreateService(repository);

        Assert.ThrowsAsync<FolderAlreadyExistsException>(() =>
            service.InsertFolderAsync("mahdi", CreateFolder()));
    }

    [Test]
    public void UpdateFolder_WhenFolderMissing()
    {
        var repository = CreateFolderRepository();
        repository.Setup(r => r.UpdateFolderAsync(It.IsAny<string>(), It.IsAny<Folder>()))
            .ThrowsAsync(new KeyNotFoundException());
        var service = CreateService(repository);

        Assert.ThrowsAsync<FolderNotFoundException>(() =>
            service.UpdateFolderAsync("mahdi", CreateFolder()));
    }

    [Test]
    public void RemoveFolder_WhenFolderMissing()
    {
        var service = CreateService();

        Assert.ThrowsAsync<FolderNotFoundException>(() => service.RemoveFolderAsync("mahdi", 99));
    }

    [Test]
    public async Task GetAllFolders_ExcludesOtherUsers()
    {
        var otherFolder = CreateFolder(2, "Other User Folder");
        otherFolder.Username = "other-user";
        var service = CreateService(CreateFolderRepository(
            [CreateFolder(1, "My Folder"), otherFolder]));

        var folders = await service.GetAllFoldersAsync("mahdi");

        Assert.That(folders.Single().Name, Is.EqualTo("My Folder"));
    }

    [Test]
    public async Task GetFolderById_OnlyFindsOwnersFolder()
    {
        var service = CreateService(CreateFolderRepository([CreateFolder(7, "Private Folder")]));

        var owned = await service.GetFolderByIdAsync("mahdi", 7);
        Assert.That(owned.Name, Is.EqualTo("Private Folder"));

        Assert.ThrowsAsync<FolderNotFoundException>(() =>
            service.GetFolderByIdAsync("other-user", 7));
    }

    [Test]
    public void InsertFolder_WhenParentBelongsToAnotherUser()
    {
        var parent = CreateFolder(1, "Private Parent");
        parent.Username = "other-user";
        var repository = CreateFolderRepository([parent]);
        var child = CreateFolder(2, "Child");
        child.ContainingFolder = parent.Id;
        var service = CreateService(repository);

        Assert.ThrowsAsync<FolderNotFoundException>(() => service.InsertFolderAsync("mahdi", child));
        repository.Verify(r => r.InsertFolderAsync(It.IsAny<string>(), It.IsAny<Folder>()), Times.Never);
    }

    [Test]
    public void UpdateFolder_WhenParentIsDescendant()
    {
        var root = CreateFolder(1, "Root");
        var child = CreateFolder(2, "Child");
        child.ContainingFolder = root.Id;
        var repository = CreateFolderRepository([root, child]);
        root.ContainingFolder = child.Id;
        var service = CreateService(repository);

        Assert.ThrowsAsync<ArgumentException>(() => service.UpdateFolderAsync("mahdi", root));
        repository.Verify(r => r.UpdateFolderAsync(It.IsAny<string>(), It.IsAny<Folder>()), Times.Never);
    }

    private static Folder CreateFolder(int id = 1, string name = "Test Folder") => new()
    {
        Id = id,
        Username = "mahdi",
        Name = name,
        Description = "A simple test folder"
    };

    private static FolderService CreateService(Mock<IFolderRepository>? repository = null) =>
        new((repository ?? CreateFolderRepository()).Object);

    private static Mock<IFolderRepository> CreateFolderRepository(List<Folder>? folders = null)
    {
        folders ??= [];

        var repository = new Mock<IFolderRepository>();
        repository.Setup(r => r.GetAllFoldersAsync(It.IsAny<string>()))
            .ReturnsAsync((string username) => folders.Where(f => f.Username == username));
        repository.Setup(r => r.GetFolderByIdAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string username, int folderId) =>
                folders.FirstOrDefault(f => f.Username == username && f.Id == folderId));
        repository.Setup(r => r.InsertFolderAsync(It.IsAny<string>(), It.IsAny<Folder>()))
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.UpdateFolderAsync(It.IsAny<string>(), It.IsAny<Folder>()))
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.DeleteFolderAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        return repository;
    }
}
