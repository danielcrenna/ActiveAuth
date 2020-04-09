// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ActiveAuth.Api.Extensions;
using ActiveAuth.Configuration;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveErrors;
using ActiveOptions;
using ActiveRoutes;
using ActiveRoutes.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sodium;
using TypeKitchen;

namespace ActiveAuth.Api
{
	// FIXME: defaults for headers should come from a dynamic source

	/// <summary>
	///     A light-weight token issuer that only works against a super user.
	/// </summary>
	[DynamicController(typeof(SuperUserOptions))]
	[DisplayName("Tokens")]
	[MetaCategory("Authentication", "Manages authenticating incoming users against policies and identities, if any.")]
	[MetaDescription("Manages authentication tokens.")]
	public class SuperUserTokenController<TKey> : Controller, IDynamicComponentEnabled<SuperUserFeature>
		where TKey : IEquatable<TKey>
	{
		private readonly IIdentityClaimNameProvider _claimNameProvider;
		private readonly ITokenFabricator<TKey> _fabricator;
		private readonly IValidOptionsSnapshot<SuperUserOptions> _options;

		public SuperUserTokenController(IValidOptionsSnapshot<SuperUserOptions> options,
			ITokenFabricator<TKey> fabricator,
			IIdentityClaimNameProvider claimNameProvider)
		{
			_options = options;
			_fabricator = fabricator;
			_claimNameProvider = claimNameProvider;
		}
		
		[AllowAnonymous]
		[DynamicHttpPost("")]
		public Task<IActionResult> IssueToken([FromBody] BearerTokenRequest model,
			[FromHeader(Name = ActiveTenant.Constants.MultiTenancy.ApplicationHeader)]
			string application,
			[FromHeader(Name = ActiveTenant.Constants.MultiTenancy.TenantHeader)]
			string tenant,
			[FromHeader(Name = ActiveVersion.Constants.Versioning.VersionHeader)]
			string version)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
				out var error))
				return Task.FromResult((IActionResult) error);

			bool isSuperUser;
			var superUser = _options.Value;
			switch (model.IdentityType)
			{
				case IdentityType.Username:
					isSuperUser = superUser.Username == model.Identity;
					if (!isSuperUser)
						return this.NotFoundAsync();
					break;
				case IdentityType.Email:
					isSuperUser = superUser.Email == model.Identity;
					if (!isSuperUser)
						return this.NotFoundAsync();
					break;
				case IdentityType.PhoneNumber:
					isSuperUser = superUser.PhoneNumber == model.Identity;
					if (!isSuperUser)
						return this.NotFoundAsync();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (!Utilities.Compare(Encoding.UTF8.GetBytes(model.Password),
				Encoding.UTF8.GetBytes(_options.Value.Password)))
				return this.UnauthorizedAsync();

			var claims = new List<Claim>
			{
				new Claim(_claimNameProvider?.RoleClaim ?? ClaimTypes.Role,
					Constants.ClaimValues.SuperUser)
			};

			var user = typeof(TKey) != typeof(Guid)
				? typeof(TKey).IsNumeric()
					? new {Id = Constants.SuperUserNumberId}.QuackLike<IUserIdProvider<TKey>>()
					: new {Id = Constants.SuperUserStringId}.QuackLike<IUserIdProvider<TKey>>()
				: new {Id = Constants.SuperUserGuidId}.QuackLike<IUserIdProvider<TKey>>();

			var token = _fabricator.CreateToken(user, claims);

			var response = new BearerTokenResponse
			{
				IdentityType = model.IdentityType,
				AccessToken = token
			};

			return Task.FromResult((IActionResult) Ok(response));
		}

		[DynamicHttpPut("")]
		public IActionResult VerifyToken()
		{
			if (!_options.Value.Enabled)
				return NotFound();

			if (User.Identity == null)
				return Unauthorized(new {Message = "Super user identity not found."});

			if (User.Identity.IsAuthenticated)
				return Ok(new {Data = User.ProjectClaims()});

			return Unauthorized(new {Message = "Super user identity not authenticated."});
		}
	}
}