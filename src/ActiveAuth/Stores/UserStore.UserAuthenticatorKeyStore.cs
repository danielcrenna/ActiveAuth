// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Stores.Models;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserAuthenticatorKeyStore<TUser>
	{
		public virtual async Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			user.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			var token = await _store.QuerySingleOrDefaultByExampleAsync<AspNetUserTokens<TKey>>(
				new
				{
					TenantId = _tenantId,
					UserId = user.Id,
					Name = "AuthenticatorKey",
					LoginProvider = "[AspNetUserStore]"
				}, cancellationToken);

			if (string.IsNullOrWhiteSpace(token.Data.Value))
			{
				token.Data = new AspNetUserTokens<TKey>
				{
					TenantId = _tenantId,
					UserId = user.Id,
					LoginProvider = "[AspNetUserStore]",
					Name = "AuthenticatorKey",
					Value = key
				};
				await _store.CreateAsync(token.Data, cancellationToken);
			}
			else
			{
				await _store.UpdateByExampleAsync(token, new {UserId = user.Id, TenantId = _tenantId},
					cancellationToken);
			}
		}

		public async Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var token = await _store.QuerySingleOrDefaultByExampleAsync<AspNetUserTokens<TKey>>(
				new
				{
					TenantId = _tenantId,
					UserId = user.Id,
					Name = "AuthenticatorKey",
					LoginProvider = "[AspNetUserStore]"
				}, cancellationToken);

			return token.Data.Value;
		}
	}
}