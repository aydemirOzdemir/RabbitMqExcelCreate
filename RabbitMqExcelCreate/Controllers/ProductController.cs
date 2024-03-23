using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMqExcelCreate.Models;
using RabbitMqExcelCreate.Services;

namespace RabbitMqExcelCreate.Controllers;
[Authorize]
public class ProductController : Controller
{
    private readonly AppDbContext context;
    private readonly UserManager<IdentityUser> userManager;
    private readonly RabbitMQPublisher rabbitMQPublisher;

    public ProductController(AppDbContext context,UserManager<IdentityUser> userManager,RabbitMQPublisher rabbitMQPublisher)
    {
        this.context = context;
        this.userManager = userManager;
        this.rabbitMQPublisher = rabbitMQPublisher;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> CreateProductExcel()
    {
        var user = await userManager.FindByNameAsync(User.Identity.Name);
        var fileName=$"product-excel-{Guid.NewGuid().ToString().Substring(1,10)}";
        UserFile userFile = new() { FileName=fileName,UserId=user.Id,FileStatus=FileStatus.Creating };
      await  context.UserFiles.AddAsync(userFile);
        await context.SaveChangesAsync();

        rabbitMQPublisher.Publish(new Shared.CreateExcelMessage() { FileId=userFile.Id,UserId=user.Id});



        TempData["StartCreatingExcel"] = true;
        return RedirectToAction(nameof(Files));
    }
    public async Task<IActionResult> Files()
    {
        var user = await userManager.FindByNameAsync(User.Identity.Name);
        return View(await context.UserFiles.Where(x=>x.UserId==user.Id).ToListAsync() );
    }
}
