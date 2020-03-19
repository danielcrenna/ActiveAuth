// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using ActiveTenant;

namespace ActiveAuth.Models
{
	public class IdentityTenantContextStore<TTenant> : ITenantContextStore<TTenant> where TTenant : IdentityTenant
	{
		private readonly ITenantService<TTenant> _tenantService;

		public IdentityTenantContextStore(ITenantService<TTenant> tenantService) => _tenantService = tenantService;

		public async Task<ITenantContext<TTenant>> FindByKeyAsync(string tenantKey)
		{
			// FIXME!
			return null;

			var tenant = await _tenantService.FindByNameAsync(tenantKey);
			if (tenant?.Data == null)
			{
				return null;
			}

			var context = new TenantContext<TTenant> {Value = tenant.Data, Identifiers = new[] {tenant.Data.Name}};
			return context;
		}
	}
}