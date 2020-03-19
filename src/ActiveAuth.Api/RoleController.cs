// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveAuth.Configuration;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveErrors;
using ActiveRoutes;
using ActiveRoutes.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Api
{
	[Route("roles")]
	[DynamicController(typeof(IdentityApiOptions))]
	[DynamicAuthorize(typeof(IdentityApiOptions), nameof(IdentityApiOptions.Policies),
		nameof(IdentityApiOptions.Policies.Roles))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Identity", "Manages application access controls.")]
	[DisplayName("Roles")]
	[MetaDescription("Manages system roles.")]
	public class RoleController<TRole, TKey> : Controller, IDynamicComponentEnabled<IdentityApiFeature>
		where TRole : IdentityRoleExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IOptionsMonitor<IdentityApiOptions> _options;
		private readonly IRoleService<TRole> _roleService;
		private readonly ITokenInfoProvider _tokenInfoProvider;

		public RoleController(IRoleService<TRole> roleService, ITokenInfoProvider tokenInfoProvider,
			IOptionsMonitor<IdentityApiOptions> options)
		{
			_roleService = roleService;
			_tokenInfoProvider = tokenInfoProvider;
			_options = options;
		}

		[DynamicHttpGet]
		public async Task<IActionResult> Get()
		{
			var roles = await _roleService.GetAsync();
			if (roles?.Data == null)
			{
				return NotFound();
			}

			return Ok(roles.Data);
		}

		[DynamicHttpPost("")]
		public async Task<IActionResult> Create([FromBody] CreateRoleModel model)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _roleService.CreateAsync(model);

			return result.Succeeded
				? Created($"{_options.CurrentValue.RootPath ?? string.Empty}/roles/{result.Data.Id}", result.Data)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpPut("{id}")]
		public async Task<IActionResult> Update([FromBody] TRole role)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _roleService.UpdateAsync(role);
			if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
			{
				return NotFound();
			}

			return result.Succeeded ? Ok() : (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _roleService.DeleteAsync(id);
			if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
			{
				return NotFound();
			}

			return result.Succeeded ? Ok() : (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpGet("{id}/claims")]
		public async Task<IActionResult> GetClaims([FromRoute] string id)
		{
			var role = await _roleService.FindByIdAsync(id);
			if (role?.Data == null)
			{
				return NotFound();
			}

			var claims = await _roleService.GetClaimsAsync(role.Data);

			if (claims?.Data.Count == 0)
			{
				return NotFound();
			}

			return Ok(claims);
		}

		[DynamicHttpPost("{id}/claims")]
		public async Task<IActionResult> AddClaim([FromRoute] string id, [FromBody] CreateClaimModel model)
		{
			if (!this.TryValidateModelOrError(model, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var role = await _roleService.FindByIdAsync(id);
			if (role?.Data == null)
			{
				return NotFound();
			}

			var issuer = _tokenInfoProvider.Issuer;
			var claim = new Claim(model.Type, model.Value, model.ValueType ?? ClaimValueTypes.String, issuer);
			var result = await _roleService.AddClaimAsync(role.Data, claim);

			return result.Succeeded
				? Created($"/api/roles/{role.Data.Id}/claims", claim)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpDelete("{id}/claims/{type}/{value}")]
		public async Task<IActionResult> RemoveClaim([FromRoute] string id, [FromRoute] string type,
			[FromRoute] string value)
		{
			var user = await _roleService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var claims = await _roleService.GetClaimsAsync(user.Data);

			var claim = claims.Data.FirstOrDefault(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
			                                            x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

			if (claim == null)
			{
				return NotFound();
			}

			var result = await _roleService.RemoveClaimAsync(user.Data, claim);

			return result.Succeeded
				? StatusCode((int) HttpStatusCode.NoContent)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpGet("{id}")]
		public async Task<TRole> FindById([FromRoute] string id)
		{
			var role = await _roleService.FindByIdAsync(id);
			return role.Data;
		}
	}
}