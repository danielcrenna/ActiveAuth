// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public class ApplicationValidator<TApplication, TUser, TKey> : IApplicationValidator<TApplication, TUser, TKey>
		where TApplication : IdentityApplication<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		public ApplicationValidator(IdentityErrorDescriber errors = null) =>
			Describer = errors ?? new IdentityErrorDescriber();

		public IdentityErrorDescriber Describer { get; }

		public virtual async Task<IdentityResult> ValidateAsync(ApplicationManager<TApplication, TUser, TKey> manager,
			TApplication application)
		{
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}

			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			var errors = new List<IdentityError>();
			await ValidateApplicationName(manager, application, errors);
			return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
		}

		private async Task ValidateApplicationName(ApplicationManager<TApplication, TUser, TKey> manager,
			TApplication application,
			ICollection<IdentityError> errors)
		{
			var applicationName = await manager.GetApplicationNameAsync(application);
			if (string.IsNullOrWhiteSpace(applicationName))
			{
				errors.Add(Describer.InvalidApplicationName(applicationName));
			}
			else
			{
				if (!string.IsNullOrEmpty(manager.Options.Application.AllowedApplicationNameCharacters) &&
				    applicationName.Any(c => !manager.Options.Application.AllowedApplicationNameCharacters.Contains(c)))
				{
					errors.Add(Describer.InvalidApplicationName(applicationName));
				}
				else
				{
					var byNameAsync = await manager.FindByNameAsync(applicationName);
					var exists = byNameAsync != null;
					if (exists)
					{
						var id = await manager.GetApplicationIdAsync(byNameAsync);
						exists = !string.Equals(id, await manager.GetApplicationIdAsync(application));
					}

					if (!exists)
					{
						return;
					}

					errors.Add(Describer.DuplicateApplicationName(applicationName));
				}
			}
		}
	}
}