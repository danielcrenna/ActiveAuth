// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace ActiveAuth.Stores.Models
{
	public class AspNetRoleClaims<TKey>
	{
		[Required] public TKey ApplicationId { get; set; }

		[Required] public TKey RoleId { get; set; }

		[Required] public string ClaimType { get; set; }

		[Required] public string ClaimValue { get; set; }
	}
}