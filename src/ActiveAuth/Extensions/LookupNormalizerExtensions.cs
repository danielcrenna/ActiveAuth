// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Extensions
{
	public static class LookupNormalizerExtensions
	{
		public static string MaybeNormalizeName(this ILookupNormalizer normalizer, string lookup)
		{
			return normalizer != null ? normalizer.NormalizeName(lookup) : lookup;
		}
	}
}