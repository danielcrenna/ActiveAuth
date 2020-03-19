// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Extensions;
using ActiveTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sodium;

namespace ActiveAuth.Models
{
	public class TenantManager<TTenant, TUser, TKey> : IDisposable
		where TTenant : IdentityTenant<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ITenantStore<TTenant> _tenantStore;
		private readonly IUserStoreExtended<TUser> _userStore;
		private bool _disposed;

		public TenantManager(
			ITenantStore<TTenant> tenantStore,
			IUserStoreExtended<TUser> userStore,
			IEnumerable<ITenantValidator<TTenant, TKey>> tenantValidators,
			IOptions<IdentityOptionsExtended> optionsAccessor,
			ILookupNormalizer keyNormalizer,
			IServiceProvider serviceProvider,
			ILogger<TenantManager<TTenant, TUser, TKey>> logger)
		{
			_tenantStore = tenantStore;
			_userStore = userStore;
			_serviceProvider = serviceProvider;

			Logger = logger;
			Options = optionsAccessor?.Value ?? new IdentityOptionsExtended();
			KeyNormalizer = keyNormalizer;
			TenantValidators = tenantValidators?.ToList() ??
			                   Enumerable.Empty<ITenantValidator<TTenant, TKey>>().ToList();
		}

		public IdentityOptionsExtended Options { get; set; }
		public IList<ITenantValidator<TTenant, TKey>> TenantValidators { get; }
		public ILogger Logger { get; set; }
		public ILookupNormalizer KeyNormalizer { get; set; }

		protected virtual CancellationToken CancellationToken => _tenantStore.CancellationToken;

		public virtual IQueryable<TTenant> Tenants
		{
			get
			{
				if (!(_tenantStore is IQueryableTenantStore<TTenant> store))
				{
					throw new NotSupportedException(ErrorStrings.StoreNotIQueryableTenantStore);
				}

				return store.Tenants;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task<int> GetCountAsync()
		{
			ThrowIfDisposed();

			return await _tenantStore.GetCountAsync(CancellationToken);
		}

		public virtual async Task<TTenant> FindByIdAsync(string tenantId)
		{
			ThrowIfDisposed();

			return await _tenantStore.FindByIdAsync(tenantId, CancellationToken);
		}

		public virtual async Task<TTenant> FindByNameAsync(string tenantName)
		{
			ThrowIfDisposed();

			if (tenantName == null)
			{
				throw new ArgumentNullException(nameof(tenantName));
			}

			tenantName = KeyNormalizer.MaybeNormalizeName(tenantName);

			var tenant = await _tenantStore.FindByNameAsync(tenantName, CancellationToken);
			if (tenant != null || !Options.Stores.ProtectPersonalData)
			{
				return tenant;
			}

			var protector = _serviceProvider.GetService(typeof(ILookupProtector)) as ILookupProtector;
			if (!(_serviceProvider.GetService(typeof(ILookupProtectorKeyRing)) is ILookupProtectorKeyRing service) ||
			    protector == null)
			{
				return null;
			}

			foreach (var allKeyId in service.GetAllKeyIds())
			{
				tenant = await _tenantStore.FindByNameAsync(protector.Protect(allKeyId, tenantName), CancellationToken);
				if (tenant != null)
				{
					return tenant;
				}
			}

			return null;
		}

		public virtual async Task<IEnumerable<TTenant>> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();

			if (email == null)
			{
				throw new ArgumentNullException(nameof(email));
			}

			email = KeyNormalizer.MaybeNormalizeName(email);

			if (!(_userStore is IUserEmailStoreExtended<TUser> emailStore))
			{
				throw new NotSupportedException();
			}

			var users = await emailStore.FindAllByEmailAsync(email, CancellationToken);
			if (users != null || !Options.Stores.ProtectPersonalData)
			{
				if (!(users is IEnumerable<IdentityUserExtended<TKey>> ext))
				{
					throw new NotSupportedException();
				}

				var tenantIds = ext.Select(x => $"{x.TenantId}");
				return await _tenantStore.FindByIdsAsync(tenantIds, CancellationToken);
			}

			var protector = _serviceProvider.GetService(typeof(ILookupProtector)) as ILookupProtector;
			if (!(_serviceProvider.GetService(typeof(ILookupProtectorKeyRing)) is ILookupProtectorKeyRing service) ||
			    protector == null)
			{
				return null;
			}

			foreach (var allKeyId in service.GetAllKeyIds())
			{
				users = await emailStore.FindAllByEmailAsync(protector.Protect(allKeyId, email), CancellationToken);

				if (users is IEnumerable<IdentityUserExtended> ext)
				{
					var tenantIds = ext.Select(x => $"{x.TenantId}");
					var tenants = await _tenantStore.FindByIdsAsync(tenantIds, CancellationToken);
					if (tenants != null)
					{
						return tenants;
					}
				}
			}

			return null;
		}

		public virtual async Task<IEnumerable<TTenant>> FindByPhoneNumberAsync(string phoneNumber)
		{
			ThrowIfDisposed();

			if (phoneNumber == null)
			{
				throw new ArgumentNullException(nameof(phoneNumber));
			}

			if (!(_userStore is IUserPhoneNumberStoreExtended<TUser> phoneStore))
			{
				throw new NotSupportedException();
			}

			var users = await phoneStore.FindAllByPhoneNumberAsync(phoneNumber, CancellationToken);
			if (users != null || !Options.Stores.ProtectPersonalData)
			{
				if (!(users is IEnumerable<IdentityUserExtended<TKey>> ext))
				{
					throw new NotSupportedException();
				}

				var tenantIds = ext.Select(x => $"{x.TenantId}");
				return await _tenantStore.FindByIdsAsync(tenantIds, CancellationToken);
			}

			var protector = _serviceProvider.GetService(typeof(ILookupProtector)) as ILookupProtector;
			if (!(_serviceProvider.GetService(typeof(ILookupProtectorKeyRing)) is ILookupProtectorKeyRing service) ||
			    protector == null)
			{
				return null;
			}

			foreach (var allKeyId in service.GetAllKeyIds())
			{
				users = await phoneStore.FindAllByPhoneNumberAsync(protector.Protect(allKeyId, phoneNumber),
					CancellationToken);

				if (users is IEnumerable<IdentityUserExtended> ext)
				{
					var tenantIds = ext.Select(x => $"{x.TenantId}");
					var tenants = await _tenantStore.FindByIdsAsync(tenantIds, CancellationToken);
					if (tenants != null)
					{
						return tenants;
					}
				}
			}

			return null;
		}

		public virtual async Task<IEnumerable<TTenant>> FindByUserNameAsync(string username)
		{
			ThrowIfDisposed();

			if (username == null)
			{
				throw new ArgumentNullException(nameof(username));
			}

			username = KeyNormalizer.MaybeNormalizeName(username);

			var users = await _userStore.FindAllByNameAsync(username, CancellationToken);
			if (users != null || !Options.Stores.ProtectPersonalData)
			{
				if (!(users is IEnumerable<IdentityUserExtended> ext))
				{
					throw new NotSupportedException();
				}

				var tenantIds = ext.Select(x => $"{x.TenantId}");
				return await _tenantStore.FindByIdsAsync(tenantIds, CancellationToken);
			}

			var protector = _serviceProvider.GetService(typeof(ILookupProtector)) as ILookupProtector;
			if (!(_serviceProvider.GetService(typeof(ILookupProtectorKeyRing)) is ILookupProtectorKeyRing service) ||
			    protector == null)
			{
				return null;
			}

			foreach (var allKeyId in service.GetAllKeyIds())
			{
				users = await _userStore.FindAllByNameAsync(protector.Protect(allKeyId, username), CancellationToken);

				if (users is IEnumerable<IdentityUserExtended> ext)
				{
					var tenantIds = ext.Select(x => $"{x.TenantId}");
					var tenants = await _tenantStore.FindByIdsAsync(tenantIds, CancellationToken);
					if (tenants != null)
					{
						return tenants;
					}
				}
			}

			return null;
		}

		public virtual async Task<IdentityResult> CreateAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			await UpdateSecurityStampInternal(tenant);
			var identityResult = await ValidateTenantAsync(tenant);
			if (!identityResult.Succeeded)
			{
				return identityResult;
			}

			await UpdateNormalizedTenantNameAsync(tenant);
			return await _tenantStore.CreateAsync(tenant, CancellationToken);
		}

		public virtual Task<IdentityResult> UpdateAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			return UpdateTenantAsync(tenant);
		}

		public virtual Task<IdentityResult> DeleteAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			return _tenantStore.DeleteAsync(tenant, CancellationToken);
		}

		public virtual async Task<string> GetTenantNameAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			return await _tenantStore.GetTenantNameAsync(tenant, CancellationToken);
		}

		public virtual async Task<string> GetTenantIdAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			return await _tenantStore.GetTenantIdAsync(tenant, CancellationToken);
		}

		protected virtual async Task<IdentityResult> UpdateTenantAsync(TTenant tenant)
		{
			var identityResult = await ValidateTenantAsync(tenant);
			if (!identityResult.Succeeded)
			{
				return identityResult;
			}

			await UpdateNormalizedTenantNameAsync(tenant);
			return await _tenantStore.UpdateAsync(tenant, CancellationToken);
		}

		protected async Task<IdentityResult> ValidateTenantAsync(TTenant tenant)
		{
			if (SupportsTenantSecurityStamp)
			{
				var stamp = await GetSecurityStampAsync(tenant);
				if (stamp == null)
				{
					throw new InvalidOperationException(ErrorStrings.NullTenantSecurityStamp);
				}
			}

			var errors = new List<IdentityError>();
			foreach (var tenantValidator in TenantValidators)
			{
				var identityResult = await tenantValidator.ValidateAsync(tenant);
				if (!identityResult.Succeeded)
				{
					errors.AddRange(identityResult.Errors);
				}
			}

			if (errors.Count <= 0)
			{
				return IdentityResult.Success;
			}

			var logger = Logger;
			var eventId = (EventId) 13;
			var tenantIdAsync = await GetTenantIdAsync(tenant);
			logger?.LogWarning(eventId, "Tenant {tenantId} validation failed: {errors}.", tenantIdAsync,
				string.Join(";", errors.Select(e => e.Code)));
			return IdentityResult.Failed(errors.ToArray());
		}

		public virtual async Task<IdentityResult> SetTenantNameAsync(TTenant tenant, string userName)
		{
			ThrowIfDisposed();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			await _tenantStore.SetTenantNameAsync(tenant, userName, CancellationToken);
			await UpdateSecurityStampInternal(tenant);
			return await UpdateTenantAsync(tenant);
		}

		public virtual async Task UpdateNormalizedTenantNameAsync(TTenant tenant)
		{
			var tenantName = await GetTenantNameAsync(tenant);
			var normalizedName = ProtectPersonalData(KeyNormalizer.MaybeNormalizeName(tenantName));
			await _tenantStore.SetNormalizedTenantNameAsync(tenant, normalizedName, CancellationToken);
		}

		private string ProtectPersonalData(string data)
		{
			if (!Options.Stores.ProtectPersonalData)
			{
				return data;
			}

			var service = _serviceProvider.GetService(typeof(ILookupProtector)) as ILookupProtector;
			{
				var keyRing = _serviceProvider.GetService(typeof(ILookupProtectorKeyRing)) as ILookupProtectorKeyRing;
				return service?.Protect(keyRing?.CurrentKeyId, data);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
			{
				return;
			}

			if (_tenantStore is IDisposable store)
			{
				store.Dispose();
			}

			_disposed = true;
		}

		protected void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#region Security Stamp

		public virtual bool SupportsTenantSecurityStamp
		{
			get
			{
				ThrowIfDisposed();
				return _tenantStore is ITenantSecurityStampStore<TTenant>;
			}
		}

		public virtual async Task<string> GetSecurityStampAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			var securityStore = GetSecurityStore();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			return await securityStore.GetSecurityStampAsync(tenant, CancellationToken);
		}

		public virtual async Task<IdentityResult> UpdateSecurityStampAsync(TTenant tenant)
		{
			ThrowIfDisposed();
			GetSecurityStore();
			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			await UpdateSecurityStampInternal(tenant);
			return await UpdateTenantAsync(tenant);
		}

		private ITenantSecurityStampStore<TTenant> GetSecurityStore()
		{
			if (_tenantStore is ITenantSecurityStampStore<TTenant> store)
			{
				return store;
			}

			throw new NotSupportedException(ErrorStrings.StoreNotITenantSecurityStampStore);
		}

		private async Task UpdateSecurityStampInternal(TTenant tenant)
		{
			if (!SupportsTenantSecurityStamp)
			{
				return;
			}

			await GetSecurityStore().SetSecurityStampAsync(tenant, GenerateSecurityStamp(), CancellationToken);
		}

		private static string GenerateSecurityStamp()
		{
			var data = SodiumCore.GetRandomBytes(20);
			return Utilities.BinaryToHex(data, Utilities.HexFormat.None);
		}

		#endregion
	}
}