// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Stores.Models;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserTwoFactorRecoveryCodeStore<TUser>
	{
		public async Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await _store.UpdateByExampleAsync(new {Value = string.Join(";", recoveryCodes)},
				new
				{
					UserId = user.Id,
					LoginProvider = "[AspNetUserStore]",
					Name = "RecoveryCodes",
					TenantId = _tenantId
				}, cancellationToken);
		}

		public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var value = await _store.QuerySingleOrDefaultByExampleAsync<string>(
				new {UserId = user.Id, TenantId = _tenantId}, cancellationToken);

			if (string.IsNullOrWhiteSpace(value.Data))
				return false;

			var recoveryCodes = value.Data.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
			return recoveryCodes.Contains(code);
		}

		public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var token = await _store.QuerySingleOrDefaultByExampleAsync<AspNetUserTokens<TKey>>(
				new
				{
					UserId = user.Id,
					Name = "RecoveryCodes",
					LoginProvider = "[AspNetUserStore]",
					TenantId = _tenantId
				}, cancellationToken);

			if (string.IsNullOrWhiteSpace(token.Data.Value))
				return 0;

			var recoveryCodes = token.Data.Value.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
			return recoveryCodes.Length;
		}
	}
}