// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Models;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserPhoneNumberStoreExtended<TUser>
	{
		public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.PhoneNumber = phoneNumber;
			return Task.CompletedTask;
		}

		public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user?.PhoneNumber);
		}

		public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user.PhoneNumberConfirmed);
		}

		public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.PhoneNumberConfirmed = confirmed;
			return Task.CompletedTask;
		}

		public async Task<TUser> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser && _lookupNormalizer.MaybeNormalizeName(phoneNumber) ==
				_lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.PhoneNumber))
			{
				return CreateSuperUserInstance();
			}

			var user = await _store.QuerySingleOrDefaultByExampleAsync<TUser>(
				new {PhoneNumber = phoneNumber, TenantId = _tenantId}, cancellationToken);
			return user.Data;
		}

		public async Task<IEnumerable<TUser>> FindAllByPhoneNumberAsync(string phoneNumber,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser && _lookupNormalizer.MaybeNormalizeName(phoneNumber) ==
				_lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.PhoneNumber))
			{
				return new[] {CreateSuperUserInstance()};
			}

			var users = await _store.QueryByExampleAsync<TUser>(new {PhoneNumber = phoneNumber}, cancellationToken);
			return users.Data;
		}
	}
}