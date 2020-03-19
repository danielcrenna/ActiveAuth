// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	public partial class UserStore<TUser, TKey, TRole> : IUserEmailStoreExtended<TUser>, IUserEmailStore<TUser>
	{
		public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			user.Email = email;
			return Task.CompletedTask;
		}

		public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user?.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.EmailConfirmed);
		}

		public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			user.EmailConfirmed = confirmed;
			return Task.CompletedTask;
		}

		public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser &&
			    normalizedEmail == _lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.Email))
			{
				return CreateSuperUserInstance();
			}

			var user = await _store.QuerySingleOrDefaultByExampleAsync<TUser>(
				new {NormalizedEmail = normalizedEmail, TenantId = _tenantId}, cancellationToken);
			return user.Data;
		}

		public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.NormalizedEmail);
		}

		public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			user.NormalizedEmail = normalizedEmail;
			return Task.FromResult<object>(null);
		}

		public async Task<IEnumerable<TUser>> FindAllByEmailAsync(string normalizedEmail,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser &&
			    normalizedEmail == _lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.Email))
			{
				return new[] {CreateSuperUserInstance()};
			}

			var users = await _store.QueryByExampleAsync<TUser>(new {NormalizedEmail = normalizedEmail},
				cancellationToken);
			return users.Data;
		}

		public async Task<int> GetCountAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var count = await _store.CountAsync<TUser>(cancellationToken);
			return (int) count.Data;
		}
	}
}