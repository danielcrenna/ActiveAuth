// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using ActiveTenant;

namespace ActiveAuth.Models
{
	public class IdentityApplicationContextStore<TApplication> : IApplicationContextStore<TApplication>
		where TApplication : IdentityApplication
	{
		private readonly IApplicationService<TApplication> _applicationService;

		public IdentityApplicationContextStore(IApplicationService<TApplication> applicationService) =>
			_applicationService = applicationService;

		public async Task<ApplicationContext<TApplication>> FindByKeyAsync(string applicationKey)
		{
			var application = await _applicationService.FindByNameAsync(applicationKey);
			if (application?.Data == null)
			{
				return null;
			}

			var context = new ApplicationContext<TApplication>
			{
				Application = application.Data, Identifiers = new[] {application.Data.Name}
			};
			return context;
		}
	}
}