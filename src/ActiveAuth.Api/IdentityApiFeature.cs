// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using ActiveRoutes;

namespace ActiveAuth.Api
{
	public class IdentityApiFeature : DynamicFeature
	{
		public override IList<Type> ControllerTypes { get; } = new[]
		{
			typeof(UserController<,,>), typeof(RoleController<,>), typeof(TenantController<,>),
			typeof(ApplicationController<,>)
		};
	}
}