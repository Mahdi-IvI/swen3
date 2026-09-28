using Bll.@new;
using Bll.@new.Exceptions;
using Dal;
using Models;
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
        var service = CreateService(documentRepository: repository);

        Assert.ThrowsAsync<DocumentAlreadyExistsException>(() =>
            service.InsertDocumentAsync("mahdi", CreateDocument()));
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
        Mock<IDocumentRepository>? documentRepository = null)
    {
        return new DocumentService(
            (documentRepository ?? CreateDocumentRepository()).Object);
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
