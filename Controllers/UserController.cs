using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace VerifyEmailForgotPasswordTutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        public UserController(DataContext context)
        {
                _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
          if (_context.Users.Any(u=>u.Email==request.Email))
            {
               return BadRequest("User already exists.");
            }

            CreatePasswordHash(request.Password, out byte[] PassworHash, out byte[] PasswordSalt);

            var user = new User
            {
                Email = request.Email,
                PasswordSalt = PasswordSalt,
                PasswordHash = PassworHash,
                VerificationToken=CreateRandomToken()

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User successfully created!");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.Email==request.Email);

            if (user == null)
            {
                return BadRequest("User not found!");
            }

            if (!VerifyPassword(request.Password,user.PasswordHash,user.PasswordSalt))
            {
                return BadRequest("Password's incorrect");
            }

            if (user.VerifiedAt==null)
            {
                return BadRequest("Not verified!");
            }

            return Ok($"Welcome back, {user.Email}! :)");
        }




        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user ==null)
            { 
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok($"User verified");
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);  

            await _context.SaveChangesAsync();

            return Ok($"You may now reset password.");
        }


        [HttpPost("Reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken==request.Token);

            if (user == null || user.ResetTokenExpires<DateTime.Now)
            {
                return BadRequest("Invalid Token!");
            }

            CreatePasswordHash(request.Password, out byte[] PasswordHash, out byte[] PasswordSalt);
            user.PasswordHash= PasswordHash;
            user.PasswordSalt= PasswordSalt;


            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();

            return Ok($"Password successfully changed :)");
        }



        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac=new HMACSHA256(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return StructuralComparisons.StructuralEqualityComparer.Equals(passwordHash, computedHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private void CreatePasswordHash(string password, out byte[] passworHash, out byte[] passwordSalt)
        {
            using (var hmac= new HMACSHA256())
            {
                passwordSalt = hmac.Key;
                passworHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
               
            }
        }
    }
}
