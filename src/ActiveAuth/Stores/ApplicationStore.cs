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
	public class ApplicationStore<TApplication, TKey> : IQueryableApplicationStore<TApplication>,
		IApplicationSecurityStampStore<TApplication>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IQueryableProvider<TApplication> _queryable;
		private readonly DataStore _store;

		public ApplicationStore(
			DataStore store,
			IQueryableProvider<TApplication> queryable,
			IServiceProvider serviceProvider)
		{
			serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
			CancellationToken = cancellationToken;
			_store = store;
			_queryable = queryable;
		}

		public Task SetSecurityStampAsync(TApplication application, string stamp, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			application.SecurityStamp = stamp;
			return Task.CompletedTask;
		}

		public Task<string> GetSecurityStampAsync(TApplication application, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(application?.SecurityStamp);
		}

		public CancellationToken CancellationToken { get; }

		public IQueryable<TApplication> Applications
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

				return Task.Run(() => GetAllApplicationsAsync(CancellationToken), CancellationToken).Result
					.AsQueryable();
			}
		}

		public async Task<IdentityResult> CreateAsync(TApplication application, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			application.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			if (application.Id == null)
			{
				var idType = typeof(TKey);
				var id = Guid.NewGuid();
				if (idType == typeof(Guid))
				{
					application.Id = (TKey) (object) id;
				}
				else if (idType == typeof(string))
				{
					application.Id = (TKey) (object) $"{id}";
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			var inserted = await _store.CreateAsync(application, cancellationToken);
			Debug.Assert(inserted.Data == ObjectCreate.Created);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> UpdateAsync(TApplication application, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			application.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			var updated = await _store.UpdateByExampleAsync(application, new {application.Id}, cancellationToken);
			Debug.Assert(updated.Data == 1);

			return IdentityResult.Success;
		}

		public Task SetApplicationNameAsync(TApplication application, string name, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			application.Name = name;
			return Task.CompletedTask;
		}

		public Task SetNormalizedApplicationNameAsync(TApplication application, string normalizedName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			application.NormalizedName = normalizedName;
			return Task.CompletedTask;
		}

		public async Task<IdentityResult> DeleteAsync(TApplication application, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var deleted = await _store.DeleteByExampleAsync<TApplication>(new {application.Id}, cancellationToken);
			Debug.Assert(deleted.Data == 1);
			return IdentityResult.Success;
		}

		public async Task<TApplication> FindByIdAsync(string applicationId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var id = StringToId(applicationId);

			var application =
				await _store.QuerySingleOrDefaultByExampleAsync<TApplication>(new {Id = id}, cancellationToken);
			return application.Data;
		}

		public async Task<IEnumerable<TApplication>> FindByIdsAsync(IEnumerable<string> applicationIds,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var ids = new List<object>();
			foreach (var tenantId in applicationIds)
				ids.Add(StringToId(tenantId));

			var applications = await _store.QueryByExampleAsync<TApplication>(new {Id = ids}, cancellationToken);
			return applications.Data;
		}

		public async Task<TApplication> FindByNameAsync(string normalizedApplicationName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var application =
				await _store.QuerySingleOrDefaultByExampleAsync<TApplication>(
					new {NormalizedName = normalizedApplicationName}, cancellationToken);
			return application.Data;
		}

		public Task<string> GetApplicationIdAsync(TApplication application, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(application?.Id?.ToString());
		}

		public Task<string> GetApplicationNameAsync(TApplication application, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(application?.Name);
		}

		public async Task<int> GetCountAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var count = await _store.CountAsync<TApplication>(cancellationToken);
			return (int) count.Data;
		}

		public void Dispose()
		{
		}

		private static object StringToId(string applicationId)
		{
			var idType = typeof(TKey);
			object id;
			if (idType == typeof(Guid) && Guid.TryParse(applicationId, out var guid))
			{
				id = guid;
			}
			else if ((idType == typeof(short) || idType == typeof(int) || idType == typeof(long)) &&
			         long.TryParse(applicationId, out var integer))
			{
				id = integer;
			}
			else if (idType == typeof(string))
			{
				id = applicationId;
			}
			else
			{
				throw new NotSupportedException();
			}

			return id;
		}

		private async Task<IEnumerable<TApplication>> GetAllApplicationsAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var applications = await _store.QueryByExampleAsync<TApplication>(cancellationToken);
			return applications.Data;
		}
	}
}