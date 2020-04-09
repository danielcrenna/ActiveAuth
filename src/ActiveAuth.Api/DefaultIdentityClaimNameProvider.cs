// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveAuth.Configuration;
using ActiveAuth.Providers;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Api
{
	internal sealed class DefaultIdentityClaimNameProvider : IIdentityClaimNameProvider
	{
		private readonly IOptionsSnapshot<ClaimOptions> _options;

		public DefaultIdentityClaimNameProvider(IOptionsSnapshot<ClaimOptions> options)
		{
			_options = options;
		}

		#region Implementation of IIdentityClaimNameProvider

		public string TenantIdClaim => _options.Value.TenantIdClaim;
		public string TenantNameClaim => _options.Value.TenantNameClaim;
		public string ApplicationIdClaim => _options.Value.ApplicationIdClaim;
		public string ApplicationNameClaim => _options.Value.ApplicationNameClaim;
		public string UserIdClaim => _options.Value.UserIdClaim;
		public string UserNameClaim => _options.Value.UserNameClaim;
		public string RoleClaim => _options.Value.RoleClaim;
		public string EmailClaim => _options.Value.EmailClaim;
		public string PermissionClaim => _options.Value.PermissionClaim;

		#endregion
	}
}