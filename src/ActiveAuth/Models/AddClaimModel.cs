// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace ActiveAuth.Models
{
	public class AddClaimModel
	{
		[Required] public string Type { get; set; }

		[Required] public string Value { get; set; }

		public string ValueType { get; set; }
	}
}