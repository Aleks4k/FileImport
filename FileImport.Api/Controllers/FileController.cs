using FileImport.Application.Files.Commands;
using FileImport.Application.Files.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileImport.Api.Controllers
{
    public class FileController : BaseController
    {
        [HttpGet]
        [Route("download")]
        [AllowAnonymous]
        public async Task<IActionResult> Download([FromQuery] DownloadFileQuery query)
        {
            var result = await Mediator.Send(query);
            return File(
                result.FileStream,
                result.ContentType,
                result.FileName,
                enableRangeProcessing: true
            );
        }
        [HttpPost]
        [Route("generateRoute")]
        public async Task<IActionResult> GenerateRoute(RouteGenerationCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        [HttpGet]
        [Route("getFolder")]
        public async Task<IActionResult> GetFolderContent([FromQuery] GetFolderContentQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
        [HttpGet]
        [Route("has-subfolders")]
        public async Task<IActionResult> DoesFolderHasSubFolders([FromQuery] DoesFolderHasSubFoldersQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
        [HttpGet]
        [Route("get-subfolders")]
        public async Task<IActionResult> GetFolderSubFolders([FromQuery] GetFolderSubFoldersQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
        [HttpDelete]
        [Route("delete-cache")]
        public async Task<IActionResult> DeleteCache([FromQuery] DeleteCacheCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        [HttpGet]
        [Route("download-request")]
        public async Task<IActionResult> RequestDownload([FromQuery] RequestDownloadQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
        [HttpDelete]
        [Route("delete-file")]
        public async Task<IActionResult> DeleteFile([FromQuery] DeleteFileCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        [HttpDelete]
        [Route("delete-folder")]
        public async Task<IActionResult> DeleteFolder([FromQuery] DeleteFolderCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        [HttpPatch]
        [Route("rename-file")]
        public async Task<IActionResult> RenameFile(RenameFileCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        [HttpPatch]
        [Route("rename-folder")]
        public async Task<IActionResult> RenameFolder(RenameFolderCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
        [HttpPost]
        [Route("upload-file")]
        public async Task<IActionResult> UploadFile(UploadFileCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}