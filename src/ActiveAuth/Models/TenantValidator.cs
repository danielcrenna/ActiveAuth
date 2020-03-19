// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveTenant;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public class TenantValidator<TTenant, TUser, TKey> : ITenantValidator<TTenant, TKey>
		where TTenant : IdentityTenant<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly TenantManager<TTenant, TUser, TKey> _manager;

		public TenantValidator(IdentityErrorDescriber errors = null) =>
			Describer = errors ?? new IdentityErrorDescriber();

		public TenantValidator(TenantManager<TTenant, TUser, TKey> manager) => _manager = manager;

		public IdentityErrorDescriber Describer { get; }

		public virtual async Task<IdentityResult> ValidateAsync(TTenant tenant)
		{
			if (_manager == null)
			{
				throw new ArgumentNullException(nameof(_manager));
			}

			if (tenant == null)
			{
				throw new ArgumentNullException(nameof(tenant));
			}

			var errors = new List<IdentityError>();
			await ValidateTenantNameAsync(_manager, tenant, errors);
			return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
		}

		private async Task ValidateTenantNameAsync(TenantManager<TTenant, TUser, TKey> manager, TTenant tenant,
			ICollection<IdentityError> errors)
		{
			var tenantName = await manager.GetTenantNameAsync(tenant);
			if (string.IsNullOrWhiteSpace(tenantName))
			{
				errors.Add(Describer.InvalidTenantName(tenantName));
			}
			else
			{
				if (!string.IsNullOrEmpty(manager.Options.Tenant.AllowedTenantNameCharacters) &&
				    tenantName.Any(c => !manager.Options.Tenant.AllowedTenantNameCharacters.Contains(c)))
				{
					errors.Add(Describer.InvalidTenantName(tenantName));
				}
				else
				{
					var byNameAsync = await manager.FindByNameAsync(tenantName);
					var exists = byNameAsync != null;
					if (exists)
					{
						var id = await manager.GetTenantIdAsync(byNameAsync);
						exists = !string.Equals(id, await manager.GetTenantIdAsync(tenant));
					}

					if (!exists)
					{
						return;
					}

					errors.Add(Describer.DuplicateTenantName(tenantName));
				}
			}
		}
	}
}