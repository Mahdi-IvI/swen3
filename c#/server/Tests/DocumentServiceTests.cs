using Bll.@new;
using Bll.@new.Exceptions;
using Bll.@new.Messaging;
using Dal;
using Models;
using Models.Messaging;
using Moq;
using NUnit.Framework;

namespace Tests;

public class DocumentServiceTests
{
    [Test]
    public async Task InsertDocument_StoresUsername()
    {
        var service = CreateService();

        var document = await service.InsertDocumentAsync("mahdi", CreateDocument());

        Assert.That(document.Username, Is.EqualTo("mahdi"));
    }

    [Test]
    public void InsertDocument_WhenDuplicate()
    {
        var repository = CreateDocumentRepository();
        repository.Setup(r => r.InsertDocumentAsync(It.IsAny<string>(), It.IsAny<Document>()))
            .ThrowsAsync(new DuplicateKeyException("duplicate"));
        var publisher = new Mock<IDocumentMessagePublisher>();
        var service = CreateService(documentRepository: repository, messagePublisher: publisher);

        Assert.ThrowsAsync<DocumentAlreadyExistsException>(() =>
            service.InsertDocumentAsync("mahdi", CreateDocument()));
        publisher.Verify(p => p.PublishAsync(It.IsAny<ProcessDocumentMessage>()), Times.Never);
    }

    [Test]
    public async Task InsertDocument_PublishesTheSavedDocument()
    {
        var repository = CreateDocumentRepository();
        repository.Setup(r => r.InsertDocumentAsync("mahdi", It.IsAny<Document>()))
            .Callback<string, Document>((_, document) => document.Id = 42)
            .Returns(Task.CompletedTask);
        var publisher = new Mock<IDocumentMessagePublisher>();
        var service = CreateService(documentRepository: repository, messagePublisher: publisher);

        await service.InsertDocumentAsync("mahdi", CreateDocument(0));

        publisher.Verify(p => p.PublishAsync(It.Is<ProcessDocumentMessage>(message =>
            message.DocumentId == 42 && message.FileName == "name" && message.MessageId != Guid.Empty)),
            Times.Once);
    }

    [Test]
    public void InsertDocument_WhenPublishingFails_ReportsTheFailure()
    {
        var publisher = new Mock<IDocumentMessagePublisher>();
        publisher.Setup(p => p.PublishAsync(It.IsAny<ProcessDocumentMessage>()))
            .ThrowsAsync(new InvalidOperationException("Broker unavailable"));
        var service = CreateService(messagePublisher: publisher);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InsertDocumentAsync("mahdi", CreateDocument()));
    }

    [Test]
    public async Task InsertDocument_WithOwnedFolder()
    {
        var folderRepository = new Mock<IFolderRepository>();
        folderRepository.Setup(r => r.GetFolderByIdAsync("mahdi", 5))
            .ReturnsAsync(new Folder { Id = 5, Username = "mahdi", Name = "Projects", Description = "" });
        var service = CreateService(folderRepository: folderRepository);
        var document = CreateDocument();
        document.ContainingFolder = 5;

        var result = await service.InsertDocumentAsync("mahdi", document);

        Assert.That(result.ContainingFolder, Is.EqualTo(5));
    }

    [Test]
    public void InsertDocument_WithAnotherUsersFolder_IsRejected()
    {
        var documentRepository = CreateDocumentRepository();
        var service = CreateService(documentRepository: documentRepository);
        var document = CreateDocument();
        document.ContainingFolder = 5;

        Assert.ThrowsAsync<FolderNotFoundException>(() => service.InsertDocumentAsync("mahdi", document));
        documentRepository.Verify(r => r.InsertDocumentAsync(It.IsAny<string>(), It.IsAny<Document>()), Times.Never);
    }

    [Test]
    public void UpdateDocument_WhenDocumentMissing()
    {
        var repository = CreateDocumentRepository();
        repository.Setup(r => r.UpdateDocumentAsync(It.IsAny<string>(), It.IsAny<Document>()))
            .ThrowsAsync(new KeyNotFoundException());
        var service = CreateService(documentRepository: repository);

        Assert.ThrowsAsync<DocumentNotFoundException>(() => service.UpdateDocumentAsync("mahdi", CreateDocument()));
    }

    [Test]
    public void RemoveDocument_WhenDocumentMissing()
    {
        var service = CreateService();

        Assert.ThrowsAsync<DocumentNotFoundException>(() => service.RemoveDocumentAsync("mahdi", 99));
    }

    [Test]
    public async Task FindMatchingDocuments_WithEmptySearch()
    {
        var documents = new List<Document>
        {
            CreateDocument(1, "City Walk"),
            CreateDocument(2, "Mountain Bike")
        };
        var service = CreateService(documentRepository: CreateDocumentRepository(documents));

        var result = await service.FindMatchingDocumentsAsync("mahdi", "");

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task FindMatchingDocuments()
    {
        var documents = new List<Document>
        {
            CreateDocument(1, "City Walk"),
            CreateDocument(2, "Mountain Bike")
        };
        var service = CreateService(documentRepository: CreateDocumentRepository(documents));

        var result = await service.FindMatchingDocumentsAsync("mahdi", "Mountain");

        Assert.That(result.Single().Name, Is.EqualTo("Mountain Bike"));
    }

    [Test]
    public async Task GetDocumentById_OnlyFindsOwnersDocument()
    {
        var service = CreateService(documentRepository: CreateDocumentRepository(
            [CreateDocument(7, "Private Document")]));

        var owned = await service.GetDocumentByIdAsync("mahdi", 7);
        Assert.That(owned.Name, Is.EqualTo("Private Document"));

        Assert.ThrowsAsync<DocumentNotFoundException>(() =>
            service.GetDocumentByIdAsync("other-user", 7));
    }

    [Test]
    public async Task FindMatchingDocuments_ExcludesOtherUsers()
    {
        var otherDocument = CreateDocument(2, "Other User Document");
        otherDocument.Username = "other-user";
        var service = CreateService(documentRepository: CreateDocumentRepository(
            [CreateDocument(1, "My Document"), otherDocument]));

        var documents = await service.FindMatchingDocumentsAsync("mahdi");

        Assert.That(documents.Single().Name, Is.EqualTo("My Document"));
    }

    private static Document CreateDocument(int id = 1, string name = "Test Document")
    {
        return new Document
        {
            Id = id,
            Username = "mahdi",
            Name = name,
            Description = "A simple test document",
            FileName = "name",
            Summery = ""
        };
    }

    private static DocumentService CreateService(
        Mock<IDocumentRepository>? documentRepository = null,
        Mock<IFolderRepository>? folderRepository = null,
        Mock<IDocumentMessagePublisher>? messagePublisher = null)
    {
        return new DocumentService(
            (documentRepository ?? CreateDocumentRepository()).Object,
            (folderRepository ?? new Mock<IFolderRepository>()).Object,
            (messagePublisher ?? new Mock<IDocumentMessagePublisher>()).Object);
    }

    private static Mock<IDocumentRepository> CreateDocumentRepository(List<Document>? documents = null)
    {
        documents ??= [];

        var repository = new Mock<IDocumentRepository>();
        repository.Setup(r => r.GetAllDocumentsAsync(It.IsAny<string>()))
            .ReturnsAsync((string username) =>
                documents.Where(t => t.Username == username));
        repository.Setup(r => r.GetDocumentByIdAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string username, int documentId) =>
                documents.FirstOrDefault(t => t.Username == username && t.Id == documentId));
        repository.Setup(r => r.InsertDocumentAsync(It.IsAny<string>(), It.IsAny<Document>()))
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.UpdateDocumentAsync(It.IsAny<string>(), It.IsAny<Document>()))
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        return repository;
    }
}
