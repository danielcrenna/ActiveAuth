// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Events;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using ActiveErrors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Services
{
	public class SignInService<TUser, TKey> : ISignInService<TUser>
		where TUser : IdentityUserExtended<TKey>, IUserEmailProvider, IPhoneNumberProvider
		where TKey : IEquatable<TKey>
	{
		private readonly IEnumerable<ISignInHandler> _handlers;
		private readonly IOptionsMonitor<IdentityOptionsExtended> _identityOptions;
		private readonly SignInManager<TUser> _signInManager;
		private readonly UserManager<TUser> _userManager;

		public SignInService(UserManager<TUser> userManager, SignInManager<TUser> signInManager,
			IEnumerable<ISignInHandler> handlers, IOptionsMonitor<IdentityOptionsExtended> identityOptions)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_identityOptions = identityOptions;
			_handlers = handlers;
		}

		public async Task<Operation<TUser>> SignInAsync(IdentityType identityType, string identity, string password)
		{
			TUser user = default;
			try
			{
				user = await _userManager.FindByIdentityAsync(identityType, identity);
				if (user == null)
					return IdentityResultExtensions.NotFound<TUser>();

				var result = await _signInManager.CheckPasswordSignInAsync(user, password,
					_identityOptions.CurrentValue.User.LockoutEnabled);

				if (result.Succeeded)
				{
					var claims = await _userManager.GetClaimsAsync(user);
					foreach (var handler in _handlers)
						await handler.OnSignInSuccessAsync(claims);
				}

				return result.ToOperation(user);
			}
			catch (Exception ex)
			{
				var operation = IdentityResult.Failed(new IdentityError
				{
					Code = ex.GetType().Name, Description = ex.Message
				}).ToOperation(user);

				return operation;
			}
		}

		public async Task<Operation> SignOutAsync(TUser user)
		{
			try
			{
				await _signInManager.SignOutAsync();
				return Operation.CompletedWithoutErrors;
			}
			catch (Exception ex)
			{
				var operation = IdentityResult.Failed(new IdentityError
				{
					Code = ex.GetType().Name, Description = ex.Message
				}).ToOperation();

				return operation;
			}
		}
	}
}