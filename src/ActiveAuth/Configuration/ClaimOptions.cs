// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace ActiveAuth.Configuration
{
	public class ClaimOptions
	{
		public string TenantIdClaim { get; set; } = ActiveTenant.Constants.Claims.TenantId;
		public string TenantNameClaim { get; set; } = ActiveTenant.Constants.Claims.TenantName;
		public string ApplicationIdClaim { get; set; } = Constants.Claims.ApplicationId;
		public string ApplicationNameClaim { get; set; } = Constants.Claims.ApplicationName;

		public string UserIdClaim { get; set; } = Constants.Claims.UserId;
		public string UserNameClaim { get; set; } = Constants.Claims.UserName;
		public string RoleClaim { get; set; } = Constants.Claims.Role;
		public string EmailClaim { get; set; } = Constants.Claims.Email;
		public string PermissionClaim { get; set; } = Constants.Claims.Permission;
	}
}