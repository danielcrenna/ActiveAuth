// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public interface IApplicationValidator<TApplication, TUser, TKey>
		where TApplication : IdentityApplication<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		Task<IdentityResult> ValidateAsync(ApplicationManager<TApplication, TUser, TKey> manager,
			TApplication application);
	}
}