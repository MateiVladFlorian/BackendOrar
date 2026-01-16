using BackendOrar.Definitions;
using BackendOrar.Models;
using BackendOrar.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
#pragma warning disable

namespace BackendOrar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("DefaultPolicy")]
    public class AccountController : ControllerBase
    {
        readonly IUserService accountService;

        public AccountController(IUserService accountService)
        { this.accountService = accountService; }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromForm] UserRequestModel accountRequestModel)
        {
            var res = await accountService.SignInAsync(accountRequestModel);

            if (res.status != null && res.status.Value < 1)
            {
                string[] messages = new string[] {
                    "User authentication has failed multiple times.",
                    "The submitted credentials do not match an existing user account.",
                    "The entered password does not match the user account."
                };

                var model = new RejectedAccountModel
                {
                    message = messages[res.status.Value + 2],
                    statusCode = res.status.Value
                };

                /* unauthorized login result with a specific message and error code */
                return Unauthorized(model);
            }
            else
            {
                var responseModel = new UserResponseModel
                {
                    id = res.id,
                    fullName = res.fullName,
                    address = res.address,
                    accessToken = res.accessToken,
                    refreshToken = res.refreshToken,
                    UserRole = res.UserRole,
                    status = res.status
                };

                /* send the login data along with the access token and refresh token */
                return Ok(responseModel);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm]UserRequestModel accountRequestModel)
        {
            var res = await accountService.RegisterAsync(accountRequestModel);

            if (res.status != null && res.status.Value <= 0)
            {
                string[] messages = new string[] {
                    "The password entered by the user provides an unsatisfactory level of security.",
                    "The user account passwords do not match.",
                    "The user account has already been registered."
                };

                var model = new RejectedAccountModel
                {
                    message = messages[res.status.Value + 2],
                    statusCode = res.status.Value
                };

                /* returns an unauthorized result with the specified message and error code */
                return Unauthorized(model);
            }
            else
            {
                var responseModel = new UserResponseModel
                {
                    id = res.id,
                    fullName = res.fullName,
                    address = res.address,
                    accessToken = res.accessToken,
                    refreshToken = res.refreshToken,
                    UserRole = res.UserRole,
                    status = res.status
                };

                /* send the email message to the client */
                await accountService.SendWelcomeAsync(res.id.Value);

                /* Returns the registered user account data along with the token pair required for the authorization process. */
                return Ok(responseModel);
            }
        }

        [HttpPost("account-checkup")]
        public async Task<IActionResult> VerifyAccount([FromBody]UserRequestModel requestModel)
        {
            /* check if the email address provided as input corresponds to an existing user account */
            bool status = await accountService.VerifyAccount(requestModel.address);
            int statusCode = status ? 1 : 0;

            string[] messages = new string[] {
                    "There is no user account corresponding to the provided email address.",
                    "The email address is associated with an existing account."
                };

            var model = new RejectedAccountModel
            {
                message = messages[statusCode],
                statusCode = statusCode
            };

            return Ok(model);
        }
    }
}
