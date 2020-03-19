// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace ActiveAuth.Models
{
	public class NoQueryableProvider<T> : IQueryableProvider<T>
	{
		public bool IsSafe => false;
		public bool SupportsUnsafe => false;
		public IQueryable<T> Queryable => null;
		public IQueryable<T> UnsafeQueryable => null;
		public ISafeQueryable<T> SafeQueryable => null;
		public IEnumerable<T> SafeAll => null;
	}
}