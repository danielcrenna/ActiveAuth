// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserSecurityStampStore<TUser>
	{
		public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.SecurityStamp = stamp;
			return Task.CompletedTask;
		}

		public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user?.SecurityStamp);
		}
	}
}