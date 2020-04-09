// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Stores.Models;
using ActiveStorage;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserRoleStore<TUser>
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly List<string> SuperUserRoles = new List<string> {Constants.ClaimValues.SuperUser};

		public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var roleId = await GetRoleIdByNameAsync(roleName);

			if (roleId != null)
			{
				var inserted = await _store.CreateAsync(
					new AspNetUserRoles<TKey> {UserId = user.Id, RoleId = roleId, TenantId = _tenantId},
					cancellationToken);

				Debug.Assert(inserted.Data == ObjectCreate.Created);
			}
		}

		public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var roleId = await GetRoleIdByNameAsync(roleName);

			if (roleId != null)
			{
				// FIXME: Can't assume the underlying store supports DELETE WHERE (because CosmosDB no longer does...)
				// FIXME: This is no longer on the correct layer of abstraction!

				var toDelete = await _store.QueryByExampleAsync<AspNetUserRoles<TKey>>(
					new {UserId = user.Id, RoleId = roleId, TenantId = _tenantId}, cancellationToken);

				foreach (var role in toDelete.Data)
				{
					var deleted =
						await _store.DeleteByExampleAsync<AspNetUserRoles<TKey>>(new {role.id}, cancellationToken);
					Debug.Assert(deleted.Data == 1);
				}
			}
		}

		public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (user.NormalizedUserName == _lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.Username))
			{
				return SuperUserRoles;
			}

			var mappingQuery =
				await _store.QueryByExampleAsync<AspNetUserRoles<TKey>>(new {UserId = user.Id, TenantId = _tenantId},
					cancellationToken);

			// Mapping:
			var roleMapping = mappingQuery.Data.AsList();

			// Roles:
			if (roleMapping.Any())
			{
				var ids = roleMapping.Select(x => x.RoleId).Distinct().AsList();
				var roles = await _store.QueryByExampleAsync<TRole>(new {TenantId = _tenantId, RoleIds = ids},
					cancellationToken);

				// FIXME: legacy implementation is not transforming this to an "IN" clause, so we're effectively always retrieving all roles and have to filter
				return roles.Data.Where(x => ids.Contains(x.Id)).Select(x => x.Name).ToList();
			}

			return new List<string>(0);
		}

		public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
		{
			var userRoles = await GetRolesAsync(user, cancellationToken);

			var match = _lookupNormalizer.MaybeNormalizeName(roleName);

			foreach (var role in userRoles)
			{
				if (_lookupNormalizer.MaybeNormalizeName(role).Equals(match))
				{
					return true;
				}
			}

			return false;
		}

		public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
		{
			var users = await _store.QueryByExampleAsync<TUser>(new {NormalizedName = roleName, TenantId = _tenantId},
				cancellationToken);

			return users.Data.AsList();
		}

		private async Task<TKey> GetRoleIdByNameAsync(string roleName)
		{
			var role = await _store.QuerySingleOrDefaultByExampleAsync<TRole>(
				new {NormalizedName = _lookupNormalizer.MaybeNormalizeName(roleName), TenantId = _tenantId},
				CancellationToken);
			return role.Data.Id;
		}
	}
}