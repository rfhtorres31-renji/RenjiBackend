
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using renjibackend.DTO;
using renjibackend.Interfaces;
using renjibackend.Models;
using renjibackend.Utility;
using System;
using System.Threading.Tasks;


// Created using Single-Responsibility Principle
// This authentication service focuses only on handling User Authentication

namespace renjibackend.Services
{
    public class Authentication
    {
        private readonly IAuthRepository _userRepo;
        private readonly ITransaction _userTransaction;
        private readonly IPasswordHashing _passwordService;
        private readonly ITokenGenerator _generateToken;
        

        public Authentication(IAuthRepository userRepo,
                              IPasswordHashing passwordService,
                              ITransaction _userTransaction,
                              ITokenGenerator generateToken)
        {
            this._userRepo = userRepo;
            this._passwordService = passwordService;
            this._generateToken = generateToken;
            this._userTransaction = _userTransaction;
        }


        public async Task<Response> RegisterUser(RegisterRequest request, CancellationToken cancellationToken)
        {
            Response response = new Response();

            try
            {      
                string hashedPassword = _passwordService.HashPassword(request.Password);

                // Check if Email already Exist
                bool isEmailExist = await _userRepo.EmailExistAsync(request.Email, cancellationToken);

                if (isEmailExist)
                {
                    response.success = false;
                    response.message = "Email already exist";
                    response.httpResponse = 409; // Conflict

                    return response;
                }

                // If email doesnt exist, open a DB Transaction for adding the new user
                await _userTransaction.BeginTransactionAsync(cancellationToken);

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

                await _userRepo.AddUserAsync(newUser, cancellationToken);
                await _userTransaction.SaveChangesAsync(cancellationToken);

                await _userTransaction.CommitTransactionAsync(cancellationToken);

                response.success = true;
                response.message = "User Added Successfully";
                response.httpResponse = 201; // User Created/Added
            }
            catch(Exception err)
            {   
                await _userTransaction.RollbackTransactionAsync(cancellationToken);

                response.success = false;
                response.message = "Internal Server Error";
                response.details = err.Message;
                response.httpResponse = 500;

                return response;
            }

            return response;
        }


        public async Task<Response> LoginUser(LoginRequest request, bool rememberMe, CancellationToken cancellationToken)
        {

            Response response = new Response();

            try
            {
                // Check if User exist
                bool isUserExist = await _userRepo.EmailExistAsync(request.Email, cancellationToken);

                if (!isUserExist)
                {
                    response.success = false;
                    response.message = "Conflict";
                    response.details = "User doesn't exist";
                    response.httpResponse = 401; // Conflict

                    return response;
                }

                User user = await _userRepo.GetUserByEmailAsync(request.Email, cancellationToken);

                PasswordVerificationResult passwordMatchObj = (PasswordVerificationResult)_passwordService.VerifyPassword(user.Password, request.Password);


                if (passwordMatchObj == PasswordVerificationResult.Success)
                {
                    string fullname = user.FirstName + ' ' + user.LastName;

                    var token = _generateToken.GenerateToken(user.Id.ToString(), user.Email, fullname, rememberMe);

                    response.success = true;
                    response.message = "Success";
                    response.details = new { token = token, name = fullname, userID = user.Id.ToString() };
                    response.httpResponse = 200;

                    return response;
                }
                else
                {
                    response.success = false;
                    response.message = "Error";
                    response.details = "Invalid User Email/Password";
                    response.httpResponse = 409;

                    return response;
                }
            }
            catch (Exception err)
            {
                response.success = false;
                response.message = "Internal Server Error";
                response.details = err.Message;
                response.httpResponse = 500;

                return response;
            }

        }



   }
}
