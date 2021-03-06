﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gt.Common.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public AuthController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// login
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[AllowAnonymous]
		// POST api/auth/login
		[HttpPost("login")]
		public IActionResult Login([FromBody]TokenRequest request)
		{
			if (request.Username == "AngelaDaddy" && request.Password == "123456")
			{
				// push the user’s name into a claim, so we can identify the user later on.
				var claims = new[]
				{
				   new Claim(ClaimTypes.Name, request.Username)
			   };
				//sign the token using a secret key.This secret will be shared between your API and anything that needs to check that the token is legit.
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecurityKey"]));
				var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
				//.NET Core’s JwtSecurityToken class takes on the heavy lifting and actually creates the token.
				/**
				 * Claims (Payload)
					Claims 部分包含了一些跟这个 token 有关的重要信息。 JWT 标准规定了一些字段，下面节选一些字段:

					iss: The issuer of the token，token 是给谁的
					sub: The subject of the token，token 主题
					exp: Expiration Time。 token 过期时间，Unix 时间戳格式
					iat: Issued At。 token 创建时间， Unix 时间戳格式
					jti: JWT ID。针对当前 token 的唯一标识
					除了规定的字段外，可以包含其他任何 JSON 兼容的字段。
				 * */
				var token = new JwtSecurityToken(
					issuer: "Gt.Common.Com",
					audience: "Gt.Common.Com",
					claims: claims,
					expires: DateTime.Now.AddMinutes(3),
					signingCredentials: creds);

				return Ok(new
				{
					token = new JwtSecurityTokenHandler().WriteToken(token)
				});
			}

			return BadRequest("Could not verify username and password");
		}

		[HttpGet("user")]
		public IActionResult GetUser()
		{
			string userName = this.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name).Value;

			var result = new
			{
				user = new
				{
					UserName = userName,
					scope = new string[] { "test", "admin" }
				}
			};
			return new JsonResult(result);
		}
	}

	public class TokenRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
}