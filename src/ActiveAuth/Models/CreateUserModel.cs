// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Models
{
	public class CreateUserModel
	{
		[Required]
		[ProtectedPersonalData]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[PersonalData] public string UserName { get; set; }

		[ProtectedPersonalData]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }

		[ProtectedPersonalData]
		[DataType(DataType.PhoneNumber)]
		public string PhoneNumber { get; set; }

		public bool PhoneNumberConfirmed { get; set; }

		public string ConcurrencyStamp { get; set; }
	}
}