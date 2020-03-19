// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Validators
{
	/// <summary>
	///     A default validator that contains equivalent validation logic as ASP.NET Core Identity, with the exception of
	///     optionally allowing registration without an email address. Any attempts to change the email address after
	///     the fact should still validate using the default logic.
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	public class DefaultEmailValidator<TUser> : IEmailValidator<TUser> where TUser : class
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly EmailAddressAttribute EmailAddressAttribute = new EmailAddressAttribute();

		private readonly IdentityErrorDescriber _describer;
		private readonly IOptions<IdentityOptionsExtended> _options;

		public DefaultEmailValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
		{
			_describer = describer;
			_options = options;
		}

		public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			var email = await manager.GetEmailAsync(user);

			if (!_options.Value.User.RequireEmail && string.IsNullOrWhiteSpace(email))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(email) || !EmailAddressAttribute.IsValid(email))
			{
				errors.Add(_describer.InvalidEmail(email));
				return;
			}

			var exists = await manager.FindByEmailAsync(email);
			if (exists == null)
			{
				return;
			}

			if (manager.Options.User.RequireUniqueEmail && !string.Equals(await manager.GetUserIdAsync(exists),
				await manager.GetUserIdAsync(user)))
			{
				errors.Add(_describer.DuplicateEmail(email));
			}
		}
	}
}