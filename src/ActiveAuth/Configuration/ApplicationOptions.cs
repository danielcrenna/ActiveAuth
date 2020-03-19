// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace ActiveAuth.Configuration
{
	public class ApplicationOptions
	{
		public string AllowedApplicationNameCharacters { get; set; } =
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
	}
}