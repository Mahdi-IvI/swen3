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
public class FoldersController(IFolderService folderService, ILogger<FoldersController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FolderDto>>> GetAll()
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        var folders = await folderService.GetAllFoldersAsync(username);

        return Ok(folders.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FolderDto>> GetById(int id)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(ToDto(await folderService.GetFolderByIdAsync(username, id)));
        }
        catch (FolderNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<FolderDto>> Insert([FromBody] FolderDto folderDto)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        try
        {
            var folder = ToModel(folderDto, username);
            folder.Id = 0;
            folder = await folderService.InsertFolderAsync(username, folder);
            var createdFolder = ToDto(folder);

            logger.LogInformation("User {Username} created Folder {FolderId}.", username, createdFolder.Id);

            return Created($"/api/folders/{createdFolder.Id}", createdFolder);
        }
        catch (FolderAlreadyExistsException e)
        {
            logger.LogWarning(e, "User {Username} tried to create a duplicate Folder.", username);
            return Conflict(e.Message);
        }
        catch (FolderNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FolderDto>> Update(int id, [FromBody] FolderDto folderDto)
    {
        var username = GetUsername();
        if (username == null)
        {
            return Unauthorized();
        }

        if (folderDto.Id != 0 && folderDto.Id != id)
        {
            logger.LogWarning("User {Username} sent mismatched route id {RouteId} for folder {FolderId}.",
                username, folderDto.Id, id);
            return BadRequest("Route id does not match Folder id.");
        }

        folderDto.Id = id;

        try
        {
            var folder = await folderService.UpdateFolderAsync(username, ToModel(folderDto, username));
            logger.LogInformation("User {Username} updated Folder {FolderId}.", username, id);
            return Ok(ToDto(folder));
        }
        catch (FolderNotFoundException e)
        {
            logger.LogWarning(e, "User {Username} tried to update missing folder {FolderId}.", username, id);
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
            await folderService.RemoveFolderAsync(username, id);
            logger.LogInformation("User {Username} deleted Folder {FolderId}.", username, id);
            return NoContent();
        }
        catch (FolderNotFoundException e)
        {
            logger.LogWarning(e, "User {Username} tried to delete missing Folder {FolderId}.", username, id);
            return NotFound(e.Message);
        }
    }

    private string? GetUsername()
    {
        return User.FindFirstValue(ClaimTypes.Name);
    }

    static FolderDto ToDto(Folder folder)
    {
        return new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            Description = folder.Description,
            ContainingFolder = folder.ContainingFolder
        };
    }

    static Folder ToModel(FolderDto dto, string username)
    {
        return new Folder
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ContainingFolder = dto.ContainingFolder,
            Username = username,
        };
    }
}
