// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ActiveAuth.Configuration;
using ActiveAuth.Events;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveOptions;
using ActiveRoutes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveAuth.Api
{
	public static class Add
	{
		public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration apiConfig)
		{
			return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication,
				string>(services, apiConfig.FastBind);
		}

		public static IServiceCollection AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services, IConfiguration apiConfig)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			return AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(services, apiConfig.FastBind);
		}

		public static IServiceCollection AddIdentityApi(this IServiceCollection services,
			Action<IdentityApiOptions> configureApi = null)
		{
			return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication,
				string>(services, configureApi);
		}

		public static IServiceCollection AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(
			this IServiceCollection services, Action<IdentityApiOptions> configureApi = null)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			services.AddMvcCore()
				.AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(configureApi);

			return services;
		}

		public static IMvcCoreBuilder AddIdentityApi(this IMvcCoreBuilder mvcBuilder, IConfiguration apiConfig)
		{
			return AddIdentityApi<IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication,
				string>(mvcBuilder, apiConfig.FastBind);
		}

		public static IMvcCoreBuilder AddIdentityApi<TUser, TRole, TTenant, TApplication, TKey>(
			this IMvcCoreBuilder mvcBuilder,
			Action<IdentityApiOptions> configureApi = null)
			where TUser : IdentityUserExtended<TKey>
			where TRole : IdentityRoleExtended<TKey>
			where TTenant : IdentityTenant<TKey>
			where TApplication : IdentityApplication<TKey>
			where TKey : IEquatable<TKey>
		{
			if (configureApi != null)
				mvcBuilder.Services.Configure(configureApi);

			mvcBuilder.Services.AddScoped<ISignInHandler, CookiesSignInHandler<TTenant, TApplication, TKey>>();

			mvcBuilder.AddActiveRoute<TenantController<TTenant, TKey>, IdentityApiFeature, IdentityApiOptions>();
			mvcBuilder.AddActiveRoute<ApplicationController<TApplication, TKey>, IdentityApiFeature, IdentityApiOptions>();
			mvcBuilder.AddActiveRoute<UserController<TUser, TTenant, TKey>, IdentityApiFeature, IdentityApiOptions>();
			mvcBuilder.AddActiveRoute<RoleController<TRole, TKey>, IdentityApiFeature, IdentityApiOptions>();

			if (mvcBuilder.Services.BuildServiceProvider().GetRequiredService<ITokenInfoProvider>().Enabled)
				mvcBuilder
					.AddActiveRoute<TokenController<TUser, TTenant, TApplication, TKey>, IdentityApiFeature,
						IdentityApiOptions>();

			return mvcBuilder;
		}

		public static IServiceCollection AddSuperUserTokenController<TKey>(this IServiceCollection services)
			where TKey : IEquatable<TKey>
		{
			var mvcBuilder = services.AddMvcCore();
			mvcBuilder.AddSuperUserTokenController<TKey>();
			return services;
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TKey>(this IMvcCoreBuilder mvcBuilder, IConfiguration config)
			where TKey : IEquatable<TKey>
		{
			return mvcBuilder.AddSuperUserTokenController<TKey>(config.FastBind);
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TKey>(this IMvcCoreBuilder mvcBuilder, Action<SuperUserOptions> configureAction = null)
			where TKey : IEquatable<TKey>
		{
			if (configureAction != null)
				mvcBuilder.Services.Configure(configureAction);

			mvcBuilder.AddActiveRoute<SuperUserTokenController<TKey>, SuperUserFeature, SuperUserOptions>();
			return mvcBuilder;
		}
	}
}