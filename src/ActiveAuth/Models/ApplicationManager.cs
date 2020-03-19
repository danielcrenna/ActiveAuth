// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sodium;

namespace ActiveAuth.Models
{
	public class ApplicationManager<TApplication, TUser, TKey> : IDisposable
		where TApplication : IdentityApplication<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IApplicationStore<TApplication> _applicationStore;
		private readonly IServiceProvider _serviceProvider;
		private bool _disposed;

		public ApplicationManager(
			IApplicationStore<TApplication> applicationStore,
			IEnumerable<IApplicationValidator<TApplication, TUser, TKey>> applicationValidators,
			IOptions<IdentityOptionsExtended> optionsAccessor,
			ILookupNormalizer keyNormalizer,
			IServiceProvider serviceProvider,
			ILogger<ApplicationManager<TApplication, TUser, TKey>> logger)
		{
			_applicationStore = applicationStore;
			_serviceProvider = serviceProvider;

			Logger = logger;
			Options = optionsAccessor?.Value ?? new IdentityOptionsExtended();
			KeyNormalizer = keyNormalizer;
			ApplicationValidators = applicationValidators?.ToList() ??
			                        Enumerable.Empty<IApplicationValidator<TApplication, TUser, TKey>>().ToList();
		}

		public IdentityOptionsExtended Options { get; set; }
		public IList<IApplicationValidator<TApplication, TUser, TKey>> ApplicationValidators { get; }
		public ILogger Logger { get; set; }
		public ILookupNormalizer KeyNormalizer { get; set; }

		protected virtual CancellationToken CancellationToken => _applicationStore.CancellationToken;

		public virtual IQueryable<TApplication> Applications
		{
			get
			{
				if (!(_applicationStore is IQueryableApplicationStore<TApplication> store))
				{
					throw new NotSupportedException(ErrorStrings.StoreNotIQueryableApplicationStore);
				}

				return store.Applications;
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
			return await _applicationStore.GetCountAsync(CancellationToken);
		}

		public virtual async Task<TApplication> FindByIdAsync(string applicationId)
		{
			ThrowIfDisposed();
			return await _applicationStore.FindByIdAsync(applicationId, CancellationToken);
		}

		public virtual async Task<TApplication> FindByNameAsync(string applicationName)
		{
			ThrowIfDisposed();
			if (applicationName == null)
			{
				throw new ArgumentNullException(nameof(applicationName));
			}

			applicationName = KeyNormalizer.MaybeNormalizeName(applicationName);

			var application = await _applicationStore.FindByNameAsync(applicationName, CancellationToken);
			if (application != null || !Options.Stores.ProtectPersonalData)
			{
				return application;
			}

			var protector = _serviceProvider.GetService(typeof(ILookupProtector)) as ILookupProtector;
			if (!(_serviceProvider.GetService(typeof(ILookupProtectorKeyRing)) is ILookupProtectorKeyRing service) ||
			    protector == null)
			{
				return null;
			}

			foreach (var allKeyId in service.GetAllKeyIds())
			{
				application = await _applicationStore.FindByNameAsync(protector.Protect(allKeyId, applicationName),
					CancellationToken);
				if (application != null)
				{
					return application;
				}
			}

			return null;
		}

		public virtual async Task<IdentityResult> CreateAsync(TApplication application)
		{
			ThrowIfDisposed();
			await UpdateSecurityStampInternal(application);
			var identityResult = await ValidateApplicationAsync(application);
			if (!identityResult.Succeeded)
			{
				return identityResult;
			}

			await UpdateNormalizedApplicationNameAsync(application);
			return await _applicationStore.CreateAsync(application, CancellationToken);
		}

		public virtual Task<IdentityResult> UpdateAsync(TApplication application)
		{
			ThrowIfDisposed();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return UpdateApplicationAsync(application);
		}

		public virtual Task<IdentityResult> DeleteAsync(TApplication application)
		{
			ThrowIfDisposed();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return _applicationStore.DeleteAsync(application, CancellationToken);
		}

		public virtual async Task<string> GetApplicationNameAsync(TApplication application)
		{
			ThrowIfDisposed();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return await _applicationStore.GetApplicationNameAsync(application, CancellationToken);
		}

		public virtual async Task<string> GetApplicationIdAsync(TApplication application)
		{
			ThrowIfDisposed();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return await _applicationStore.GetApplicationIdAsync(application, CancellationToken);
		}

		protected virtual async Task<IdentityResult> UpdateApplicationAsync(TApplication application)
		{
			var identityResult = await ValidateApplicationAsync(application);
			if (!identityResult.Succeeded)
			{
				return identityResult;
			}

			await UpdateNormalizedApplicationNameAsync(application);
			return await _applicationStore.UpdateAsync(application, CancellationToken);
		}

		protected async Task<IdentityResult> ValidateApplicationAsync(TApplication application)
		{
			if (SupportsApplicationSecurityStamp)
			{
				var stamp = await GetSecurityStampAsync(application);
				if (stamp == null)
				{
					throw new InvalidOperationException(ErrorStrings.NullApplicationSecurityStamp);
				}
			}

			var errors = new List<IdentityError>();
			foreach (var applicationValidator in ApplicationValidators)
			{
				var identityResult = await applicationValidator.ValidateAsync(this, application);
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
			var applicationIdAsync = await GetApplicationIdAsync(application);
			logger?.LogWarning(eventId, "Application {ApplicationId} validation failed: {errors}.", applicationIdAsync,
				string.Join(";", errors.Select(e => e.Code)));
			return IdentityResult.Failed(errors.ToArray());
		}

		public virtual async Task<IdentityResult> SetApplicationNameAsync(TApplication application, string userName)
		{
			ThrowIfDisposed();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			await _applicationStore.SetApplicationNameAsync(application, userName, CancellationToken);
			await UpdateSecurityStampInternal(application);
			return await UpdateApplicationAsync(application);
		}

		public virtual async Task UpdateNormalizedApplicationNameAsync(TApplication application)
		{
			var applicationName = await GetApplicationNameAsync(application);
			;
			var normalizedName = ProtectPersonalData(KeyNormalizer.MaybeNormalizeName(applicationName));
			await _applicationStore.SetNormalizedApplicationNameAsync(application, normalizedName, CancellationToken);
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

			if (_applicationStore is IDisposable store)
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

		public virtual bool SupportsApplicationSecurityStamp
		{
			get
			{
				ThrowIfDisposed();
				return _applicationStore is IApplicationSecurityStampStore<TApplication>;
			}
		}

		public virtual async Task<string> GetSecurityStampAsync(TApplication application)
		{
			ThrowIfDisposed();
			var securityStore = GetSecurityStore();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return await securityStore.GetSecurityStampAsync(application, CancellationToken);
		}

		public virtual async Task<IdentityResult> UpdateSecurityStampAsync(TApplication application)
		{
			ThrowIfDisposed();
			GetSecurityStore();
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			await UpdateSecurityStampInternal(application);
			return await UpdateApplicationAsync(application);
		}

		private IApplicationSecurityStampStore<TApplication> GetSecurityStore()
		{
			if (_applicationStore is IApplicationSecurityStampStore<TApplication> store)
			{
				return store;
			}

			throw new NotSupportedException(ErrorStrings.StoreNotIApplicationSecurityStampStore);
		}

		private async Task UpdateSecurityStampInternal(TApplication application)
		{
			if (!SupportsApplicationSecurityStamp)
			{
				return;
			}

			await GetSecurityStore().SetSecurityStampAsync(application, GenerateSecurityStamp(), CancellationToken);
		}

		private static string GenerateSecurityStamp()
		{
			var data = SodiumCore.GetRandomBytes(20);
			return Utilities.BinaryToHex(data, Utilities.HexFormat.None);
		}

		#endregion
	}
}