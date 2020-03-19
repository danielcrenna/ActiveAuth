// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public interface IApplicationStore<TApplication> where TApplication : class
	{
		CancellationToken CancellationToken { get; }

		Task<string> GetApplicationIdAsync(TApplication application, CancellationToken cancellationToken);
		Task<string> GetApplicationNameAsync(TApplication application, CancellationToken cancellationToken);
		Task<int> GetCountAsync(CancellationToken cancellationToken);

		Task<IdentityResult> CreateAsync(TApplication application, CancellationToken cancellationToken);
		Task<IdentityResult> UpdateAsync(TApplication application, CancellationToken cancellationToken);
		Task<IdentityResult> DeleteAsync(TApplication application, CancellationToken cancellationToken);

		Task SetApplicationNameAsync(TApplication application, string name, CancellationToken cancellationToken);

		Task SetNormalizedApplicationNameAsync(TApplication application, string normalizedName,
			CancellationToken cancellationToken);

		Task<TApplication> FindByIdAsync(string applicationId, CancellationToken cancellationToken);

		Task<IEnumerable<TApplication>> FindByIdsAsync(IEnumerable<string> applicationIds,
			CancellationToken cancellationToken);

		Task<TApplication> FindByNameAsync(string normalizedApplicationName, CancellationToken cancellationToken);
	}
}