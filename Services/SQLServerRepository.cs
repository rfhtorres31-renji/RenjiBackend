using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using renjibackend.Data;
using renjibackend.Interfaces;
using renjibackend.Models;
using System;
using System.Data;
using System.Threading.Tasks;


// Created using Single-Responsibility Principle
// This focuses only on communicating on SQL Server 

namespace renjibackend.Services
{
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

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _db.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        }

    }
}
