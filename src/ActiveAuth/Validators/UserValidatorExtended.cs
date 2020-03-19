// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Validators
{
	/// <inheritdoc />
	/// <summary>
	///     Allows extending <see cref="T:Microsoft.AspNetCore.Identity.UserValidator`1" /> to include composite email, phone
	///     number, and      username
	///     validations similar to <see cref="T:Microsoft.AspNetCore.Identity.IPasswordValidator`1" />, and provides additional
	///     options.
	/// </summary>
	public class UserValidatorExtended<TUser> : UserValidator<TUser> where TUser : class
	{
		private readonly IEnumerable<IEmailValidator<TUser>> _email;
		private readonly IOptions<IdentityOptionsExtended> _options;
		private readonly IEnumerable<IPhoneNumberValidator<TUser>> _phone;
		private readonly IEnumerable<IUsernameValidator<TUser>> _username;

		public UserValidatorExtended
		(
			IEnumerable<IEmailValidator<TUser>> email,
			IEnumerable<IPhoneNumberValidator<TUser>> phone,
			IEnumerable<IUsernameValidator<TUser>> username,
			IOptions<IdentityOptionsExtended> options
		)
		{
			_email = email;
			_phone = phone;
			_username = username;
			_options = options;
		}

		public override async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
		{
			Contract.Assert(manager != null);
			Contract.Assert(user != null);

			ICollection<IdentityError> errors = new List<IdentityError>();

			await ValidateEmail(manager, user, errors);
			await ValidatePhoneNumberAsync(manager, user, errors);
			await ValidateUserName(manager, user, errors);

			if (_options.Value.User.RequireEmailPhoneNumberOrUsername)
			{
				if (string.IsNullOrWhiteSpace(await manager.GetEmailAsync(user)) &&
				    string.IsNullOrWhiteSpace(await manager.GetPhoneNumberAsync(user)) &&
				    string.IsNullOrWhiteSpace(await manager.GetUserNameAsync(user)))
				{
					errors.Add(manager.ErrorDescriber.MustHaveEmailPhoneOrUsername());
				}
			}

			return errors.Count > 0 ? IdentityResultFactory.Failed(errors) : IdentityResult.Success;
		}

		private async Task ValidateUserName(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			foreach (var validator in _username)
			{
				await validator.ValidateAsync(manager, user, errors);
			}
		}

		private async Task ValidatePhoneNumberAsync(UserManager<TUser> manager, TUser user,
			ICollection<IdentityError> errors)
		{
			foreach (var validator in _phone)
			{
				await validator.ValidateAsync(manager, user, errors);
			}
		}

		private async Task ValidateEmail(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			foreach (var validator in _email)
			{
				await validator.ValidateAsync(manager, user, errors);
			}
		}
	}
}