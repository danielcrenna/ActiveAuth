// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Validators
{
	public interface IValidator<TUser> where TUser : class
	{
		Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors);
	}
}