// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using ActiveErrors;

namespace ActiveAuth.Models
{
	public interface ISignInService<TUser, TTenant, TApplication, TKey>
		where TUser : class, IUserEmailProvider, IPhoneNumberProvider
	{
		Task<Operation<TUser>> SignInAsync(IdentityType identityType, string identity, string password,
			bool persistent);

		Task<Operation> SignOutAsync(TUser user);
	}
}