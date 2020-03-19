// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserLockoutStore<TUser>
	{
		public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user?.LockoutEnd);
		}

		public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.LockoutEnd = lockoutEnd?.UtcDateTime;
			return Task.CompletedTask;
		}

		public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.AccessFailedCount++;
			return Task.FromResult(user.AccessFailedCount);
		}

		public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.AccessFailedCount = 0;
			return Task.CompletedTask;
		}

		public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user.AccessFailedCount);
		}

		public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user.LockoutEnabled);
		}

		public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.LockoutEnabled = enabled;
			return Task.CompletedTask;
		}
	}
}