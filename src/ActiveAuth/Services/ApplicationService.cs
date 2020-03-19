// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using ActiveErrors;

namespace ActiveAuth.Services
{
	public class ApplicationService<TApplication, TUser, TKey> : IApplicationService<TApplication>
		where TApplication : IdentityApplication<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly ApplicationManager<TApplication, TUser, TKey> _applicationManager;
		private readonly IQueryableProvider<TApplication> _queryableProvider;

		public ApplicationService(ApplicationManager<TApplication, TUser, TKey> applicationManager,
			IQueryableProvider<TApplication> queryableProvider)
		{
			_applicationManager = applicationManager;
			_queryableProvider = queryableProvider;
		}

		public IQueryable<TApplication> Applications => _applicationManager.Applications;

		public async Task<Operation<int>> GetCountAsync()
		{
			var result = await _applicationManager.GetCountAsync();
			var operation = new Operation<int>(result);
			return operation;
		}

		public Task<Operation<IEnumerable<TApplication>>> GetAsync()
		{
			var all = _queryableProvider.SafeAll ?? Applications;
			return Task.FromResult(new Operation<IEnumerable<TApplication>>(all));
		}

		public async Task<Operation<TApplication>> CreateAsync(CreateApplicationModel model)
		{
			var application = (TApplication) FormatterServices.GetUninitializedObject(typeof(TApplication));
			application.Name = model.Name;
			application.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

			var result = await _applicationManager.CreateAsync(application);
			return result.ToOperation(application);
		}

		public async Task<Operation> UpdateAsync(TApplication application)
		{
			var result = await _applicationManager.UpdateAsync(application);
			return result.ToOperation();
		}

		public async Task<Operation> DeleteAsync(string id)
		{
			var operation = await FindByIdAsync(id);
			if (!operation.Succeeded)
			{
				return operation;
			}

			var deleted = await _applicationManager.DeleteAsync(operation.Data);
			return deleted.ToOperation();
		}

		public async Task<Operation<TApplication>> FindByIdAsync(string id)
		{
			var application = await _applicationManager.FindByIdAsync(id);
			return application == null
				? new Operation<TApplication>(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ApplicationNotFound,
					HttpStatusCode.NotFound))
				: new Operation<TApplication>(application);
		}

		public async Task<Operation<TApplication>> FindByNameAsync(string name)
		{
			return new Operation<TApplication>(await _applicationManager.FindByNameAsync(name));
		}
	}
}