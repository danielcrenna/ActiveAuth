// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveRoutes;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Configuration
{
	public class IdentityOptionsExtended : IdentityOptions, IFeatureToggle
	{
		public IdentityOptionsExtended() { }

		public IdentityOptionsExtended(IdentityOptions inner)
		{
			User = new UserOptionsExtended(inner.User);
			Password = new PasswordOptionsExtended(inner.Password);
			Stores = new StoreOptionsExtended(inner.Stores);

			base.User = inner.User;
			base.Password = inner.Password;
			base.Stores = inner.Stores;

			Lockout = inner.Lockout;
			Tokens = inner.Tokens;
			SignIn = inner.SignIn;
			ClaimsIdentity = inner.ClaimsIdentity;
		}

		public IdentityType? DefaultIdentityType { get; set; } = null;
		public IdentityApiOptions Api { get; set; } = new IdentityApiOptions();
		public UserOptionsExtended User { get; set; } = new UserOptionsExtended();
		public PasswordOptionsExtended Password { get; set; } = new PasswordOptionsExtended();
		public StoreOptionsExtended Stores { get; set; } = new StoreOptionsExtended();
		public TenantOptions Tenant { get; set; } = new TenantOptions();
		public ApplicationOptions Application { get; set; } = new ApplicationOptions();

		public bool Enabled { get; set; }
	}
}