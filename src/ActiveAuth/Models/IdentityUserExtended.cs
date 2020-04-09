// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ActiveAuth.Providers;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public class IdentityUserExtended<TKey> : IdentityUser<TKey>, IUserIdProvider<TKey>, IUserNameProvider,
		IUserEmailProvider, IPhoneNumberProvider
		where TKey : IEquatable<TKey>
	{
		public TKey TenantId { get; set; }
	}

	public class IdentityUserExtended : IdentityUserExtended<string> { }
}