// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using ActiveAuth.Configuration;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace ActiveAuth.Api
{
	internal sealed class DefaultTokenFabricator<TKey> : ITokenFabricator<TKey> where TKey : IEquatable<TKey>
	{
		private readonly Func<DateTimeOffset> _timestamps;
		private readonly IOptionsSnapshot<TokenOptions> _tokens;

		public DefaultTokenFabricator(Func<DateTimeOffset> timestamps, IOptionsSnapshot<TokenOptions> tokens)
		{
			_timestamps = timestamps;
			_tokens = tokens;
		}

		#region Implementation of ITokenFabricator

		public string CreateToken(IUserIdProvider<TKey> user, IEnumerable<Claim> claims = null)
		{ 
			var identity = user.QuackLike<IUserIdProvider<TKey>>();

			var request = new TokenFabricationRequest
			{
				TokenIssuer = _tokens.Value.Issuer,
				TokenAudience = _tokens.Value.Audience,
				TokenTimeToLiveSeconds = _tokens.Value.TimeToLiveSeconds,
				SigningKeyString = _tokens.Value.SigningKey,
				EncryptingKeyString = _tokens.Value.EncryptingKey
			};

			var token = identity.CreateToken<IUserIdProvider<TKey>, TKey>(request, claims, _timestamps);
			return token;
		}

		#endregion
	}
}