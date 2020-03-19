// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using ActiveErrors;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Services
{
	public class RoleService<TRole, TKey> : IRoleService<TRole> where TRole :
		IdentityRoleExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly ILookupNormalizer _lookupNormalizer;
		private readonly IQueryableProvider<TRole> _queryableProvider;
		private readonly RoleManager<TRole> _roleManager;
		private readonly IRoleStoreExtended<TRole> _roleStore;

		public RoleService(RoleManager<TRole> roleManager, ILookupNormalizer lookupNormalizer,
			IRoleStoreExtended<TRole> roleStore, IQueryableProvider<TRole> queryableProvider)
		{
			_roleManager = roleManager;
			_lookupNormalizer = lookupNormalizer;
			_roleStore = roleStore;
			_queryableProvider = queryableProvider;
		}

		public IQueryable<TRole> Roles => _roleManager.Roles;

		public async Task<Operation<int>> GetCountAsync()
		{
			var result = await _roleManager.GetCountAsync();
			var operation = new Operation<int>(result);
			return operation;
		}

		public Task<Operation<IEnumerable<TRole>>> GetAsync()
		{
			var all = _queryableProvider.SafeAll ?? Roles;
			return Task.FromResult(new Operation<IEnumerable<TRole>>(all));
		}

		public async Task<Operation<TRole>> CreateAsync(CreateRoleModel model)
		{
			var role = (TRole) FormatterServices.GetUninitializedObject(typeof(TRole));
			role.Name = model.Name;
			role.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";
			role.NormalizedName = _lookupNormalizer.MaybeNormalizeName(model.Name);

			var result = await _roleManager.CreateAsync(role);
			return result.ToOperation(role);
		}

		public async Task<Operation> UpdateAsync(TRole role)
		{
			var result = await _roleManager.UpdateAsync(role);
			return result.ToOperation();
		}

		public async Task<Operation> DeleteAsync(string id)
		{
			var operation = await FindByIdAsync(id);
			if (!operation.Succeeded)
			{
				return operation;
			}

			var deleted = await _roleManager.DeleteAsync(operation.Data);
			return deleted.ToOperation();
		}

		#region Find

		public async Task<Operation<TRole>> FindByIdAsync(string id)
		{
			var role = await _roleManager.FindByIdAsync(id);
			return role == null
				? new Operation<TRole>(new Error(ErrorEvents.ResourceMissing, ErrorStrings.RoleNotFound,
					HttpStatusCode.NotFound))
				: new Operation<TRole>(role);
		}

		public async Task<Operation<TRole>> FindByNameAsync(string roleName)
		{
			var role = await _roleManager.FindByNameAsync(roleName);
			return role == null
				? new Operation<TRole>(new Error(ErrorEvents.ResourceMissing, ErrorStrings.RoleNotFound,
					HttpStatusCode.NotFound))
				: new Operation<TRole>(role);
		}

		#endregion

		#region Claims

		public async Task<Operation<IList<Claim>>> GetClaimsAsync(TRole role)
		{
			var claims = await _roleManager.GetClaimsAsync(role);
			return new Operation<IList<Claim>>(claims);
		}

		public async Task<Operation<IList<Claim>>> GetAllRoleClaimsAsync()
		{
			var claims = await _roleStore.GetAllRoleClaimsAsync();
			return new Operation<IList<Claim>>(claims);
		}

		public async Task<Operation> AddClaimAsync(TRole role, Claim claim)
		{
			var result = await _roleManager.AddClaimAsync(role, claim);
			return result.ToOperation();
		}

		public async Task<Operation> RemoveClaimAsync(TRole role, Claim claim)
		{
			var result = await _roleManager.RemoveClaimAsync(role, claim);
			return result.ToOperation();
		}

		#endregion
	}
}