// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserAuthenticationTokenStore<TUser>
	{
		public Task SetTokenAsync(TUser user, string loginProvider, string name, string value,
			CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetTokenAsync(TUser user, string loginProvider, string name,
			CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}