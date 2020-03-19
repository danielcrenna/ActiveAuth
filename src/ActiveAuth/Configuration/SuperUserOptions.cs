// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using ActiveAuth.DataAnnotations;
using ActiveRoutes;

namespace ActiveAuth.Configuration
{
	public class SuperUserOptions : IFeatureNamespace, IFeatureToggle, IFeatureScheme, IFeaturePolicy
	{
		private string _email;
		private string _password;
		private string _phoneNumber;
		private string _username;

		public SuperUserOptions() => Enabled = false;

		[RequiredIfTrue(nameof(Enabled))]
		[DataType(DataType.Text)]
		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		public string Username
		{
			get => Enabled ? _username : null;
			set => _username = value;
		}

		[RequiredIfTrue(nameof(Enabled))]
		[DataType(DataType.Password)]
		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		public string Password
		{
			get => Enabled ? _password : null;
			set => _password = value;
		}

		[DataType(DataType.PhoneNumber)]
		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		public string PhoneNumber
		{
			get => Enabled ? _phoneNumber : null;
			set => _phoneNumber = value;
		}

		[DataType(DataType.EmailAddress)]
		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		public string Email
		{
			get => Enabled ? _email : null;
			set => _email = value;
		}

		public string RootPath { get; set; } = "/superuser";
		public string Policy { get; set; }
		public string Scheme { get; set; }

		public bool Enabled { get; set; }
	}
}