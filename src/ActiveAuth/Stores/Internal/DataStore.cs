// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActiveErrors;
using ActiveStorage;

namespace ActiveAuth.Stores.Internal
{
	public class DataStore : IObjectQueryByExampleStore, IObjectCountStore, IObjectCreateStore,
		IObjectDeleteByExampleStore, IObjectUpdateByExampleStore
	{
		private readonly IObjectCreateStore _create;
		private readonly IObjectDeleteByExampleStore _deleteByExample;
		private readonly IObjectQueryByExampleStore _queryByExample;
		private readonly IObjectCountStore _queryCount;
		private readonly IObjectUpdateByExampleStore _updateByExample;

		public DataStore(IObjectQueryByExampleStore queryByExample, IObjectCountStore queryCount,
			IObjectCreateStore create, IObjectDeleteByExampleStore deleteByExample,
			IObjectUpdateByExampleStore updateByExample)
		{
			_queryByExample = queryByExample;
			_queryCount = queryCount;
			_create = create;
			_deleteByExample = deleteByExample;
			_updateByExample = updateByExample;
		}

		public async Task<Operation<ulong>> CountAsync(Type type, CancellationToken cancellationToken = default)
		{
			return await _queryCount.CountAsync(type, cancellationToken);
		}

		public async Task<Operation<ulong>> CountAsync<T>(CancellationToken cancellationToken = default)
		{
			return await _queryCount.CountAsync<T>(cancellationToken);
		}

		public async Task<Operation<ObjectSave>> CreateAsync(object @object,
			CancellationToken cancellationToken = default, params string[] fields)
		{
			return await _create.CreateAsync(@object, cancellationToken, fields);
		}

		public async Task<Operation<int>> DeleteByExampleAsync<T>(object example,
			CancellationToken cancellationToken = default)
		{
			return await _deleteByExample.DeleteByExampleAsync<T>(example, cancellationToken);
		}

		public async Task<Operation<IEnumerable<T>>> QueryByExampleAsync<T>(
			CancellationToken cancellationToken = default)
		{
			return await _queryByExample.QueryByExampleAsync<T>(cancellationToken);
		}

		public async Task<Operation<IEnumerable<T>>> QueryByExampleAsync<T>(object example,
			CancellationToken cancellationToken = default)
		{
			return await _queryByExample.QueryByExampleAsync<T>(example, cancellationToken);
		}

		public async Task<Operation<T>> QuerySingleOrDefaultByExampleAsync<T>(object example,
			CancellationToken cancellationToken = default)
		{
			return await _queryByExample.QuerySingleOrDefaultByExampleAsync<T>(example, cancellationToken);
		}

		public async Task<Operation<int>> UpdateByExampleAsync<T>(T @object, object example,
			CancellationToken cancellationToken = default)
		{
			return await _updateByExample.UpdateByExampleAsync(@object, example, cancellationToken);
		}
	}
}