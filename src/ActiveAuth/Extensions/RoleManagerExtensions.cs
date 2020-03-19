// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ActiveAuth.Models;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Extensions
{
	/// <summary>
	///     Provides conventional RoleManager access to Zero extensions to the ASP.NET Identity system.
	/// </summary>
	public static class RoleManagerExtensions
	{
		public static Task<int> GetCountAsync<TRole>(this RoleManager<TRole> roleManager) where TRole : class
		{
			roleManager.ThrowIfDisposed();
			if (roleManager.GetStore() is IRoleStoreExtended<TRole> extended)
			{
				return extended.GetCountAsync(extended.CancellationToken);
			}

			return null;
		}

		public static IRoleStore<TRole> GetStore<TRole>(this RoleManager<TRole> roleManager) where TRole : class
		{
			var accessor = ReadAccessor.Create(typeof(RoleManager<TRole>));
			var roleStore = accessor[roleManager, "Store"];
			return roleStore as IRoleStore<TRole>;
		}

		private static void ThrowIfDisposed<TRole>(this RoleManager<TRole> roleManager) where TRole : class
		{
			var accessor = ReadAccessor.Create(typeof(RoleManager<TRole>));
			var disposedField = accessor[roleManager, "_disposed"];
			if (disposedField is bool disposed && disposed)
			{
				throw new ObjectDisposedException(roleManager.GetType().Name);
			}
		}
	}
}