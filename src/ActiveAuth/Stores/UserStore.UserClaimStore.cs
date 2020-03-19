// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Stores.Extensions;
using ActiveAuth.Stores.Models;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Stores
{
	partial class UserStore<TUser, TKey, TRole> : IUserClaimStore<TUser>
	{
		public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var claims = new List<Claim>();

			if (SupportsSuperUser && user.NormalizedUserName ==
				_lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.Username))
			{
				var superUser = CreateSuperUserInstance();
				claims.TryAddClaim(_claimNameProvider.UserIdClaim, $"{superUser.Id}");
				claims.TryAddClaim(_claimNameProvider.UserNameClaim, superUser.UserName);
				claims.TryAddClaim(_claimNameProvider.EmailClaim, superUser.Email, ClaimValueTypes.Email);
				claims.TryAddClaim(_claimNameProvider.RoleClaim, Constants.ClaimValues.SuperUser,
					ClaimValueTypes.Email);
				return claims;
			}

			var id = user.Id == null ? string.Empty : $"{user.Id}";

			if (_tenantId != null && !_tenantId.Equals(default))
			{
				claims.TryAddClaim(_claimNameProvider.TenantIdClaim, $"{_tenantId}");
			}

			if (_applicationId != null && !_applicationId.Equals(default))
			{
				claims.TryAddClaim(_claimNameProvider.ApplicationIdClaim, $"{_applicationId}");
			}

			if (!string.IsNullOrWhiteSpace(_tenantName))
			{
				claims.TryAddClaim(_claimNameProvider.TenantNameClaim, _tenantName);
			}

			if (!string.IsNullOrWhiteSpace(_applicationName))
			{
				claims.TryAddClaim(_claimNameProvider.ApplicationNameClaim, _applicationName);
			}

			claims.TryAddClaim(_claimNameProvider.UserIdClaim, id);
			claims.TryAddClaim(_claimNameProvider.UserNameClaim, user.UserName);
			claims.TryAddClaim(_claimNameProvider.EmailClaim, user.Email, ClaimValueTypes.Email);
			claims.AddRange(await GetUserClaimsAsync(user, cancellationToken));

			var roleNames = await GetRolesAsync(user, cancellationToken);

			foreach (var roleName in roleNames)
			{
				var role = await _roles.FindByNameAsync(roleName);
				if (role == null)
				{
					continue;
				}

				claims.Add(new Claim(_claimNameProvider.RoleClaim, roleName));

				var roleClaims = await _roles.GetClaimsAsync(role);
				if (!roleClaims.Any())
				{
					continue;
				}

				foreach (var claim in roleClaims)
				{
					claims.Add(claim);
				}
			}

			return claims;
		}

		public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			foreach (var claim in claims)
			{
				await _store.CreateAsync(
					new AspNetUserClaims<TKey>
					{
						TenantId = _tenantId, UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value
					}, cancellationToken);
			}
		}

		public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await _store.UpdateByExampleAsync(new {ClaimType = newClaim.Type, ClaimValue = newClaim.Value},
				new {TenantId = _tenantId, UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value},
				cancellationToken);
		}

		public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			foreach (var claim in claims)
			{
				var deleted = await _store.DeleteByExampleAsync<AspNetUserClaims<TKey>>(
					new {UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value, TenantId = _tenantId},
					cancellationToken);

				Debug.Assert(deleted.Data == 1);
			}
		}

		public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var users = await _store.QueryByExampleAsync<TUser>(
				new {TenantId = _tenantId, ClaimType = claim.Type, ClaimValue = claim.Value}, cancellationToken);

			return users.Data.AsList();
		}

		private async Task<IList<Claim>> GetUserClaimsAsync(TUser user, CancellationToken cancellationToken)
		{
			var claims =
				await _store.QueryByExampleAsync<AspNetUserClaims<TKey>>(new {UserId = user.Id, TenantId = _tenantId},
					cancellationToken);
			return claims.Data.Select(x => new Claim(x.ClaimType, x.ClaimValue)).AsList();
		}
	}
}