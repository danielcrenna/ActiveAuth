// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveAuth.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace ActiveAuth.Models
{
	public interface ITokenCredentials
	{
		SigningCredentials SigningKey { get; set; }
		EncryptingCredentials EncryptingKey { get; set; }

		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		string SigningKeyString { get; set; }

		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		string EncryptingKeyString { get; set; }
	}
}