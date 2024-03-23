using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMqExcelCreate.Hubs;
using RabbitMqExcelCreate.Models;

namespace RabbitMqExcelCreate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly IHubContext<MyHub> hubContext;

    public FilesController(AppDbContext context,IHubContext<MyHub> hubContext)
    {
        this.context = context;
        this.hubContext = hubContext;
    }
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file,int fileId)
    {
        if (!(file.Length > 0)) return BadRequest();
        UserFile? userFile=await context.UserFiles.FirstOrDefaultAsync(x=>x.Id==fileId);
        string filePath = userFile.FileName + Path.GetExtension(file.FileName);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);
        using FileStream stream = new(path,FileMode.Create);
        await file.CopyToAsync(stream);

        userFile.CreatedDate = DateTime.Now;
        userFile.FilePath = filePath;
        userFile.FileStatus=FileStatus.Completed;
        context.UserFiles.Update(userFile);
        await context.SaveChangesAsync();


        await hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");


        return Ok();




    }
}
