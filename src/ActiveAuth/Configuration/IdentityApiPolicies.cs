// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveRoutes;

namespace ActiveAuth.Configuration
{
	public class IdentityApiPolicies
	{
		public ManageUsersPolicy Users { get; set; } = new ManageUsersPolicy();
		public ManageRolesPolicy Roles { get; set; } = new ManageRolesPolicy();
		public ManageApplicationsPolicy Applications { get; set; } = new ManageApplicationsPolicy();
		public ManageTenantsPolicy Tenants { get; set; } = new ManageTenantsPolicy();

		public class ManageUsersPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; }
			public string Policy { get; set; }
		}

		public class ManageRolesPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; }
			public string Policy { get; set; }
		}

		public class ManageApplicationsPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; }
			public string Policy { get; set; }
		}

		public class ManageTenantsPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; }
			public string Policy { get; set; }
		}
	}
}