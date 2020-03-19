// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveAuth.Events;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveTenant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ActiveAuth.Api
{
	public class CookiesSignInHandler<TTenant, TApplication, TKey> : ISignInHandler
		where TTenant : IdentityTenant<TKey>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IAuthenticationService _authentication;
		private readonly IIdentityClaimNameProvider _claimNameProvider;
		private readonly ICookiesInfoProvider _cookiesInfoProvider;
		private readonly IHttpContextAccessor _http;

		public CookiesSignInHandler(IHttpContextAccessor http, IAuthenticationService authentication,
			IIdentityClaimNameProvider claimNameProvider,
			ICookiesInfoProvider cookiesInfoProvider)
		{
			_http = http;
			_authentication = authentication;
			_claimNameProvider = claimNameProvider;
			_cookiesInfoProvider = cookiesInfoProvider;
		}

		#region Implementation of ISignInHandler

		public async Task OnSignInSuccessAsync(IList<Claim> claims)
		{
			if (_http.HttpContext.GetTenantContext<TTenant>() is TenantContext<TTenant> tenantContext &&
			    tenantContext.Value != null)
			{
				claims.Add(new Claim(_claimNameProvider.TenantIdClaim, $"{tenantContext.Value.Id}"));
				claims.Add(new Claim(_claimNameProvider.TenantNameClaim, tenantContext.Value.Name));
			}

			if (_http.HttpContext.GetApplicationContext<TApplication>() is ApplicationContext<TApplication>
				applicationContext && applicationContext.Application != null)
			{
				claims.Add(new Claim(_claimNameProvider.ApplicationIdClaim, $"{applicationContext.Application.Id}"));
				claims.Add(new Claim(_claimNameProvider.ApplicationNameClaim, applicationContext.Application.Name));
			}

			if (_cookiesInfoProvider.Enabled)
			{
				var scheme = _cookiesInfoProvider.Scheme;
				var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
				var properties = new AuthenticationProperties {IsPersistent = true};

				await _authentication.SignInAsync(_http.HttpContext, scheme, principal, properties);
				_http.HttpContext.User = principal;
			}
		}

		#endregion
	}
}