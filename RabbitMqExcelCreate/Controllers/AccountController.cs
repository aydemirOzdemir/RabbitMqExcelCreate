using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMqExcelCreate.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(string email,string password)
    {
        var hasUser = await userManager.FindByEmailAsync(email);
        if (hasUser == null)
        {
            return View();
        }
        var signInResult = await signInManager.PasswordSignInAsync(hasUser,password,true,false);
        if (!signInResult.Succeeded)
        {
            return View();
        }
        

        
        return RedirectToAction(nameof(HomeController.Index),nameof(HomeController));
    }
}
