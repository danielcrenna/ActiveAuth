// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace ActiveAuth.Validators
{
	public interface IEmailValidator<TUser> : IValidator<TUser> where TUser : class
	{
	}
}