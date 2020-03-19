// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace ActiveAuth.Extensions
{
	public static class ClaimsExtensions
	{
		public static void TryAddClaim(this List<Claim> claims, string type, string value,
			string typeValue = ClaimValueTypes.String)
		{
			if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(value) &&
			    !claims.Exists(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
				claims.Add(new Claim(type, value, typeValue));
		}
	}
}