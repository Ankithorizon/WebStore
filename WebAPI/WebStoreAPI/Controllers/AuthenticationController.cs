﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebStore.Context.AuthDTO;
using WebStore.Context.Authentication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

      
        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }


        // local database sign in
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var response = new Response();
            try
            {
                // check for 500
                // throw new Exception();

                // check for ModeState
                // ModelState.AddModelError("Username", "Username is Required!");
                // ModelState.AddModelError("Password", "Password is Required!");

                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByNameAsync(model.Username);
                    if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                    {
                        var userRoles = await userManager.GetRolesAsync(user);

                        var authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };

                        foreach (var userRole in userRoles)
                        {
                            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                        }

                        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                        var token = new JwtSecurityToken(
                            issuer: _configuration["JWT:ValidIssuer"],
                            audience: _configuration["JWT:ValidAudience"],
                            expires: DateTime.Now.AddHours(3),
                            // expires: DateTime.UtcNow.AddSeconds(8),
                            claims: authClaims,
                            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                            );

                        response.ResponseCode = 200;
                        response.ResponseMessage = "Login Success !";

                        return Ok(new
                        {
                            response = response,
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            userName = model.Username,
                            myRole = authClaims[2].Value
                        });
                    }
                    else
                    {
                        response.ResponseCode = 401;
                        response.ResponseMessage = "Username / Password Incorrect !";
                        return BadRequest(new
                        {
                            response = response,
                        });
                    }
                }
                else
                    return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                response.ResponseCode = 500;
                response.ResponseMessage = "Server Error !";
                return BadRequest(new
                {
                    response = response,
                });
            }
        }       

        // registration with role
        [HttpPost]
        [Route("register/{myRole}")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model, string myRole)
        {       
            Response _response = new Response();
            try
            {
                // check for 500: Exception
                // throw new Exception();

                if (ModelState.IsValid)
                {
                    var userExists = await userManager.FindByNameAsync(model.Username);
                    if (userExists != null)
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { ResponseCode = 500, ResponseMessage = "User already exists!" });

                    ApplicationUser user = new ApplicationUser()
                    {
                        Email = model.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = model.Username
                    };

                    var result = await userManager.CreateAsync(user, model.Password);
                    if (!result.Succeeded)
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { ResponseCode = 500, ResponseMessage = "User creation failed!" });

                    if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                        await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                    if (!await roleManager.RoleExistsAsync(UserRoles.Shopper))
                        await roleManager.CreateAsync(new IdentityRole(UserRoles.Shopper));
                  
                    await userManager.AddToRoleAsync(user, myRole);

                    _response.ResponseCode = 200;
                    _response.ResponseMessage = "User created successfully!";
                    return Ok(_response);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                /*
                _response.ResponseCode = 500;
                _response.ResponseMessage = "Server Error!";
                */
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { ResponseCode = 500, ResponseMessage = "Server Error!" });
            }
        }
    }
}
