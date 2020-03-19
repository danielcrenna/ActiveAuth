// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveRoutes;

namespace ActiveAuth.Configuration
{
	public class IdentityApiOptions : IFeatureToggle, IFeatureNamespace
	{
		public IdentityApiPolicies Policies { get; set; } = new IdentityApiPolicies();
		public string RootPath { get; set; } = "auth";
		public bool Enabled { get; set; } = true;
	}
}