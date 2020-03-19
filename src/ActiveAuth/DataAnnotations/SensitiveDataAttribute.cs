// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace ActiveAuth.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public class SensitiveDataAttribute : Attribute
	{
		public SensitiveDataAttribute(SensitiveDataCategory category) => Category = category;

		public SensitiveDataCategory Category { get; }
	}
}