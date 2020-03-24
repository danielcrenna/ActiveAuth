// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using ActiveAuth.Models;

namespace ActiveAuth.Providers
{
	public interface ITokenFabricator<in TKey> where TKey : IEquatable<TKey>
	{
		string CreateToken(IUserIdProvider<TKey> user, IEnumerable<Claim> claims = null);
	}
}