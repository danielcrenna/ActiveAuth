// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Configuration
{
	public class StoreOptionsExtended : StoreOptions
	{
		public StoreOptionsExtended()
		{
		}

		public StoreOptionsExtended(StoreOptions inner)
		{
			MaxLengthForKeys = inner.MaxLengthForKeys;
			ProtectPersonalData = inner.ProtectPersonalData;
		}

		public bool CreateIfNotExists { get; set; } = true;
		public bool MigrateOnStartup { get; set; } = true;
	}
}