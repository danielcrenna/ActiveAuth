// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public interface IUserPhoneNumberStoreExtended<TUser> :
		IUserStoreExtended<TUser>,
		IUserPhoneNumberStore<TUser> where TUser : class
	{
		Task<TUser> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
		Task<IEnumerable<TUser>> FindAllByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
	}
}