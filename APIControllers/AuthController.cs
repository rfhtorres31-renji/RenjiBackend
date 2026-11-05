using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using renjibackend.Data;
using renjibackend.DTO;
using renjibackend.Interfaces;
using renjibackend.Models;
using renjibackend.Services;
using renjibackend.Utility;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


// ==== Updated November 3 2026 ==== // 
// Refactored by Ronnier Torres
// Redesigned using Single-Responsibility Principle
// This authentication controller focuses only on one single responsibility which handles HTTP Request/Responses (Presentation Layer)

namespace renjibackend.APIControllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

        private readonly Authentication _authService;

       public AuthController(Authentication authService)
       {
            this._authService = authService;
       }


        // For User Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {   
                    Success=true,
                    Message = "Model State is not valid",
                    Details = ModelState
                });
            }

            Response result = await _authService.RegisterUser(request, cancellationToken);

            if(!result.success)
            {
                if(result.httpResponse==409)
                {
                    return Conflict(result.message);
                }
                else if(result.httpResponse==500)
                {
                    return BadRequest(result.message);
                }
            }

            return Ok(result);
        }


        // For User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, bool rememberMe, CancellationToken cancellationToken)
        {  
           Response response = new Response();

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = true,
                    Message = "Model State is not valid",
                    Details = ModelState
                });
            }

            Response result = await _authService.LoginUser(request, rememberMe, cancellationToken);

            if (!result.success)
            {
                if (result.httpResponse == 409)
                {
                    return Conflict(result.message);
                }
                else if (result.httpResponse == 500)
                {
                    return BadRequest(result.message);
                }
                else if(result.httpResponse == 401)
                {
                    return Unauthorized(result.message);
                }
            }

            return Ok(result);

        }
    }
}
