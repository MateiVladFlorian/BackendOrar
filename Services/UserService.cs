using BackendOrar.Data;
using BackendOrar.Definitions;
using BackendOrar.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
#pragma warning disable

namespace BackendOrar.Services
{
    public class UserService : IUserService
    {
        readonly IAppSettings appSettings;
        readonly ICryptoService cryptoService;

        readonly OrarContext orarContext;
        readonly IJwtTokenService jwtTokenService;

        readonly IAdminService adminService;
        readonly IJwtSettings jwtSettings;

        public UserService(OrarContext orarContext, IAppSettings appSettings, 
            IJwtSettings jwtSettings, ICryptoService cryptoService, 
            IJwtTokenService jwtTokenService, IAdminService adminService)
        {
            this.orarContext = orarContext;
            this.appSettings = appSettings;
            this.jwtSettings = jwtSettings;

            this.cryptoService = cryptoService;
            this.jwtTokenService = jwtTokenService;
            this.adminService = adminService;
        }

        public async Task<User?> GetAccountAsync(int id)
        {
            /* get the user account data */
            User? account = await orarContext.User
                .FirstOrDefaultAsync(u => u.Id == id);

            account.Password = cryptoService.Decrypt(account.Password);
            return account;
        }

        public async Task<bool> VerifyAccount(string address)
        {
            User? account = await orarContext.User
                .FirstOrDefaultAsync(e => e.Address == address);

            return account != null;
        }

        public async Task<UserResponseModel> SignInAsync(UserRequestModel userRequestModel)
        {
            User? account = await orarContext.User
                .FirstOrDefaultAsync(e => e.Address == userRequestModel.address);

            var accountResponseModel = new UserResponseModel();
            string encryptedPassword = cryptoService.Encrypt(userRequestModel.password);
            accountResponseModel.UserRole = UserRole.Readonly;

            if (account != null)
            {
                if (encryptedPassword.CompareTo(account.Password) != 0)
                    accountResponseModel.status = 0;
                else
                {
                    /* account is signed in successfully */
                    accountResponseModel.status = 1;
                    accountResponseModel.fullName = account.FullName;
                    accountResponseModel.address = account.Address;
                    accountResponseModel.id = account.Id;

                    /* Create a copy of the authentication claims */
                    var claims = new Claim[]
                    {
                    new Claim("id", accountResponseModel.id.Value.ToString()),
                    new Claim(ClaimTypes.Name, accountResponseModel.fullName),
                    new Claim(ClaimTypes.Email, accountResponseModel.address.ToString()),
                    new Claim(ClaimTypes.Role, accountResponseModel.UserRole.ToString())
                    };

                    string accessToken = jwtTokenService.GenAccessToken(claims);
                    string refreshToken = jwtTokenService.GenRefreshToken();

                    /* add the token pair to the response model */
                    accountResponseModel.accessToken = accessToken;
                    accountResponseModel.refreshToken = refreshToken;

                    var lastTokenPair = await orarContext.TokenPair
                        .FirstOrDefaultAsync(pair => pair.UserId == account.Id);

                    /* remove the previous token pair corresponding to the account entity */
                    orarContext.TokenPair.Remove(lastTokenPair);
                    await orarContext.SaveChangesAsync();

                    var tokenPair = new TokenPair
                    {
                        UserId = account.Id,
                        CreatedAt = DateTime.UtcNow,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };

                    /* insert the token pair data into the TokenPair entity table */
                    orarContext.TokenPair.Add(tokenPair);
                    await orarContext.SaveChangesAsync();
                }

                accountResponseModel.UserRole = (UserRole)account.UserRole;
            }
            else
                accountResponseModel.status = -1;
            return accountResponseModel;
        }

        public async Task<UserResponseModel> RegisterAsync(UserRequestModel accountRequestModel)
        {
            var accountResponseModel = new UserResponseModel();
            accountResponseModel.UserRole = UserRole.Readonly;

            /* check if the entered password provides enhanced security */
            if (ValidatePasswordStrength(accountRequestModel.password) != 2)
            {
                accountResponseModel.status = -2;
                return accountResponseModel;
            }

            if (accountRequestModel.password.CompareTo(accountRequestModel.confirmPassword) != 0)
            {
                /* the passwords do not match */
                accountResponseModel.status = -1;
                return accountResponseModel;
            }

            /* require the password encryption */
            string encryptedPassword = cryptoService
                .Encrypt(accountRequestModel.password);

            string avatarPath = null;

            /* check if the username or password is taken */
            User account = await orarContext.User
                .FirstOrDefaultAsync(e => e.Address == accountRequestModel.address 
                || e.FullName == accountRequestModel.fullName);

            if (account == null)
            {
                account = new User
                {
                    FirstName = accountRequestModel.firstName,
                    LastName = accountRequestModel.lastName,
                    FullName = $"{accountRequestModel.lastName} {accountRequestModel.firstName}",
                    Password = cryptoService.Encrypt(accountRequestModel.password),
                    PhoneNumber = accountRequestModel.phoneNumber,
                    Description = accountRequestModel.description,
                    Address = accountRequestModel.address
                };

                if(accountRequestModel.userRole != null)
                    account.UserRole = accountRequestModel.userRole.Value;

                orarContext.User.Add(account);
                await orarContext.SaveChangesAsync();

                /* new account has been created */
                accountResponseModel.id = account.Id;
                accountResponseModel.fullName = account.FullName;
                accountResponseModel.address = account.Address;
                accountResponseModel.status = 1;

                /* create a copy of the authentication claims */
                var claims = new Claim[]
                {
                    new Claim("id", accountResponseModel.id.Value.ToString()),
                    new Claim(ClaimTypes.Name, accountResponseModel.fullName),
                    new Claim(ClaimTypes.Email, accountResponseModel.address.ToString()),
                    new Claim(ClaimTypes.Role, accountResponseModel.UserRole.ToString())
                };

                string accessToken = jwtTokenService.GenAccessToken(claims);
                string refreshToken = jwtTokenService.GenRefreshToken();

                /* add the token pair to the response model */
                accountResponseModel.accessToken = accessToken;
                accountResponseModel.refreshToken = refreshToken;

                var tokenPair = new TokenPair
                {
                    UserId = account.Id,
                    CreatedAt = DateTime.UtcNow,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                /* Insert the token pair data into the TokenPair entity table. */
                orarContext.TokenPair.Add(tokenPair);
                await orarContext.SaveChangesAsync();
                await SendWelcomeAsync(account.Id);
            }
            else
            {
                /* the account is already registered */
                accountResponseModel.status = 0;
            }

            /* response result model */
            return accountResponseModel;
        }

        public async Task<UserResponseModel> SendWelcomeAsync(int id)
        {
            User? account = await orarContext.User
                .FirstOrDefaultAsync(e => e.Id == id);

            var accountResponseModel = new UserResponseModel();

            if (account != null)
            {
                /* get the welcome email HTML template */
                string message = @"
                    ﻿﻿<p style='color: #272a35;'>
                        Welcome, {0}! Thank you for registering a new account on the OrarBackend platform.<br/>
                        Have a nice day!
                    </p><br/>
                    <p style='color: #4a606d;'>
                        Best Regards,<br/>
                        <small style='color: #54669f !important; margin-left: 10px;'>Orar Support</small>
                    </p>
                ";

                /* returns the status code */
                int res = await adminService.SendEmail(account.Address, "Orar support", 
                    string.Format(message, account.FullName));
                accountResponseModel.status = res < 1 ? 0 : 1;
            }
            else
            {
                // cannot find user account
                accountResponseModel.status = -1;
            }

            return accountResponseModel;
        }


        /// <summary>
        /// Validate the password strength by checking if it contains alphanumeric characters and special characters.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private int ValidatePasswordStrength(string password)
        {
            int res = (password.Length < 8 || password.Length > 20) ? -1 : 1;
            int score = 0;

            string[] checks = new string[]
            {
                @"[A-Za-z]",
                @"[@.#$!%^&*.?]",
                @"\d"
            };

            for (int i = 0; i < checks.Length; i++)
            {
                var regx = new Regex(checks[i]);

                if (regx.IsMatch(password))
                    score++;
            }

            return res * (score == 3 ? 2 : 1);
        }

        public async Task<int?> GetAccountIdFromAccessTokenAsync(string accessToken)
        {
            TokenPair? tokenPair = await orarContext.TokenPair.
                FirstOrDefaultAsync(pair => pair.AccessToken.CompareTo(accessToken) == 0);

            if (tokenPair != null)
            {
                User? account = await orarContext.User.
                    FirstOrDefaultAsync(a => a.Id == tokenPair.UserId);

                /* returns the corresponding identifier of the user account */
                if (account != null)
                    return account.Id;
            }

            return null;
        }

        public async Task<User?> GetAccountFromAccessTokenAsync(string accessToken)
        {
            TokenPair? tokenPair = await orarContext.TokenPair.
                FirstOrDefaultAsync(pair => pair.AccessToken.CompareTo(accessToken) == 0);

            if (tokenPair != null)
            {
                User? account = await orarContext.User.
                    FirstOrDefaultAsync(a => a.Id == tokenPair.UserId);

                /* returns the corresponding identifier of the user account */
                if (account != null)
                    return account;
            }

            return null;
        }

        public async Task<bool> IsAccessTokenExpired(string accessToken)
        {
            TokenPair? tokenPair = await orarContext.TokenPair.
                FirstOrDefaultAsync(pair => pair.AccessToken.CompareTo(accessToken) == 0);

            /* check if the input access token has not expired yet */
            if (tokenPair != null && (jwtSettings.ValidateLifetime != null && jwtSettings.ValidateLifetime.Value))
            {
                var currentDate = DateTime.UtcNow;
                var maxTime = TimeSpan.FromMinutes(jwtSettings.AccessTokenExpirationMinutes);
                var finalDate = tokenPair.CreatedAt.Add(maxTime);
                return finalDate < currentDate;
            }

            return false;
        }
    }
}
