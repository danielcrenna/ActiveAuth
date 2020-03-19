// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ActiveAuth.Events
{
	public interface ISignInHandler
	{
		Task OnSignInSuccessAsync(IList<Claim> claims);
	}
}