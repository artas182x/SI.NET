using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using demo2.Persistance;
using demo2.Persistance.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace demo2.Controllers
{
	public class AuthenticationController : Controller
	{
		private IOptions<IdentityOptions> _options;
		private PrimaryContext _context;
		private SignInManager<MyUserAccount> _signInManager;
		private UserManager<MyUserAccount> _userManager;
		private RoleManager<MyRole> _roleManager;

		public AuthenticationController(IOptions<IdentityOptions> options, UserManager<MyUserAccount> userManager,
		                                RoleManager<MyRole> roleManager, SignInManager<MyUserAccount> signInManager,
		                                PrimaryContext context)
		{
			this._options = options;
			this._signInManager = signInManager;
			this._userManager = userManager;
			this._roleManager = roleManager;
			this._context = context;
		}

		[HttpGet]
		[Route("/logowanie")]
		public async Task<IActionResult> Login([FromQuery] string returnUrl)
		{
			ViewBag.returnUrl = returnUrl;
			return await Task.Run(() => View());
		}
		
		[HttpGet]
		[Route("/nieudane-logowanie")]
		public async Task<string> LoginFailure()
		{
			return await Task.Run(() => "1234");
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/logowanie")]
		public async Task<IActionResult> Login(string username, string password, [FromForm] string returnUrl)
		{
			var result = await this._signInManager.PasswordSignInAsync(username, password, isPersistent: true,
				lockoutOnFailure: false);
			
			if (result.Succeeded)
			{
				if (returnUrl != null)
					return Redirect(returnUrl);
				
				return RedirectToAction(controllerName: "Home", actionName: "Index");
			} else if (result.IsLockedOut)
			{
				return this.RedirectToAction(controllerName: "Authentication", actionName:"LockedOut");
			}

			return await Task.Run(() => View());
		}

		public async Task<IActionResult> Logout()
		{
			await this._signInManager.SignOutAsync();
			return RedirectToAction(controllerName: "Home", actionName: "Index");
		}

		public IActionResult LockedOut()
		{
			throw new NotImplementedException();
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public string TestBearer()
		{
			return "Udało się zalogować poprzez JWT";
		}

		[Route("/token")]
		[HttpPost]
		public async Task<IActionResult> CreateToken(string username, string password)
		{
			var user = _context.Users.FirstOrDefault(u => u.UserName == username);
			if (user != null)
			{
				var signInResult =
					await this._signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
				if (signInResult.Succeeded)
				{
					return new ObjectResult(this.GenerateToken(username));
				}
			}

			return Unauthorized();
		}

		private string GenerateToken(string username)
		{
			var claims = new Claim[]
			{
				new Claim(ClaimTypes.Name, username),
				new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()
				.ToString()),
				new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1))
				.ToUnixTimeMilliseconds().ToString()), 
			};
			
			var token = new JwtSecurityToken(new JwtHeader(
				new SigningCredentials(
					new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes("1234567890123456")),
					SecurityAlgorithms.HmacSha256)
			), new JwtPayload(claims));

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}