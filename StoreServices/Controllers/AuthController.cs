using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Model.Requests;

namespace store_manager_backend;

[ApiController, Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost]
    public void Login([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] LoginRequest data)
    {
        
    
        /*[HttpPost("GetAuthCode")]
        [Authorize(Roles = "AuthService")]
        public string GetAuthCode([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] AuthCodeRequest data)*/
    }
}