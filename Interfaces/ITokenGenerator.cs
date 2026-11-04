using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using renjibackend.Data;
using renjibackend.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace renjibackend.Interfaces
{
    public interface ITokenGenerator
    {
        string GenerateToken(string userID, string email, string name, bool rememberMe);
    }

}


