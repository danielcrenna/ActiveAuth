// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Tokens;

namespace ActiveAuth.Models
{
	public class TokenFabricationRequest : ITokenCredentials
	{
		public int TokenTimeToLiveSeconds { get; set; }
		public string TokenIssuer { get; set; }
		public string TokenAudience { get; set; }
		public bool Encrypt { get; set; }

		public string SigningKeyString { get; set; } = Constants.Tokens.NoSigningKeySet;
		public string EncryptingKeyString { get; set; } = Constants.Tokens.NoEncryptingKeySet;

		public SigningCredentials SigningKey { get; set; }
		public EncryptingCredentials EncryptingKey { get; set; }
	}
}