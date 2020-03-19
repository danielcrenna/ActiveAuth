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
using ActiveErrors;
using ActiveRoutes;
using ActiveRoutes.Meta;
using ActiveTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Api
{
	[Route("users")]
	[DynamicController(typeof(IdentityApiOptions))]
	[DynamicAuthorize(typeof(IdentityApiOptions), nameof(IdentityApiOptions.Policies),
		nameof(IdentityApiOptions.Policies.Users))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Identity", "Manages application access controls.")]
	[DisplayName("Users")]
	[MetaDescription("Manages user accounts.")]
	public class UserController<TUser, TTenant, TKey> : Controller, IDynamicComponentEnabled<IdentityApiFeature>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IOptions<IdentityApiOptions> _options;
		private readonly ITenantService<TTenant> _tenantService;
		private readonly IUserService<TUser> _userService;

		public UserController(IUserService<TUser> userService, ITenantService<TTenant> tenantService,
			IOptions<IdentityApiOptions> options)
		{
			_userService = userService;
			_tenantService = tenantService;
			_options = options;
		}

		[DynamicHttpGet("")]
		public async Task<IActionResult> Get()
		{
			var users = await _userService.GetAsync();
			if (users?.Data == null)
			{
				return NotFound();
			}

			return Ok(users.Data);
		}

		[DynamicHttpPost("")]
		public async Task<IActionResult> Create([FromBody] CreateUserModel model)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _userService.CreateAsync(model);

			return result.Succeeded
				? Created($"{_options.Value.RootPath ?? string.Empty}/users/{result.Data.Id}", result.Data)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpPut("{id}")]
		public async Task<IActionResult> Update([FromBody] TUser user)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _userService.UpdateAsync(user);
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

			var result = await _userService.DeleteAsync(id);
			if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
			{
				return NotFound();
			}

			return result.Succeeded ? Ok() : (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpGet("{id}")]
		[DynamicHttpGet("id/{id}")]
		public async Task<IActionResult> FindById([FromRoute] string id)
		{
			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			return user.Succeeded
				? Ok(user.Data)
				: (IActionResult) BadRequest(user.Errors);
		}

		[DynamicHttpGet("email/{email}")]
		public async Task<IActionResult> FindByEmail([FromRoute] string email)
		{
			var user = await _userService.FindByEmailAsync(email);
			if (user?.Data == null)
			{
				return NotFound();
			}

			return user.Succeeded
				? Ok(user.Data)
				: (IActionResult) BadRequest(user.Errors);
		}

		[DynamicHttpGet("username/{username}")]
		public async Task<IActionResult> FindByUsername([FromRoute] string username)
		{
			var user = await _userService.FindByNameAsync(username);
			if (user?.Data == null)
			{
				return NotFound();
			}

			return user.Succeeded
				? Ok(user.Data)
				: (IActionResult) BadRequest(user.Errors);
		}

		[DynamicHttpGet("phone/{phone}")]
		public async Task<IActionResult> FindByPhoneNumber([FromRoute] string phone)
		{
			var user = await _userService.FindByPhoneNumberAsync(phone);
			if (user?.Data == null)
			{
				return NotFound();
			}

			return user.Succeeded
				? Ok(user.Data)
				: (IActionResult) BadRequest(user.Errors);
		}

		#region Role Assignment

		[DynamicHttpGet("{id}/roles")]
		public async Task<IActionResult> GetRoles([FromRoute] string id)
		{
			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var result = await _userService.GetRolesAsync(user.Data);
			if (result.Data == null || result.Data.Count == 0)
			{
				return NotFound();
			}

			return Ok(result.Data);
		}

		[DynamicHttpPost("{id}/roles/{role}")]
		public async Task<IActionResult> AddToRole([FromRoute] string id, [FromRoute] string role)
		{
			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var result = await _userService.AddToRoleAsync(user.Data, role);

			return result.Succeeded
				? Created($"/api/users/{user.Data}/roles", user)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpDelete("{id}/roles/{role}")]
		public async Task<IActionResult> RemoveFromRole([FromRoute] string id, [FromRoute] string role)
		{
			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var result = await _userService.RemoveFromRoleAsync(user.Data, role);

			return result.Succeeded
				? Ok()
				: (IActionResult) BadRequest(result.Errors);
		}

		#endregion

		#region Claim Assignment

		[DynamicHttpGet("{id}/claims")]
		public async Task<IActionResult> GetClaims([FromRoute] string id)
		{
			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var result = await _userService.GetClaimsAsync(user.Data);

			return result.Succeeded
				? Ok(result.Data)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpPost("{id}/claims")]
		public async Task<IActionResult> AddClaim([FromRoute] string id, [FromBody] AddClaimModel model)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var claim = new Claim(model.Type, model.Value, model.ValueType ?? ClaimValueTypes.String);

			var result = await _userService.AddClaimAsync(user.Data, claim);

			return result.Succeeded
				? Created($"/api/users/{user.Data}/claims", claim)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpDelete("{id}/claims/{type}/{value}")]
		public async Task<IActionResult> RemoveClaim([FromRoute] string id, [FromRoute] string type,
			[FromRoute] string value)
		{
			var user = await _userService.FindByIdAsync(id);
			if (user?.Data == null)
			{
				return NotFound();
			}

			var claims = await _userService.GetClaimsAsync(user.Data);

			var claim = claims.Data.FirstOrDefault(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
			                                            x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

			if (claim == null)
			{
				return NotFound();
			}

			var result = await _userService.RemoveClaimAsync(user.Data, claim);

			return result.Succeeded
				? StatusCode((int) HttpStatusCode.NoContent)
				: (IActionResult) BadRequest(result.Errors);
		}

		#endregion

		#region Tenant Assignment

		[DynamicHttpGet("email/{email}/tenants")]
		public async Task<IActionResult> FindTenantsByEmail([FromRoute] string email)
		{
			var tenants = await _tenantService.FindByEmailAsync(email);
			if (tenants?.Data == null)
			{
				return NotFound();
			}

			return tenants.Succeeded
				? Ok(tenants.Data)
				: (IActionResult) BadRequest(tenants.Errors);
		}

		[DynamicHttpGet("username/{username}/tenants")]
		public async Task<IActionResult> FindTenantsByUsername([FromRoute] string username)
		{
			var tenants = await _tenantService.FindByUserNameAsync(username);
			if (tenants?.Data == null)
			{
				return NotFound();
			}

			return tenants.Succeeded
				? Ok(tenants.Data)
				: (IActionResult) BadRequest(tenants.Errors);
		}

		[DynamicHttpGet("phone/{phone}/tenants")]
		public async Task<IActionResult> FindTenantsByPhoneNumber([FromRoute] string phone)
		{
			var tenants = await _tenantService.FindByPhoneNumberAsync(phone);
			if (tenants?.Data == null)
			{
				return NotFound();
			}

			return tenants.Succeeded
				? Ok(tenants.Data)
				: (IActionResult) BadRequest(tenants.Errors);
		}

		#endregion
	}
}