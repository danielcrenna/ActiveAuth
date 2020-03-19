// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Security
{
	internal sealed class NoLookupProtectorKeyRing : ILookupProtectorKeyRing
	{
		private const string ProtectorContextKey = "None";

		public IEnumerable<string> GetAllKeyIds()
		{
			return new[] {ProtectorContextKey};
		}

		public string CurrentKeyId => ProtectorContextKey;

		public string this[string keyId] => ProtectorContextKey;
	}
}