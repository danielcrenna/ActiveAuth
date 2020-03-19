// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace ActiveAuth.Models
{
	public interface IApplicationSecurityStampStore<TApplication> : IApplicationStore<TApplication>
		where TApplication : class
	{
		Task SetSecurityStampAsync(TApplication application, string stamp, CancellationToken cancellationToken);
		Task<string> GetSecurityStampAsync(TApplication application, CancellationToken cancellationToken);
	}
}