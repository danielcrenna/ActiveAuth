// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveAuth.Models
{
	public interface IUserEmailStoreExtended<TUser> : IUserStoreExtended<TUser> where TUser : class
	{
		Task<IEnumerable<TUser>> FindAllByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
	}
}