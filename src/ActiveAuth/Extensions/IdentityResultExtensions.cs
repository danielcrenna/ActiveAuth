// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using ActiveErrors;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Extensions
{
	public static class IdentityResultExtensions
	{
		public static Operation ToOperation(this IdentityResult result)
		{
			var errors = result.Errors.Select(x => new Error(ErrorEvents.IdentityError, $"{x.Code} - {x.Description}"));
			var operation = new Operation(errors.ToList());
			if (result.Succeeded)
			{
				operation.Result =
					operation.HasErrors ? OperationResult.SucceededWithErrors : OperationResult.Succeeded;
			}
			else
			{
				operation.Result = OperationResult.Error;
			}

			return operation;
		}

		public static Operation<T> ToOperation<T>(this IdentityResult result, T data)
		{
			var errors = result.Errors.Select(x => new Error(ErrorEvents.IdentityError, $"{x.Code} - {x.Description}"))
				.ToList();
			var operation = new Operation<T>(data, errors);
			if (result.Succeeded)
			{
				operation.Result =
					operation.HasErrors ? OperationResult.SucceededWithErrors : OperationResult.Succeeded;
			}
			else
			{
				operation.Result = OperationResult.Error;
			}

			return operation;
		}

		public static Operation<TUser> NotFound<TUser>()
		{
			return new Operation<TUser>(new Error(ErrorEvents.ResourceMissing, "User is not found."));
		}

		public static Operation<TUser> ToOperation<TUser>(this SignInResult result, TUser user)
		{
			Operation<TUser> operation;
			if (!result.Succeeded)
			{
				var errors = new List<Error>();

				if (result.IsLockedOut)
				{
					errors.Add(new Error(ErrorEvents.IdentityError, "001 - User is locked out."));
				}

				if (result.IsNotAllowed)
				{
					errors.Add(new Error(ErrorEvents.IdentityError, "002 - User is not allowed to sign in."));
				}

				if (result.RequiresTwoFactor)
				{
					errors.Add(
						new Error(ErrorEvents.IdentityError, "003 - User requires multi-factor authentication."));
				}

				operation = new Operation<TUser>(user, errors) {Result = OperationResult.Refused};
			}
			else
			{
				operation = new Operation<TUser>(user);
			}

			if (result.Succeeded)
			{
				operation.Result =
					operation.HasErrors ? OperationResult.SucceededWithErrors : OperationResult.Succeeded;
			}

			return operation;
		}
	}
}