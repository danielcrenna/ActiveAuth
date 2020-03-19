// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Validators
{
	/// <summary>
	///     A default validator that contains equivalent validation logic as ASP.NET Core Identity, with the exception of
	///     optionally allowing registration without a username. Any attempts to change the username after
	///     the fact should still validate using the default logic.
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	public class DefaultUsernameValidator<TUser> : IUsernameValidator<TUser> where TUser : class
	{
		private readonly IdentityErrorDescriber _describer;
		private readonly IOptions<IdentityOptionsExtended> _options;

		public DefaultUsernameValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
		{
			_describer = describer;
			_options = options;
		}

		public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			var username = await manager.GetUserNameAsync(user);

			if (!_options.Value.User.RequireUsername && string.IsNullOrWhiteSpace(username))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(username) || ContainsDeniedUserNameCharacters(manager, username))
			{
				errors.Add(_describer.InvalidUserName(username));
				return;
			}

			var exists = await manager.FindByNameAsync(username);
			if (exists == null)
			{
				return;
			}

			if (!_options.Value.User.RequireUniqueUsername)
			{
				return;
			}

			if (!string.Equals(await manager.GetUserIdAsync(exists), await manager.GetUserIdAsync(user)))
			{
				errors.Add(_describer.DuplicateUserName(username));
			}
		}

		private static bool ContainsDeniedUserNameCharacters(UserManager<TUser> manager, string userName)
		{
			return !string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
			       userName.Any(x => !manager.Options.User.AllowedUserNameCharacters.Contains(x));
		}
	}
}