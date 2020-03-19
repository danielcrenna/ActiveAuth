// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace ActiveAuth.Api
{
	public class BearerTokenRequest
	{
		[Required] public IdentityType IdentityType { get; set; } = IdentityType.Username;

		[Required] public string Identity { get; set; }

		[DataType(DataType.Password)]
		[Required]
		public string Password { get; set; }
	}
}