// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public interface IRoleStoreExtended<TRole> : IRoleStore<TRole> where TRole : class
	{
		CancellationToken CancellationToken { get; }
		Task<int> GetCountAsync(CancellationToken cancellationToken);
		Task<IList<Claim>> GetAllRoleClaimsAsync(CancellationToken cancellationToken = default);
	}
}