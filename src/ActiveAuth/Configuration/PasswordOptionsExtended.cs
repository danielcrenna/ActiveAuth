// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Configuration
{
	public class PasswordOptionsExtended : PasswordOptions
	{
		public PasswordOptionsExtended()
		{
		}

		public PasswordOptionsExtended(PasswordOptions inner)
		{
			RequireDigit = inner.RequireDigit;
			RequireLowercase = inner.RequireLowercase;
			RequireNonAlphanumeric = inner.RequireNonAlphanumeric;
			RequireUppercase = inner.RequireUppercase;
			RequiredLength = inner.RequiredLength;
			RequiredUniqueChars = inner.RequiredUniqueChars;
		}

		public PasswordHashStrategy HashStrategy { get; set; } = PasswordHashStrategy.Pbkdf2;
		public bool RequireUniquePassword { get; set; }
	}
}