using Microsoft.AspNetCore.Identity;
using renjibackend.Interfaces;
using System.Threading.Tasks;

// Created using Single-Responsibility Principle
// This focuses only on hashing password

namespace renjibackend.Services
{
    public class PasswordHashing : IPasswordHashing
    {

        private readonly PasswordHasher<object> hasher = new PasswordHasher<object>();

        public string HashPassword(string plainedPassword)
        {

            string hashedPassword = hasher.HashPassword(null, plainedPassword);

            return hashedPassword;
        }


        public object VerifyPassword(string hashedPassword, string userPassword)
        {
            var result = hasher.VerifyHashedPassword(null, hashedPassword, userPassword);

            return result;
        }

    }
}
