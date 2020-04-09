// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Models;
using ActiveAuth.Stores.Extensions;
using ActiveAuth.Stores.Internal;
using ActiveStorage;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	public partial class RoleStore<TKey, TRole> :
		IRoleStoreExtended<TRole>,
		IQueryableRoleStore<TRole>
		where TRole : IdentityRoleExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly TKey _applicationId;
		private readonly IQueryableProvider<TRole> _queryable;
		private readonly DataStore _store;
		private readonly TKey _tenantId;

		public RoleStore(
			IQueryableProvider<TRole> queryable,
			DataStore store,
			IServiceProvider serviceProvider)
		{
			serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
			serviceProvider.TryGetTenantId(out _tenantId);
			serviceProvider.TryGetApplicationId(out _applicationId);

			CancellationToken = cancellationToken;

			_queryable = queryable;
			_store = store;
		}

		public IQueryable<TRole> Roles
		{
			get
			{
				if (_queryable.IsSafe)
				{
					return _queryable.Queryable;
				}

				if (_queryable.SupportsUnsafe)
				{
					return _queryable.UnsafeQueryable;
				}

				return Task.Run(() => GetAllRolesAsync(CancellationToken), CancellationToken).Result.AsQueryable();
			}
		}

		public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			role.ApplicationId = _applicationId;
			role.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			if (role.Id == null)
			{
				var idType = typeof(TKey);
				var id = Guid.NewGuid();
				if (idType == typeof(Guid))
				{
					role.Id = (TKey) (object) id;
				}
				else if (idType == typeof(string))
				{
					role.Id = (TKey) (object) $"{id}";
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			var query = await _store.CreateAsync(role, cancellationToken);
			Debug.Assert(query.Data == ObjectCreate.Created);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			role.ConcurrencyStamp ??= $"{Guid.NewGuid()}";
			var query = await _store.UpdateByExampleAsync(role, new {role.Id, TenantId = _tenantId}, cancellationToken);

			Debug.Assert(query.Data == 1);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			role.ConcurrencyStamp ??= $"{Guid.NewGuid()}";
			var query = await _store.DeleteByExampleAsync<TRole>(new {role.Id, TenantId = _tenantId},
				cancellationToken);

			Debug.Assert(query.Data == 1);
			return IdentityResult.Success;
		}

		public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(role?.Id?.ToString());
		}

		public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(role?.Name);
		}

		public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			role.Name = roleName;
			return Task.CompletedTask;
		}

		public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(role?.NormalizedName);
		}

		public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			role.NormalizedName = normalizedName;
			return Task.CompletedTask;
		}

		public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var roles = await _store.QuerySingleOrDefaultByExampleAsync<TRole>(new {Id = roleId, TenantId = _tenantId},
				cancellationToken);
			return roles.Data;
		}

		public async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var roles = await _store.QuerySingleOrDefaultByExampleAsync<TRole>(
				new {NormalizedName = normalizedRoleName, TenantId = _tenantId}, cancellationToken);
			return roles.Data;
		}

		public void Dispose() { }

		public CancellationToken CancellationToken { get; }

		public async Task<int> GetCountAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var count = await _store.CountAsync<TRole>(cancellationToken);
			return (int) count.Data;
		}

		private async Task<IEnumerable<TRole>> GetAllRolesAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var roles = await _store.QueryByExampleAsync<TRole>(new {TenantId = _tenantId}, cancellationToken);
			return roles.Data;
		}
	}
}