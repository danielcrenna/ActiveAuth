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
using ActiveTenant;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Stores
{
	public class TenantStore<TTenant, TKey> :
		IQueryableTenantStore<TTenant>,
		ITenantSecurityStampStore<TTenant>
		where TTenant : IdentityTenant<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IQueryableProvider<TTenant> _queryable;
		private readonly DataStore _store;

		public TenantStore(
			DataStore store,
			IQueryableProvider<TTenant> queryable,
			IServiceProvider serviceProvider)
		{
			serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
			CancellationToken = cancellationToken;
			_store = store;
			_queryable = queryable;
		}

		public CancellationToken CancellationToken { get; }

		public IQueryable<TTenant> Tenants
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

				return Task.Run(() => GetAllTenantsAsync(CancellationToken), CancellationToken).Result.AsQueryable();
			}
		}

		public async Task<IdentityResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			tenant.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			if (tenant.Id == null)
			{
				var idType = typeof(TKey);
				var id = Guid.NewGuid();
				if (idType == typeof(Guid))
				{
					tenant.Id = (TKey) (object) id;
				}
				else if (idType == typeof(string))
				{
					tenant.Id = (TKey) (object) $"{id}";
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			var query = await _store.CreateAsync(tenant, cancellationToken);
			Debug.Assert(query.Data == ObjectSave.Created);

			return IdentityResult.Success;
		}

		public async Task<IdentityResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			tenant.ConcurrencyStamp ??= $"{Guid.NewGuid()}";

			var updated = await _store.UpdateByExampleAsync(tenant, new {tenant.Id}, cancellationToken);

			Debug.Assert(updated.Data == 1);
			return IdentityResult.Success;
		}

		public Task SetTenantNameAsync(TTenant tenant, string name, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			tenant.Name = name;
			return Task.CompletedTask;
		}

		public Task SetNormalizedTenantNameAsync(TTenant tenant, string normalizedName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			tenant.NormalizedName = normalizedName;
			return Task.CompletedTask;
		}

		public async Task<IdentityResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var deleted = await _store.DeleteByExampleAsync<TTenant>(new {tenant.Id}, cancellationToken);
			Debug.Assert(deleted.Data == 1);
			return IdentityResult.Success;
		}

		public async Task<TTenant> FindByIdAsync(string tenantId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var id = StringToId(tenantId);

			var tenant = await _store.QuerySingleOrDefaultByExampleAsync<TTenant>(new {Id = id}, cancellationToken);
			return tenant.Data;
		}

		public async Task<IEnumerable<TTenant>> FindByIdsAsync(IEnumerable<string> tenantIds,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var ids = new List<object>();
			foreach (var tenantId in tenantIds)
				ids.Add(StringToId(tenantId));

			var tenants = await _store.QueryByExampleAsync<TTenant>(new {Id = ids}, cancellationToken);
			return tenants.Data;
		}

		public async Task<TTenant> FindByNameAsync(string normalizedTenantName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var tenant =
				await _store.QuerySingleOrDefaultByExampleAsync<TTenant>(new {NormalizedName = normalizedTenantName},
					cancellationToken);
			return tenant.Data;
		}

		public Task<string> GetTenantIdAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(tenant?.Id?.ToString());
		}

		public Task<string> GetTenantNameAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(tenant?.Name);
		}

		public async Task<int> GetCountAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var count = await _store.CountAsync<TTenant>(cancellationToken);
			return (int) count.Data;
		}

		public void Dispose() { }

		public Task SetSecurityStampAsync(TTenant tenant, string stamp, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			tenant.SecurityStamp = stamp;
			return Task.CompletedTask;
		}

		public Task<string> GetSecurityStampAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(tenant?.SecurityStamp);
		}

		private static object StringToId(string tenantId)
		{
			var idType = typeof(TKey);
			object id;
			if (idType == typeof(Guid) && Guid.TryParse(tenantId, out var guid))
			{
				id = guid;
			}
			else if ((idType == typeof(short) || idType == typeof(int) || idType == typeof(long)) &&
			         long.TryParse(tenantId, out var integer))
			{
				id = integer;
			}
			else if (idType == typeof(string))
			{
				id = tenantId;
			}
			else
			{
				throw new NotSupportedException();
			}

			return id;
		}

		private async Task<IEnumerable<TTenant>> GetAllTenantsAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var tenants = await _store.QueryByExampleAsync<TTenant>(cancellationToken);
			return tenants.Data;
		}
	}
}