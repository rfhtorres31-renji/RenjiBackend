using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using renjibackend.Data;
using renjibackend.DTO;
using renjibackend.Services;
using renjibackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using renjibackend.Utility;


namespace renjibackend.APIControllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

       private readonly IRSDbContext db;
       private readonly PasswordHashing passwordService;
       private Response response = new Response();
       private readonly TokenGenerator generateToken;

       public AuthController(IRSDbContext _db,
                             PasswordHashing _passwordService,
                             TokenGenerator _generateToken)
       {
            this.db = _db;
            this.passwordService = _passwordService;
            this.generateToken = _generateToken;
       }
      

       // For User Registration
       [HttpPost("register")]
       public async Task<IActionResult> Register([FromBody] RegisterRequest request)
       {   
            try
            {
                if (!ModelState.IsValid)
                {
                    response.success = false;
                    response.message = "Reqister Request Format is Invalid";
                    response.details = ModelState;
                    return BadRequest(response);
                }

                string hashedPassword = passwordService.HashPassword(request.Password);
                Debug.WriteLine(hashedPassword);

                // Check if Email already Exist
                bool isEmailExist = await db.Users.AnyAsync(u => u.Email == request.Email); 

                if (isEmailExist)
                {
                    response.success = false;
                    response.message = "Email already exist";
                    return Conflict(response); // Exit the function if Email already exist
                }


                var newUser = new User
                {
                    FirstName = request.Firstname,
                    LastName = request.Lastname,
                    Email = request.Email,
                    DepartmentId = request.DepartmentId,
                    Password = hashedPassword,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                response.success = true;
                response.message = "User Registered Successfully";

                return Ok(response);
            }

            catch (Exception err)
            {
                response.success = false;
                response.message = "Internal Server Error";
                response.details = err.Message;

                return StatusCode(500, response);
            }
          
        }

        // For User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, bool rememberMe)
        {
           try
           {
               if (!ModelState.IsValid)
               {
                 response.success = false;
                 response.message = "Bad Request";
                 response.details = ModelState;
                 return BadRequest(response);
               }

                // Check if User exist
                bool isUserExist = await db.Users.AnyAsync(u => u.Email == request.Email);

                if (!isUserExist)
                {
                    response.success = false;
                    response.message = "Conflict";
                    response.details = "User doesn't exist";
                    return Conflict(response);
                }

                string hashedPassword = await db.Users.Where(u => u.Email == request.Email).Select(n=>n.Password).FirstOrDefaultAsync() ?? "";

                PasswordVerificationResult passwordMatchObj = (PasswordVerificationResult)passwordService.VerifyPassword(hashedPassword, request.Password);

                if (passwordMatchObj == PasswordVerificationResult.Success)
                {
                    int userID = await db.Users.Where(u => u.Email == request.Email).Select(n => n.Id).FirstOrDefaultAsync();
                    string email = await db.Users.Where(u => u.Email == request.Email).Select(n => n.Email).FirstOrDefaultAsync() ?? "";
                    string firstname = await db.Users.Where(u => u.Email == request.Email).Select(n => n.FirstName).FirstOrDefaultAsync() ?? "";
                    string lastname = await db.Users.Where(u => u.Email == request.Email).Select(n => n.LastName).FirstOrDefaultAsync() ?? "";
                    string fullname = firstname + ' ' + lastname;

                    var token = generateToken.GenerateToken(userID.ToString(), email, fullname, rememberMe);

                    response.success = true;
                    response.message = "Success";
                    response.details = new { token = token, name = fullname, userID = userID };

                    Debug.WriteLine(fullname);

                    return Ok(response);
                }
                else
                {
                    response.success = true;
                    response.message = "Error";
                    response.details = "Invalid User Email/Password";
                    return Conflict(response);
                }             
           }
           catch (Exception err)
           {
                response.success = false;
                response.message = "Internal Server Error";
                response.details = err.Message;
                return StatusCode(500, response);
            }
        }
    }
}
