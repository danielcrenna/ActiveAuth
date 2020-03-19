// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace ActiveAuth.Stores.Models
{
	public interface IIdentityClaimNameProvider
	{
		string TenantIdClaim { get; }
		string TenantNameClaim { get; }
		string ApplicationIdClaim { get; }
		string ApplicationNameClaim { get; }
		string UserIdClaim { get; }
		string UserNameClaim { get; }
		string RoleClaim { get; }
		string EmailClaim { get; }
		string PermissionClaim { get; }
	}
}