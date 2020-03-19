// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ActiveAuth.Models;
using ActiveTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveAuth.Stores
{
	public static class Add
	{
		public static IdentityBuilder AddIdentityStores<TKey, TUser, TRole, TTenant, TApplication>
		(
			this IdentityBuilder identityBuilder
		)
			where TKey : IEquatable<TKey>
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
		{
			var services = identityBuilder.Services;

			services.AddTransient<IUserStoreExtended<TUser>, UserStore<TUser, TKey, TRole>>();
			services.AddTransient<IUserStore<TUser>>(r => r.GetRequiredService<IUserStoreExtended<TUser>>());

			services.AddTransient<IRoleStoreExtended<TRole>, RoleStore<TKey, TRole>>();
			services.AddTransient<IRoleStore<TRole>>(r => r.GetRequiredService<IRoleStoreExtended<TRole>>());

			services.AddTransient<ITenantStore<TTenant>, TenantStore<TTenant, TKey>>();
			services.AddScoped<TenantManager<TTenant, TUser, TKey>>();

			services.AddTransient<IApplicationStore<TApplication>, ApplicationStore<TApplication, TKey>>();
			services.AddScoped<ApplicationManager<TApplication, TUser, TKey>>();

			return identityBuilder
				.AddRoles<TRole>()
				.AddUserManager<UserManager<TUser>>()
				.AddRoleManager<RoleManager<TRole>>()
				.AddSignInManager<SignInManager<TUser>>();
		}
	}
}