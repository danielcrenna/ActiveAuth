// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace ActiveAuth
{
	internal sealed class Resources
	{
		/// <summary></summary>
		internal static string FormatInvalidPhoneNumber(string phoneNumber)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidPhoneNumber, phoneNumber);
		}

		/// <summary></summary>
		internal static string FormatDuplicatePhoneNumber(string phoneNumber)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicatePhoneNumber, phoneNumber);
		}

		/// <summary></summary>
		internal static string FormatInvalidTenantName(string tenantName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidTenantName, tenantName);
		}

		/// <summary></summary>
		internal static string FormatDuplicateTenantName(string tenantName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicateTenantName, tenantName);
		}

		/// <summary></summary>
		internal static string FormatInvalidApplicationName(string ApplicationName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidApplicationName, ApplicationName);
		}

		/// <summary></summary>
		internal static string FormatDuplicateApplicationName(string ApplicationName)
		{
			return string.Format(CultureInfo.CurrentCulture, ErrorStrings.DuplicateApplicationName, ApplicationName);
		}
	}
}