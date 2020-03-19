// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Stores.Models;
using ActiveStorage;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserLoginStore<TUser>
	{
		public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var inserted = await _store.CreateAsync(
				new AspNetUserLogins<TKey>
				{
					LoginProvider = login.LoginProvider,
					ProviderKey = login.ProviderKey,
					ProviderDisplayName = login.ProviderDisplayName,
					UserId = user.Id,
					TenantId = _tenantId
				}, cancellationToken);

			Debug.Assert(inserted.Data == ObjectSave.Created);
		}

		public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var deleted = await _store.DeleteByExampleAsync<AspNetUserLogins<TKey>>(
				new {UserId = user.Id, LoginProvider = loginProvider, ProviderKey = providerKey, TenantId = _tenantId},
				cancellationToken);

			Debug.Assert(deleted.Data == 1);
		}

		public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var logins =
				await _store.QueryByExampleAsync<AspNetUserLogins<TKey>>(new {UserId = user.Id, TenantId = _tenantId},
					cancellationToken);

			return logins.Data.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
				.AsList();
		}

		public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var user = await _store.QuerySingleOrDefaultByExampleAsync<AspNetUserLogins<TKey>>(
				new {LoginProvider = loginProvider, ProviderKey = providerKey, TenantId = _tenantId},
				cancellationToken);

			if (user.Data == null)
				return null;

			return await FindByIdAsync(user.Data.UserId.ToString(), CancellationToken);
		}
	}
}