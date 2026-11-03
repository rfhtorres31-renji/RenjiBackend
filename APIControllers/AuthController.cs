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
using renjibackend.Interfaces;


namespace renjibackend.APIControllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

       private readonly IUserRepository _userRepo;
       private readonly PasswordHashing _passwordService;     
       private readonly TokenGenerator _generateToken;


       public AuthController(IUserRepository userRepo,
                             PasswordHashing passwordService,
                             TokenGenerator generateToken)
       {
            this._userRepo = userRepo;
            this._passwordService = passwordService;
            this._generateToken = generateToken;
       }
      

       // For User Registration
       [HttpPost("register")]
       public async Task<IActionResult> Register([FromBody] RegisterRequest request)
       {   
            Response response = new Response();
            try
            {   

                if (!ModelState.IsValid)
                {
                    response.success = false;
                    response.message = "Reqister Request Format is Invalid";
                    response.details = ModelState;
                    return BadRequest(response);
                }

                string hashedPassword = _passwordService.HashPassword(request.Password);

                // Check if Email already Exist
                bool isEmailExist = await _userRepo.EmailExistAsync(request.Email);

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

                await _userRepo.AddUserAsync(newUser);
                await _userRepo.SaveChangesAsync();

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
           Response response = new Response();

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
                bool isUserExist = await _userRepo.EmailExistAsync(request.Email);

                if (!isUserExist)
                {
                    response.success = false;
                    response.message = "Conflict";
                    response.details = "User doesn't exist";
                    return Conflict(response);
                }

                string hashedPassword = await _userRepo.GetHashedPassword(request.Email);

                PasswordVerificationResult passwordMatchObj = (PasswordVerificationResult)_passwordService.VerifyPassword(hashedPassword, request.Password);

                if (passwordMatchObj == PasswordVerificationResult.Success)
                {
                    int userID = await _userRepo.GetUserId(request.Email);
                    string email = await _userRepo.GetEmail(request.Email);
                    string firstname = await _userRepo.GetFirstName(request.Email);
                    string lastname = await _userRepo.GetLastName(request.Email);
                    string fullname = firstname + ' ' + lastname;

                    var token = _generateToken.GenerateToken(userID.ToString(), email, fullname, rememberMe);

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
