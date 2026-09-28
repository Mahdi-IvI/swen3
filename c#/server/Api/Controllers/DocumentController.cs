using Api.DTOs;
using Bll.@new;
using Bll.@new.Exceptions;
using Models;

namespace Api.Controllers;

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        var documents = await documentService.FindMatchingDocumentsAsync(username);

        return Ok(documents.Select(ToDto));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> Search(
        [FromQuery(Name = "term"), Required] string searchText)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        var documents = await documentService.FindMatchingDocumentsAsync(username, searchText);

        return Ok(documents.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetById(int id)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(ToDto(await documentService.GetDocumentByIdAsync(username, id)));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Insert([FromBody] DocumentDto documentDto)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            var document = ToModel(documentDto, username);
            document.Id = 0;
            document = await documentService.InsertDocumentAsync(username, document);
            var createdDocument = ToDto(document);

            logger.LogInformation("User {Username} created Document {DocumentId}.", username, createdDocument.Id);

            return Created($"/api/documents/{createdDocument.Id}", createdDocument);
        }
        catch (DocumentAlreadyExistsException e)
        {
            logger.LogWarning(e, "User {Username} tried to create a duplicate Document.", username);
            return Conflict(e.Message);
        }
        catch (FolderNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DocumentDto>> Update(int id, [FromBody] DocumentDto documentDto)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        if (documentDto.Id != 0 && documentDto.Id != id)
        {
            logger.LogWarning("User {Username} sent mismatched route id {RouteId} for document {DocumentId}.",
                username, documentDto.Id, id);
            return BadRequest("Route id does not match Document id.");
        }

        documentDto.Id = id;

        try
        {
            var document = await documentService.UpdateDocumentAsync(username, ToModel(documentDto, username));
            logger.LogInformation("User {Username} updated Document {DocumentId}.", username, id);
            return Ok(ToDto(document));
        }
        catch (DocumentNotFoundException e)
        {
            logger.LogWarning(e, "User {Username} tried to update missing document {DocumentId}.", username, id);
            return NotFound(e.Message);
        }
        catch (FolderNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            await documentService.RemoveDocumentAsync(username, id);
            logger.LogInformation("User {Username} deleted Document {DocumentId}.", username, id);
            return NoContent();
        }
        catch (DocumentNotFoundException e)
        {
            logger.LogWarning(e, "User {Username} tried to delete missing Document {DocumentId}.", username, id);
            return NotFound(e.Message);
        }
    }

    private string? GetUsername()
    {
        return User.FindFirstValue(ClaimTypes.Name);
    }

    static DocumentDto ToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Name = document.Name,
            Description = document.Description,
            FileName = document.FileName,
            ContainingFolder = document.ContainingFolder
        };
    }

    static Document ToModel(DocumentDto dto, string username)
    {
        return new Document
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            FileName = dto.FileName,
            ContainingFolder = dto.ContainingFolder,
            Username = username,
            Summery = ""
        };
    }
}
