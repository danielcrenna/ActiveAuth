// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
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
	[Route("tenants")]
	[DynamicController(typeof(IdentityApiOptions))]
	[DynamicAuthorize(typeof(IdentityApiOptions), nameof(IdentityApiOptions.Policies),
		nameof(IdentityApiOptions.Policies.Tenants))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Identity", "Manages application access controls.")]
	[DisplayName("Tenants")]
	[MetaDescription("Manages system tenants.")]
	public class TenantController<TTenant, TKey> : Controller, IDynamicFeatureToggle<IdentityApiFeature>
		where TTenant : IdentityTenant<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IOptions<IdentityApiOptions> _options;
		private readonly ITenantService<TTenant> _tenantService;

		public TenantController(ITenantService<TTenant> tenantService, IOptions<IdentityApiOptions> options)
		{
			_tenantService = tenantService;
			_options = options;
		}

		[DynamicHttpGet("")]
		public async Task<IActionResult> Get()
		{
			var tenants = await _tenantService.GetAsync();
			if (tenants?.Data == null)
				return NotFound();
			return Ok(tenants.Data);
		}

		[DynamicHttpPost("")]
		public async Task<IActionResult> Create([FromBody] CreateTenantModel model)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
				out var error))
				return error;

			var result = await _tenantService.CreateAsync(model);

			return result.Succeeded
				? Created($"{_options.Value.RootPath ?? string.Empty}/tenants/{result.Data.Id}", result.Data)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
				out var error))
				return error;

			var result = await _tenantService.DeleteAsync(id);
			if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
			{
				return NotFound();
			}

			return result.Succeeded ? NoContent() : (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpPut("{id}")]
		public async Task<IActionResult> Update([FromBody] TTenant tenant)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
				out var error))
				return error;

			var result = await _tenantService.UpdateAsync(tenant);
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
			var tenant = await _tenantService.FindByIdAsync(id);
			if (tenant?.Data == null)
			{
				return NotFound();
			}

			return tenant.Succeeded
				? Ok(tenant.Data)
				: (IActionResult) BadRequest(tenant.Errors);
		}

		[DynamicHttpGet("name/{name}")]
		public async Task<IActionResult> FindByUsername([FromRoute] string name)
		{
			var tenant = await _tenantService.FindByNameAsync(name);
			if (tenant?.Data == null)
			{
				return NotFound();
			}

			return tenant.Succeeded
				? Ok(tenant.Data)
				: (IActionResult) BadRequest(tenant.Errors);
		}
	}
}