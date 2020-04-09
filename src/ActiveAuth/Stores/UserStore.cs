// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveAuth.Stores.Extensions;
using ActiveAuth.Stores.Internal;
using ActiveAuth.Stores.Models;
using ActiveStorage;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth.Stores
{
	public partial class UserStore<TUser, TKey, TRole> : IQueryableUserStore<TUser>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
		where TRole : IdentityRole<TKey>
	{
		private readonly TKey _applicationId;
		private readonly string _applicationName;

		private readonly IIdentityClaimNameProvider _claimNameProvider;

		private readonly ILookupNormalizer _lookupNormalizer;
		private readonly IPasswordHasher<TUser> _passwordHasher;
		private readonly IQueryableProvider<TUser> _queryable;
		private readonly RoleManager<TRole> _roles;
		private readonly DataStore _store;
		private readonly ISuperUserInfoProvider _superUserInfoProvider;

		private readonly TKey _tenantId;
		private readonly string _tenantName;

		public UserStore(
			DataStore store,
			IPasswordHasher<TUser> passwordHasher,
			RoleManager<TRole> roles,
			IQueryableProvider<TUser> queryable,
			IIdentityClaimNameProvider claimNameProvider,
			ISuperUserInfoProvider superUserInfoProvider,
			ILookupNormalizer lookupNormalizer,
			IServiceProvider serviceProvider)
		{
			serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
			serviceProvider.TryGetTenantId(out _tenantId);
			serviceProvider.TryGetTenantName(out _tenantName);
			serviceProvider.TryGetApplicationId(out _applicationId);
			serviceProvider.TryGetApplicationName(out _applicationName);

			CancellationToken = cancellationToken;

			_store = store;
			_passwordHasher = passwordHasher;
			_roles = roles;
			_queryable = queryable;

			_claimNameProvider = claimNameProvider;
			_superUserInfoProvider = superUserInfoProvider;
			_lookupNormalizer = lookupNormalizer;
		}

		public IQueryable<TUser> Users => MaybeQueryable();

		public CancellationToken CancellationToken { get; }

		public bool SupportsSuperUser => _superUserInfoProvider?.Enabled ?? false;

		public async Task<IEnumerable<TUser>> FindAllByNameAsync(string normalizedUserName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser &&
			    normalizedUserName == _lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.Username))
			{
				return new[] {CreateSuperUserInstance()};
			}

			var users = await _store.QueryByExampleAsync<TUser>(new {NormalizedUserName = normalizedUserName},
				cancellationToken);
			return users.Data;
		}

		public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			user.TenantId = _tenantId;
			user.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			if (user.Id == null)
			{
				var idType = typeof(TKey);
				var id = Guid.NewGuid();
				if (idType == typeof(Guid))
				{
					user.Id = (TKey) (object) id;
				}
				else if (idType == typeof(string))
				{
					user.Id = (TKey) (object) $"{id}";
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			var inserted = await _store.CreateAsync(user, cancellationToken);
			Debug.Assert(inserted.Data == ObjectCreate.Created);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var deletedUsers =
				await _store.DeleteByExampleAsync<TUser>(new {user.Id, TenantId = _tenantId}, cancellationToken);
			if (deletedUsers.Data == 1)
			{
				var deleteLogins =
					await _store.DeleteByExampleAsync<AspNetUserLogins<TKey>>(
						new {UserId = user.Id, TenantId = _tenantId}, cancellationToken);
				Debug.Assert(deleteLogins.Data == 1);
			}

			Debug.Assert(deletedUsers.Data == 1);
			return IdentityResult.Success;
		}

		public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser && UserIdIsSuperUserId(userId))
			{
				return CreateSuperUserInstance();
			}

			var user = await _store.QuerySingleOrDefaultByExampleAsync<TUser>(new {Id = userId, TenantId = _tenantId},
				cancellationToken);
			return user.Data;
		}

		public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SupportsSuperUser &&
			    normalizedUserName == _lookupNormalizer.MaybeNormalizeName(_superUserInfoProvider?.Username))
			{
				return CreateSuperUserInstance();
			}

			var user = await _store.QuerySingleOrDefaultByExampleAsync<TUser>(
				new {NormalizedUserName = normalizedUserName, TenantId = _tenantId}, cancellationToken);
			return user.Data;
		}

		public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user?.NormalizedUserName);
		}

		public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult($"{user.Id}");
		}

		public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(user?.UserName);
		}

		public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.NormalizedUserName = normalizedName;
			return Task.CompletedTask;
		}

		public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			user.UserName = userName;
			return Task.CompletedTask;
		}

		public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			user.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			var updated =
				await _store.UpdateByExampleAsync(user, new {user.Id, TenantId = _tenantId}, cancellationToken);
			Debug.Assert(updated.Data == 1);
			return IdentityResult.Success;
		}

		public void Dispose()
		{
		}

		private static bool UserIdIsSuperUserId(string userId)
		{
			if (typeof(TKey) == typeof(Guid))
				return Guid.TryParse(userId, out var guid) && guid == Constants.SuperUserGuidId;

			if (typeof(TKey).IsNumeric())
				return int.TryParse(userId, out var number) && number == Constants.SuperUserNumberId;

			return userId == Constants.SuperUserStringId;
		}

		private IQueryable<TUser> MaybeQueryable()
		{
			if (_queryable.IsSafe)
			{
				return _queryable.Queryable;
			}

			if (_queryable.SupportsUnsafe)
			{
				return _queryable.UnsafeQueryable;
			}

			return Task.Run(GetAllUsersAsync, CancellationToken).Result.AsQueryable();
		}

		private async Task<IEnumerable<TUser>> GetAllUsersAsync()
		{
			var users = await _store.QueryByExampleAsync<TUser>(new {TenantId = _tenantId}, CancellationToken);
			return users.Data;
		}

		private TUser CreateSuperUserInstance()
		{
			var superuser = Activator.CreateInstance<TUser>();

			if (typeof(TKey) == typeof(Guid))
			{
				superuser.Id = (TKey) (object) Constants.SuperUserGuidId;
			}
			else if (typeof(TKey) == typeof(string))
			{
				superuser.Id = (TKey) (object) Constants.SuperUserStringId;
			}
			else
			{
				superuser.Id = (TKey) (object) Constants.SuperUserNumberId;
			}

			var options = _superUserInfoProvider;

			superuser.UserName = options?.Username ?? Constants.SuperUserDefaultUserName;
			superuser.NormalizedUserName = _lookupNormalizer.MaybeNormalizeName(superuser.UserName);
			superuser.PhoneNumber =
				_lookupNormalizer.MaybeNormalizeName(options?.PhoneNumber ?? Constants.SuperUserDefaultPhoneNumber);
			superuser.PhoneNumberConfirmed = true;
			superuser.Email = options?.Email ?? Constants.SuperUserDefaultEmail;
			superuser.NormalizedEmail =
				_lookupNormalizer.MaybeNormalizeName(options?.Email ?? Constants.SuperUserDefaultEmail);
			superuser.EmailConfirmed = true;
			superuser.LockoutEnabled = false;
			superuser.TwoFactorEnabled = false;
			superuser.SecurityStamp = Constants.SuperUserSecurityStamp;
			superuser.ConcurrencyStamp = $"{Guid.NewGuid()}";
			superuser.PasswordHash = _passwordHasher.HashPassword(superuser, options?.Password);

			return superuser;
		}
	}
}