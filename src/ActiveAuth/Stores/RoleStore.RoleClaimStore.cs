// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Stores.Models;
using ActiveStorage;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Stores
{
	public partial class RoleStore<TKey, TRole> : IRoleClaimStore<TRole>
	{
		public async Task<IList<Claim>> GetClaimsAsync(TRole role,
			CancellationToken cancellationToken = new CancellationToken())
		{
			cancellationToken.ThrowIfCancellationRequested();

			var claims = await _store.QueryByExampleAsync<AspNetRoleClaims<TKey>>(
				new {ApplicationId = _applicationId, RoleId = role.Id}, cancellationToken);

			return claims.Data.Select(x => new Claim(x.ClaimType, x.ClaimValue)).AsList();
		}

		public async Task AddClaimAsync(TRole role, Claim claim,
			CancellationToken cancellationToken = new CancellationToken())
		{
			cancellationToken.ThrowIfCancellationRequested();

			role.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			var query = await _store.CreateAsync(
				new AspNetRoleClaims<TKey>
				{
					ApplicationId = _applicationId,
					RoleId = role.Id,
					ClaimType = claim.Type,
					ClaimValue = claim.Value
				}, cancellationToken);

			Debug.Assert(query.Data == ObjectSave.Created);
		}

		public async Task RemoveClaimAsync(TRole role, Claim claim,
			CancellationToken cancellationToken = new CancellationToken())
		{
			cancellationToken.ThrowIfCancellationRequested();

			var deleted = await _store.DeleteByExampleAsync<AspNetRoleClaims<TKey>>(
				new
				{
					ApplicationId = _applicationId,
					RoleId = role.Id,
					ClaimType = claim.Type,
					ClaimValue = claim.Value
				}, cancellationToken);

			Debug.Assert(deleted.Data == 1);
		}

		public async Task<IList<Claim>> GetAllRoleClaimsAsync(
			CancellationToken cancellationToken = new CancellationToken())
		{
			cancellationToken.ThrowIfCancellationRequested();

			var claims =
				await _store.QueryByExampleAsync<AspNetRoleClaims<TKey>>(new {ApplicationId = _applicationId},
					cancellationToken);

			return claims.Data.Select(x => new Claim(x.ClaimType, x.ClaimValue)).AsList();
		}
	}
}