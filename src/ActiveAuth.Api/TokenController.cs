// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ActiveAuth.Api.Extensions;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveErrors;
using ActiveLogging;
using ActiveRoutes;
using ActiveRoutes.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ActiveAuth.Api
{
	[Route("tokens")]
	[DynamicController(typeof(TokenOptions))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Authentication", "Manages authenticating incoming users against policies and identities, if any.")]
	[DisplayName("Tokens")]
	[MetaDescription("Manages authentication tokens.")]
	public class TokenController<TUser, TTenant, TApplication, TKey> : Controller,
		IDynamicComponentEnabled<TokenApiFeature>
		where TUser : IdentityUserExtended<TKey>
		where TTenant : IdentityTenant<TKey>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly ITokenFabricator<TKey> _fabricator;
		private readonly IHttpContextAccessor _http;
		private readonly ISafeLogger<TokenController<TUser, TTenant, TApplication, TKey>> _logger;
		private readonly ISignInService<TUser> _signInService;
		private readonly Func<DateTimeOffset> _timestamps;

		public TokenController(
			Func<DateTimeOffset> timestamps,
			IHttpContextAccessor http,
			ISignInService<TUser> signInService,
			ITokenFabricator<TKey> fabricator,
			ISafeLogger<TokenController<TUser, TTenant, TApplication, TKey>> logger)
		{
			_timestamps = timestamps;
			_http = http;
			_signInService = signInService;
			_fabricator = fabricator;
			_logger = logger;
		}

		[DynamicAuthorize(typeof(TokenOptions))]
		[DynamicHttpPut]
		public IActionResult VerifyToken()
		{
			if (User.Identity == null)
			{
				_logger?.Trace(() => "User is unauthorized");

				return Unauthorized();
			}

			if (User.Identity.IsAuthenticated)
			{
				_logger?.Trace(() => "{User} verified token", User.Identity.Name);

				return Ok(new {Data = User.ProjectClaims()});
			}

			return Unauthorized();
		}

		// FIXME: defaults for headers should come from a dynamic source

		[AllowAnonymous]
		[DynamicHttpPost]
		public async Task<IActionResult> IssueToken(
			[FromBody] BearerTokenRequest model,
			[FromHeader(Name = ActiveTenant.Constants.MultiTenancy.ApplicationHeader)]
			string application,
			[FromHeader(Name = ActiveTenant.Constants.MultiTenancy.TenantHeader)]
			string tenant,
			[FromHeader(Name = ActiveVersion.Constants.Versioning.VersionHeader)]
			string version
		)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			// FIXME: pin claims transformation to user-provided scope
			var operation = await _signInService.SignInAsync(model.IdentityType, model.Identity, model.Password);
			if (!operation.Succeeded)
				return operation.ToResult();

			var token = _fabricator.CreateToken(operation.Data);

			return Ok(new {AccessToken = token});
		}
	}
}