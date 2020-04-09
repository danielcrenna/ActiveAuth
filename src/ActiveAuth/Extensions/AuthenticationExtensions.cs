// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ActiveAuth.Models;
using Microsoft.IdentityModel.Tokens;
using Sodium;

namespace ActiveAuth.Extensions
{
	internal static class AuthenticationExtensions
	{
		public static void MaybeSetSecurityKeys(ITokenCredentials request)
		{
			request.SigningKey ??= request.SigningKey = BuildSigningCredentials(request);
			request.EncryptingKey ??= (request.EncryptingKey = BuildEncryptingCredentials(request));
		}

		private static SigningCredentials BuildSigningCredentials(ITokenCredentials request)
		{
			MaybeSelfCreateMissingKeyStrings(request);
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.SigningKeyString));
			return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		}

		private static EncryptingCredentials BuildEncryptingCredentials(ITokenCredentials request)
		{
			MaybeSelfCreateMissingKeyStrings(request);
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.EncryptingKeyString));
			return new EncryptingCredentials(securityKey, JwtConstants.DirectKeyUseAlg,
				SecurityAlgorithms.Aes256CbcHmacSha512);
		}

		public static bool MaybeSelfCreateMissingKeyStrings(ITokenCredentials feature)
		{
			var changed = false;

			if (feature.SigningKey == null || feature.SigningKeyString == Constants.Tokens.NoSigningKeySet)
			{
				Trace.TraceWarning("No JWT signing key found, creating temporary key.");
				feature.SigningKeyString = Encoding.UTF8.GetString(SodiumCore.GetRandomBytes(128));
				changed = true;
			}

			if (feature.EncryptingKey == null || feature.EncryptingKeyString == Constants.Tokens.NoEncryptingKeySet)
			{
				Trace.TraceWarning("No JWT encryption key found, using signing key.");
				feature.EncryptingKeyString = feature.SigningKeyString;
				changed = true;
			}

			return changed;
		}

	}
}