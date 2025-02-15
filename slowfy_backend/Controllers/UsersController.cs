using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using slowfy_backend.Data;
using slowfy_backend.Models;
using Microsoft.AspNetCore.Authorization;
using slowfy_backend.Services;

namespace slowfy_backend.Controllers
{
    public class UsersController : Controller
    {
        private readonly slowfy_backendContext _context;
        private IUsersService _usersService;

        public UsersController(slowfy_backendContext context, IUsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return Json(await _context.User.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Email,Password,Name,AvatarSrc")] User user)
        {
            if (ModelState.IsValid)
            {
                if (await _usersService.EmailExists(user.Email)) return BadRequest("Email already exists");
                
                var rUser = await _usersService.Create(user);
                return Json(rUser);
            }
            return BadRequest("bad request");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var user = await _usersService.VerifyCredential(email, password);
                var token = _usersService.CreateToken(user);
                return Json(token);
            }
            catch (Exception e)
            {
                return BadRequest("bad request");
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetMeEmail()
        {
            return Json(User?.FindFirstValue(ClaimTypes.Email));
        }
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetProfile()
        {
            var email = User?.FindFirstValue(ClaimTypes.Email);
            return Json(await _context.User.FirstOrDefaultAsync(p => p.Email == email));
        }
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetMyName()
        {
            var email = User?.FindFirstValue(ClaimTypes.Email);
            User? user = await _context.User.FirstOrDefaultAsync(p => p.Email == email);
            return user != null ? Json(user.Name) : BadRequest();
        }

        [HttpGet]
        [Authorize]
        public ActionResult CheckToken() => Ok("Ok");


        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetProfilePhoto()
        {
            var email = User?.FindFirstValue(ClaimTypes.Email);
            User? user = await _context.User.FirstOrDefaultAsync(p => p.Email == email);
            return user != null ? Json(user.AvatarSrc) : BadRequest();
        }
        
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SetProfilePhoto(string src)
        {
            var email = User?.FindFirstValue(ClaimTypes.Email);
            User? user = await _context.User.FirstOrDefaultAsync(p => p.Email == email);
            if (user == null) return BadRequest();
            user.AvatarSrc = src;
            var result = await _context.SaveChangesAsync();
            return Json(result);
        }
    }
}
