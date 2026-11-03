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
    public interface IUserRepository
    {
        Task<bool> EmailExistAsync(string email);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<User> GetUserByEmailAsync(string email);
    }

}
