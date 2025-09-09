using Microsoft.AspNetCore.Identity;


namespace renjibackend.Services
{
    public class PasswordHashing
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
