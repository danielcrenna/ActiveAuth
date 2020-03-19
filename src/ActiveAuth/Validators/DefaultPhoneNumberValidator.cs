// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Validators
{
	public class DefaultPhoneNumberValidator<TUser> : IPhoneNumberValidator<TUser> where TUser : class
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly PhoneAttribute PhoneAttribute = new PhoneAttribute();

		private readonly IdentityErrorDescriber _describer;
		private readonly IOptions<IdentityOptionsExtended> _options;

		public DefaultPhoneNumberValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
		{
			_describer = describer;
			_options = options;
		}

		public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			var phoneNumber = await manager.GetPhoneNumberAsync(user);

			if (!_options.Value.User.RequirePhoneNumber && string.IsNullOrWhiteSpace(phoneNumber))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(phoneNumber) || ContainsDeniedPhoneNumberCharacters(phoneNumber) ||
			    !PhoneAttribute.IsValid(phoneNumber))
			{
				errors.Add(_describer.InvalidPhoneNumber(phoneNumber));
				return;
			}

			var exists = await manager.FindByNameAsync(phoneNumber);
			if (exists == null)
			{
				return;
			}

			if (!_options.Value.User.RequireUniquePhoneNumber)
			{
				return;
			}

			if (!string.Equals(await manager.GetUserIdAsync(exists), await manager.GetUserIdAsync(user)))
			{
				errors.Add(_describer.DuplicatePhoneNumber(phoneNumber));
			}
		}

		private bool ContainsDeniedPhoneNumberCharacters(string userName)
		{
			return !string.IsNullOrEmpty(_options.Value.User.AllowedPhoneNumberCharacters) &&
			       userName.Any(x => !_options.Value.User.AllowedPhoneNumberCharacters.Contains(x));
		}
	}
}