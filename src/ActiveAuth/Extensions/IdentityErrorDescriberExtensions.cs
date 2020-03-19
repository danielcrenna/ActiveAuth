// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Extensions
{
	public static class IdentityErrorDescriberExtensions
	{
		public static IdentityError InvalidPhoneNumber(this IdentityErrorDescriber describer, string phoneNumber)
		{
			return new IdentityError
			{
				Code = nameof(InvalidPhoneNumber), Description = Resources.FormatInvalidPhoneNumber(phoneNumber)
			};
		}

		public static IdentityError DuplicatePhoneNumber(this IdentityErrorDescriber describer, string phoneNumber)
		{
			return new IdentityError
			{
				Code = nameof(DuplicatePhoneNumber), Description = Resources.FormatDuplicatePhoneNumber(phoneNumber)
			};
		}

		public static IdentityError MustHaveEmailPhoneOrUsername(this IdentityErrorDescriber describer)
		{
			return new IdentityError
			{
				Code = nameof(MustHaveEmailPhoneOrUsername), Description = ErrorStrings.MustHaveEmailPhoneOrUsername
			};
		}

		public static IdentityError InvalidTenantName(this IdentityErrorDescriber describer, string tenantName)
		{
			return new IdentityError
			{
				Code = nameof(InvalidTenantName), Description = Resources.FormatInvalidTenantName(tenantName)
			};
		}

		public static IdentityError DuplicateTenantName(this IdentityErrorDescriber describer, string tenantName)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateTenantName), Description = Resources.FormatDuplicateTenantName(tenantName)
			};
		}

		public static IdentityError InvalidApplicationName(this IdentityErrorDescriber describer,
			string applicationName)
		{
			return new IdentityError
			{
				Code = nameof(InvalidApplicationName),
				Description = Resources.FormatInvalidApplicationName(applicationName)
			};
		}

		public static IdentityError DuplicateApplicationName(this IdentityErrorDescriber describer,
			string applicationName)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateApplicationName),
				Description = Resources.FormatDuplicateApplicationName(applicationName)
			};
		}
	}
}