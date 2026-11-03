using Azure.Core;
using Microsoft.EntityFrameworkCore;
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
        Task<string> GetHashedPassword(string email);
        Task<int> GetUserId(string email);
        Task<string> GetEmail(string email);
        Task<string> GetFirstName(string email);
        Task<string> GetLastName(string email);

    }


    public class SQLServerUserRepository : IUserRepository
    {
        private readonly IRSDbContext _db;

        public SQLServerUserRepository(IRSDbContext db)
        {
            _db = db;
        }

        public async Task<bool> EmailExistAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<string> GetHashedPassword(string email)
        {
            return await _db.Users.Where(u => u.Email == email).Select(n => n.Password).FirstOrDefaultAsync() ?? "";
        }

        public async Task<int> GetUserId(string email)
        {
            return await _db.Users.Where(u => u.Email == email).Select(n => n.Id).FirstOrDefaultAsync();
        }

        public async Task<string> GetEmail(string email)
        {
            return await _db.Users.Where(u => u.Email == email).Select(n => n.Email).FirstOrDefaultAsync() ?? "";
        }

        public async Task<string> GetFirstName(string email)
        {
            return await _db.Users.Where(u => u.Email == email).Select(n => n.FirstName).FirstOrDefaultAsync() ?? "";
        }

        public async Task<string> GetLastName(string email)
        {
            return await _db.Users.Where(u => u.Email == email).Select(n => n.LastName).FirstOrDefaultAsync() ?? "";
        }


    }
}
