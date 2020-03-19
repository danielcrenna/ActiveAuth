// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public class IdentityRoleExtended<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
	{
		public TKey ApplicationId { get; set; }
	}

	public class IdentityRoleExtended : IdentityRoleExtended<string>
	{
	}
}