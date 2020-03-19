// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveErrors;

namespace ActiveAuth.Models
{
	public interface IRoleService
	{
		Task<Operation<int>> GetCountAsync();
	}

	public interface IRoleService<TRole> : IRoleService
	{
		IQueryable<TRole> Roles { get; }

		Task<Operation<IEnumerable<TRole>>> GetAsync();
		Task<Operation<TRole>> CreateAsync(CreateRoleModel model);
		Task<Operation> UpdateAsync(TRole role);
		Task<Operation> DeleteAsync(string id);

		Task<Operation<TRole>> FindByIdAsync(string id);
		Task<Operation<TRole>> FindByNameAsync(string roleName);

		Task<Operation<IList<Claim>>> GetClaimsAsync(TRole role);
		Task<Operation<IList<Claim>>> GetAllRoleClaimsAsync();
		Task<Operation> AddClaimAsync(TRole role, Claim claim);
		Task<Operation> RemoveClaimAsync(TRole role, Claim claim);
	}
}