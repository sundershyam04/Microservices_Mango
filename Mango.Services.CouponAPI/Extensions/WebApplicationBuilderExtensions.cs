using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mango.Services.CouponAPI.Extensions
{
	/// <summary>
	/// Extension method to add authentication set-up to builder(WebApplicationBuilder Type object)
	/// this WebApplicationBuilder builder => Extended type
	/// </summary>
	public static class WebApplicationBuilderExtensions	{
		public static WebApplicationBuilder WithAuthentication(this WebApplicationBuilder builder)
		{
			var secret = builder.Configuration["ApiSettings:Secret"];
			var issuer = builder.Configuration["ApiSettings:Issuer"];
			var audience = builder.Configuration["ApiSettings:Audience"];

			var key = Encoding.ASCII.GetBytes(secret);

			// auth options configure
			builder.Services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x =>
			{ // Parameters to be matched with JwtToken contents inorder to validate auth CHECK TOKEN DATA WITH Authdata set here --> validate
				x.TokenValidationParameters = new()
				{
					//set values to param ( to be matched with incoming Jwttoken)
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidIssuer = issuer,
					ValidAudience = audience,
					//validate
					ValidateIssuerSigningKey = true, // enables validation
					ValidateAudience = true,
					ValidateIssuer = true
				};
			});
			return builder;
		}
	}
}
