// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace ActiveAuth
{
	public static class Constants
	{
		public const string SuperUserDefaultUserName = "superuser";
		public const string SuperUserDefaultEmail = "superuser@email.com";
		public const string SuperUserDefaultPhoneNumber = "9999999999";
		public const string SuperUserSecurityStamp = "A2ECC018-9B97-420B-815E-9D5B595BFA86";
		public const int SuperUserNumberId = int.MaxValue;
		public const string SuperUserStringId = "87BA0A16-7253-4A6F-A8D4-82DFA1F723C1";

		public static readonly Guid SuperUserGuidId = Guid.Parse(SuperUserStringId);

		public static class Tokens
		{
			public const string NoSigningKeySet = "PRIVATE-KEY-REPLACE-ME";
			public const string NoEncryptionKeySet = "ENCRYPTION-KEY-REPLACE-ME";
		}

		public static class ClaimValues
		{
			public const string SuperUser = "superuser";
		}
	}
}