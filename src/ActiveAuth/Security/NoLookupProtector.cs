// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Security
{
	internal sealed class NoLookupProtector : ILookupProtector
	{
		public string Protect(string keyId, string data)
		{
			return data;
		}

		public string Unprotect(string keyId, string data)
		{
			return data;
		}
	}
}