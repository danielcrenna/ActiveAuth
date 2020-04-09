// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ActiveAuth.Models;
using Microsoft.IdentityModel.Tokens;

namespace ActiveAuth.Api
{
	internal static class UserProviderExtensions
	{
		public static string CreateToken<TUser, TKey>(this TUser user, TokenFabricationRequest request,
			IEnumerable<Claim> userClaims, Func<DateTimeOffset> timestamps) where TUser : IUserIdProvider<TKey> where TKey : IEquatable<TKey>
		{
			var now = timestamps();
			var expires = now.AddSeconds(request.TokenTimeToLiveSeconds);

			/*
                See: https://tools.ietf.org/html/rfc7519#section-4.1
                All claims are optional, but since our JSON conventions elide null values,
                We need to ensure any optional claims are emitted as empty strings.
            */

			// JWT.io claims:
			var sub = user.Id?.ToString() ?? string.Empty;
			var jti = $"{Guid.NewGuid()}";
			var iat = now.ToUnixTimeSeconds().ToString();
			var exp = expires.ToUnixTimeSeconds().ToString();
			var nbf = now.ToUnixTimeSeconds().ToString();
			var iss = request.TokenIssuer ?? string.Empty;
			var aud = request.TokenAudience ?? string.Empty;

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, sub, ClaimValueTypes.String),
				new Claim(JwtRegisteredClaimNames.Jti, jti, ClaimValueTypes.String),
				new Claim(JwtRegisteredClaimNames.Iat, iat, ClaimValueTypes.Integer64),
				new Claim(JwtRegisteredClaimNames.Nbf, nbf, ClaimValueTypes.Integer64),
				new Claim(JwtRegisteredClaimNames.Exp, exp, ClaimValueTypes.Integer64)
			};

			claims.AddRange(userClaims);

			var handler = new JwtSecurityTokenHandler();

			if (!request.Encrypt)
			{
				var jwt = new JwtSecurityToken(iss, aud, claims, now.UtcDateTime, expires.UtcDateTime,
					request.SigningKey);

				return handler.WriteToken(jwt);
			}

			var descriptor = new SecurityTokenDescriptor
			{
				Audience = aud,
				Issuer = iss,
				Subject = new ClaimsIdentity(claims),
				EncryptingCredentials = request.EncryptingKey
			};

			return handler.CreateEncodedJwt(descriptor);
		}
	}
}