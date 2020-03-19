// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ActiveAuth.Configuration;
using Microsoft.AspNetCore.Identity;

namespace ActiveAuth.Extensions
{
	internal static class IdentityOptionsExtensions
	{
		public static void Apply(this IdentityOptions o, IdentityOptionsExtended x)
		{
			o.ClaimsIdentity.RoleClaimType = x.ClaimsIdentity.RoleClaimType;
			o.ClaimsIdentity.SecurityStampClaimType = x.ClaimsIdentity.SecurityStampClaimType;
			o.ClaimsIdentity.UserIdClaimType = x.ClaimsIdentity.UserIdClaimType;
			o.ClaimsIdentity.UserNameClaimType = x.ClaimsIdentity.UserNameClaimType;

			o.Stores.ProtectPersonalData = x.Stores.ProtectPersonalData;
			o.Stores.MaxLengthForKeys = x.Stores.MaxLengthForKeys;

			o.User.AllowedUserNameCharacters = x.User.AllowedUserNameCharacters;
			o.User.RequireUniqueEmail = x.User.RequireUniqueEmail;

			o.Lockout.AllowedForNewUsers = x.Lockout.AllowedForNewUsers;
			o.Lockout.DefaultLockoutTimeSpan = x.Lockout.DefaultLockoutTimeSpan;
			o.Lockout.MaxFailedAccessAttempts = x.Lockout.MaxFailedAccessAttempts;

			o.Password.RequireDigit = x.Password.RequireDigit;
			o.Password.RequireLowercase = x.Password.RequireLowercase;
			o.Password.RequireNonAlphanumeric = x.Password.RequireNonAlphanumeric;
			o.Password.RequireUppercase = x.Password.RequireUppercase;
			o.Password.RequiredLength = x.Password.RequiredLength;
			o.Password.RequiredUniqueChars = x.Password.RequiredUniqueChars;

			o.Tokens.AuthenticatorTokenProvider = x.Tokens.AuthenticatorTokenProvider;
			o.Tokens.ChangeEmailTokenProvider = x.Tokens.ChangeEmailTokenProvider;
			o.Tokens.ChangePhoneNumberTokenProvider = x.Tokens.ChangePhoneNumberTokenProvider;
			o.Tokens.EmailConfirmationTokenProvider = x.Tokens.EmailConfirmationTokenProvider;
			o.Tokens.PasswordResetTokenProvider = x.Tokens.PasswordResetTokenProvider;
			o.Tokens.ProviderMap = x.Tokens.ProviderMap;

			o.SignIn.RequireConfirmedEmail = x.SignIn.RequireConfirmedEmail;
			o.SignIn.RequireConfirmedPhoneNumber = x.SignIn.RequireConfirmedPhoneNumber;
		}
	}
}