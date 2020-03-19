// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace ActiveAuth.Providers
{
	public interface ITenantIdProvider<TKey> where TKey : IEquatable<TKey>
	{
		TKey Id { get; }
	}
}