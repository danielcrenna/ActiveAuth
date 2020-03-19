// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ActiveAuth.Models
{
	/// <summary>
	///     Provides implementation-safe expression predicates when <see cref="IQueryable" /> access is undesirable or
	///     unstable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISafeQueryable<T>
	{
		Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
		T SingleOrDefault(Expression<Func<T, bool>> predicate);

		Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
		T FirstOrDefault(Expression<Func<T, bool>> predicate);

		IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
		Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate);
	}
}