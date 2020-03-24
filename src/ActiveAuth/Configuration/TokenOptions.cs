// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveAuth.DataAnnotations;
using ActiveRoutes;

namespace ActiveAuth.Configuration
{
	public class TokenOptions : IFeatureToggle, IFeatureScheme, IFeatureNamespace
	{
		public string Issuer { get; set; } = "https://mysite.com";
		public string Audience { get; set; } = "https://mysite.com";
		public int TimeToLiveSeconds { get; set; } = 180;
		public bool Encrypt { get; set; } = true;
		public int ClockSkewSeconds { get; set; } = 10;
		public bool AllowRefresh { get; set; } = true;

		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		public string SigningKey { get; set; } = Constants.Tokens.NoSigningKeySet;

		[SensitiveData(SensitiveDataCategory.OperationalSecurity)]
		public string EncryptingKey { get; set; } = Constants.Tokens.NoEncryptingKeySet;

		public string RootPath { get; set; } = "/auth";

		public string Scheme { get; set; }
		public bool Enabled { get; set; } = true;
	}
}