// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveErrors;

namespace ActiveAuth.Models
{
	public interface IApplicationService
	{
		Task<Operation<int>> GetCountAsync();
	}

	public interface IApplicationService<TApplication> : IApplicationService
	{
		Task<Operation<IEnumerable<TApplication>>> GetAsync();
		Task<Operation<TApplication>> CreateAsync(CreateApplicationModel model);
		Task<Operation> UpdateAsync(TApplication application);
		Task<Operation> DeleteAsync(string id);

		Task<Operation<TApplication>> FindByIdAsync(string id);
		Task<Operation<TApplication>> FindByNameAsync(string name);
	}
}