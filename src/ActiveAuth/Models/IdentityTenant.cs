// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ActiveTenant;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public class IdentityTenant : IdentityTenant<string>
	{
	}

	public class IdentityTenant<TKey> : ITenant<TKey>
	{
		[ProtectedPersonalData] public virtual string NormalizedName { get; set; }

		public virtual string SecurityStamp { get; set; }

		public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
		[PersonalData] public virtual TKey Id { get; set; }

		[ProtectedPersonalData] public virtual string Name { get; set; }
	}
}