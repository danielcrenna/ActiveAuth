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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ActiveAuth.Api
{
	[Route("applications")]
	[DynamicController(typeof(IdentityApiOptions))]
	[DynamicAuthorize(typeof(IdentityApiOptions), nameof(IdentityApiOptions.Policies),
		nameof(IdentityApiOptions.Policies.Applications))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Identity", "Manages application access controls.")]
	[DisplayName("Applications")]
	[MetaDescription("Manages system applications.")]
	public class ApplicationController<TApplication, TKey> : Controller,
		IDynamicFeatureToggle<IdentityApiFeature>
		where TApplication : IdentityApplication<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IApplicationService<TApplication> _applicationService;
		private readonly IOptions<IdentityApiOptions> _options;

		public ApplicationController(IApplicationService<TApplication> applicationService,
			IOptions<IdentityApiOptions> options)
		{
			_applicationService = applicationService;
			_options = options;
		}

		[DynamicHttpGet("")]
		public async Task<IActionResult> Get()
		{
			var applications = await _applicationService.GetAsync();
			if (applications.Data == null)
				return NotFound();

			return Ok(applications.Data);
		}

		[DynamicHttpPost("")]
		public async Task<IActionResult> Create([FromBody] CreateApplicationModel model)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _applicationService.CreateAsync(model);

			return result.Succeeded
				? Created($"{_options.Value.RootPath ?? string.Empty}/applications/{result.Data.Id}", result.Data)
				: (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _applicationService.DeleteAsync(id);
			if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
			{
				return NotFound();
			}

			return result.Succeeded ? NoContent() : (IActionResult) BadRequest(result.Errors);
		}

		[DynamicHttpPut("{id}")]
		public async Task<IActionResult> Update([FromBody] TApplication tenant)
		{
			if (!this.TryValidateModelOrError(ModelState, ErrorEvents.ValidationFailed,
				ErrorStrings.ValidationFailed, out var error))
				return error;

			var result = await _applicationService.UpdateAsync(tenant);
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
			var application = await _applicationService.FindByIdAsync(id);
			if (application?.Data == null)
			{
				return NotFound();
			}

			return application.Succeeded
				? Ok(application.Data)
				: (IActionResult) BadRequest(application.Errors);
		}

		[DynamicHttpGet("name/{name}")]
		public async Task<IActionResult> FindByUsername([FromRoute] string name)
		{
			var application = await _applicationService.FindByNameAsync(name);
			if (application?.Data == null)
			{
				return NotFound();
			}

			return application.Succeeded
				? Ok(application.Data)
				: (IActionResult) BadRequest(application.Errors);
		}
	}
}