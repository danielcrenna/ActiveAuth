// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace ActiveAuth.Models
{
	public interface IQueryableApplicationStore<TApplication> : IApplicationStore<TApplication>, IDisposable
		where TApplication : class
	{
		IQueryable<TApplication> Applications { get; }
	}
}