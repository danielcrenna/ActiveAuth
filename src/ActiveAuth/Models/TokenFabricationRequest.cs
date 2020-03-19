// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Tokens;

namespace ActiveAuth.Models
{
	public class TokenFabricationRequest
	{
		public int TimeToLiveSeconds { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public bool Encrypt { get; set; }
		public SigningCredentials SigningKeyString { get; set; }
		public EncryptingCredentials EncryptionKeyString { get; set; }
	}
}