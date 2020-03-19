// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using ActiveAuth.Configuration;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using ActiveAuth.Security;
using ActiveAuth.Services;
using ActiveAuth.Validators;
using ActiveLogging;
using ActiveTenant;
using HQ.Platform.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveAuth
{
	public static class Add
	{
		private static readonly Action<IdentityOptionsExtended> Defaults;

		static Add() =>
			Defaults = x =>
			{
				// Sensible defaults not set by ASP.NET Core Identity:
				x.Stores.ProtectPersonalData = true;
				x.Stores.MaxLengthForKeys = 128;

				// Extended:
				x.User.RequireUniqueEmail = true;
			};

		public static IdentityBuilder AddIdentityExtended(this IServiceCollection services,
			IConfiguration configuration)
		{
			return services
				.AddIdentityCoreExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication
					, string>(configuration);
		}

		public static IdentityBuilder AddIdentityExtended(this IServiceCollection services,
			Action<IdentityOptionsExtended> configureIdentityExtended = null,
			Action<IdentityOptions> configureIdentity = null)
		{
			return services
				.AddIdentityCoreExtended<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication
					, string>(configureIdentityExtended, configureIdentity);
		}

		public static IdentityBuilder AddIdentityExtended<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services, IConfiguration configuration)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			return services.AddIdentityCoreExtended<TUser, TRole, TTenant, TApplication, TKey>(configuration);
		}

		public static IdentityBuilder AddIdentityExtended<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services,
			Action<IdentityOptionsExtended> configureIdentityExtended = null)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			return services.AddIdentityCoreExtended<TUser, TRole, TTenant, TApplication, TKey>(o =>
			{
				configureIdentityExtended?.Invoke(o);
			});
		}

		public static IdentityBuilder AddIdentityCoreExtended<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services,
			IConfiguration configuration)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			AddIdentityPreamble(services);
			services.Configure<IdentityOptions>(configuration);
			services.Configure<IdentityOptionsExtended>(configuration);

			return services.AddIdentityCoreExtended<TUser, TRole, TTenant, TApplication, TKey>(configuration.Bind,
				configuration.Bind);
		}

		private static void AddIdentityPreamble(IServiceCollection services)
		{
			var authBuilder = services.AddAuthentication(o =>
			{
				o.DefaultScheme = IdentityConstants.ApplicationScheme;
				o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
			});
			authBuilder.AddIdentityCookies(o => { });
		}

		public static IdentityBuilder AddIdentityCoreExtended<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services,
			Action<IdentityOptionsExtended> configureIdentityExtended = null,
			Action<IdentityOptions> configureIdentity = null)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			services.AddSafeLogging();

			var identityBuilder = services.AddIdentityCore<TUser>(o =>
			{
				var x = new IdentityOptionsExtended(o);
				Defaults(x);
				configureIdentityExtended?.Invoke(x);
				o.Apply(x);
			});

			if (configureIdentityExtended != null)
			{
				services.Configure(configureIdentityExtended);
			}

			if (configureIdentity != null)
			{
				services.Configure(configureIdentity);
			}

			identityBuilder.AddDefaultTokenProviders();

			// See: https://github.com/blowdart/AspNetCoreIdentityEncryption
			identityBuilder.AddPersonalDataProtection<NoLookupProtector, NoLookupProtectorKeyRing>();
			identityBuilder.Services.AddSingleton<IPersonalDataProtector, DefaultPersonalDataProtector>();

			services.AddScoped<IEmailValidator<TUser>, DefaultEmailValidator<TUser>>();
			services.AddScoped<IPhoneNumberValidator<TUser>, DefaultPhoneNumberValidator<TUser>>();
			services.AddScoped<IUsernameValidator<TUser>, DefaultUsernameValidator<TUser>>();

			var validator = services.SingleOrDefault(x => x.ServiceType == typeof(IUserValidator<TUser>));
			var removed = services.Remove(validator);
			Debug.Assert(validator != null);
			Debug.Assert(removed);

			services.AddScoped<IUserValidator<TUser>, UserValidatorExtended<TUser>>();
			services.AddScoped<ITenantValidator<TTenant, TKey>, TenantValidator<TTenant, TUser, TKey>>();
			services
				.AddScoped<IApplicationValidator<TApplication, TUser, TKey>,
					ApplicationValidator<TApplication, TUser, TKey>>();

			services.AddScoped<IUserService<TUser>, UserService<TUser, TKey>>();
			services.AddScoped<ITenantService<TTenant>, TenantService<TTenant, TUser, TKey>>();
			services.AddScoped<IApplicationService<TApplication>, ApplicationService<TApplication, TUser, TKey>>();
			services.AddScoped<IRoleService<TRole>, RoleService<TRole, TKey>>();
			services.AddScoped<ISignInService<TUser>, SignInService<TUser, TKey>>();

			// untyped forwarding
			services.AddTransient<IApplicationService>(r => r.GetService<IApplicationService<TApplication>>());

			return identityBuilder;
		}

		public static IServiceCollection AddIdentityTenantContextStore<TTenant>(this IServiceCollection services)
			where TTenant : IdentityTenant
		{
			services.AddScoped<ITenantContextStore<TTenant>, IdentityTenantContextStore<TTenant>>();
			return services;
		}

		public static IServiceCollection AddIdentityApplicationContextStore<TApplication>(
			this IServiceCollection services)
			where TApplication : IdentityApplication
		{
			services.AddScoped<IApplicationContextStore<TApplication>, IdentityApplicationContextStore<TApplication>>();
			return services;
		}
	}
}