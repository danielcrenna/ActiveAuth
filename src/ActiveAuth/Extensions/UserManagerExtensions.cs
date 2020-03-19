// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Extensions
{
	public static class UserManagerExtensions
	{
		public static Task<int> CountAsync<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			userManager.ThrowIfDisposed();
			if (userManager.Store() is IUserStoreExtended<TUser> extended)
			{
				return extended.GetCountAsync(extended.CancellationToken);
			}

			return null;
		}

		public static bool SupportsSuperUser<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			userManager.ThrowIfDisposed();
			return userManager.Store() is IUserStoreExtended<TUser> extended && extended.SupportsSuperUser;
		}

		public static Task<TUser> FindByPhoneNumberAsync<TUser>(this UserManager<TUser> userManager, string phoneNumber)
			where TUser : class
		{
			if (userManager.Store() is IUserPhoneNumberStoreExtended<TUser> extended)
			{
				return extended.FindByPhoneNumberAsync(phoneNumber, extended.CancellationToken);
			}

			return null;
		}

		public static IUserStore<TUser> Store<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			var accessor = ReadAccessor.Create(typeof(UserManager<TUser>));
			var userStore = accessor[userManager, "Store"];
			return userStore as IUserStore<TUser>;
		}

		public static async Task<TUser> FindByIdentityAsync<TUser>(this UserManager<TUser> userManager,
			IdentityType identityType, string identity)
			where TUser : class, IUserEmailProvider, IPhoneNumberProvider
		{
			TUser user;
			switch (identityType)
			{
				case IdentityType.Username:
					user = await userManager.FindByNameAsync(identity);
					break;
				case IdentityType.Email:
					user = await userManager.FindByEmailAsync(identity);
					if (user != null && !user.EmailConfirmed)
						return null;
					break;
				case IdentityType.PhoneNumber:
					user = await userManager.FindByPhoneNumberAsync(identity);
					if (user != null && !user.PhoneNumberConfirmed)
						return null;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return user;
		}

		private static void ThrowIfDisposed<TUser>(this UserManager<TUser> userManager) where TUser : class
		{
			var accessor = ReadAccessor.Create(typeof(UserManager<TUser>));
			var disposedField = accessor[userManager, "_disposed"];
			if (disposedField is bool disposed && disposed)
			{
				throw new ObjectDisposedException(userManager.GetType().Name);
			}
		}
	}
}