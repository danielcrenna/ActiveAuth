// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveAuth.Models
{
	public interface IQueryableProvider<T>
	{
		/// <summary>
		///     Determines whether to throw a <see cref="NotSupportedException" /> if <see cref="Queryable" /> is accessed.
		///     If this is <code>false</code>, then developers may potentially access a <see cref="Queryable" /> that is
		///     unpredictable, or, if none is available, may materialize the entire underlying collection in order to
		///     perform a query.
		/// </summary>
		bool IsSafe { get; }

		bool SupportsUnsafe { get; }
		IQueryable<T> Queryable { get; }
		IQueryable<T> UnsafeQueryable { get; }
		ISafeQueryable<T> SafeQueryable { get; }
		IEnumerable<T> SafeAll { get; }
	}
}