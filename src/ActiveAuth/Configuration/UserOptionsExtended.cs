// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Configuration
{
	public class UserOptionsExtended : UserOptions
	{
		public UserOptionsExtended()
		{
		}

		public UserOptionsExtended(UserOptions inner)
		{
			AllowedUserNameCharacters = inner.AllowedUserNameCharacters;
			RequireUniqueEmail = inner.RequireUniqueEmail;
		}

		public bool RequireUniqueUsername { get; set; } = true;
		public bool RequireUniquePhoneNumber { get; set; } = false;
		public bool RequireEmail { get; set; } = true;
		public bool RequirePhoneNumber { get; set; } = false;
		public bool RequireUsername { get; set; } = true;
		public bool RequireEmailPhoneNumberOrUsername { get; set; } = false;

		public string AllowedPhoneNumberCharacters { get; set; } = "()123456789-+#";
		public bool LockoutEnabled { get; set; } = true;
	}
}