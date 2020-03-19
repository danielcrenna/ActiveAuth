// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;

namespace ActiveAuth.Api.Extensions
{
	internal static class ClaimsPrincipalExtensions
	{
		public static IDictionary<string, object> ProjectClaims(this ClaimsPrincipal user)
		{
			IDictionary<string, object> result = new ExpandoObject();

			if (user?.Identity == null || !user.Identity.IsAuthenticated)
			{
				return (ExpandoObject) result;
			}

			var claims = user.ClaimsList();

			foreach (var (k, v) in claims)
			{
				if (v.Count == 1)
				{
					result.Add(k, v[0]);
				}
				else
				{
					result.Add(k, v);
				}
			}

			return (ExpandoObject) result;
		}

		private static IDictionary<string, IList<string>> ClaimsList(this ClaimsPrincipal user)
		{
			IDictionary<string, IList<string>> claims = new Dictionary<string, IList<string>>();
			foreach (var claim in user?.Claims ?? Enumerable.Empty<Claim>())
			{
				if (!claims.TryGetValue(claim.Type, out var list))
				{
					claims.Add(claim.Type, list = new List<string>());
				}

				list.Add(claim.Value);
			}

			return claims;
		}
	}
}