// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace ActiveAuth.Providers
{
	public interface ICookiesInfoProvider
	{
		bool Enabled { get; }
		string Scheme { get; }
	}
}