// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public interface IUserStoreExtended<TUser> : IUserStore<TUser> where TUser : class
	{
		CancellationToken CancellationToken { get; }
		bool SupportsSuperUser { get; }
		Task<int> GetCountAsync(CancellationToken cancellationToken);
		Task<IEnumerable<TUser>> FindAllByNameAsync(string normalizedUserName, CancellationToken cancellationToken);
	}
}