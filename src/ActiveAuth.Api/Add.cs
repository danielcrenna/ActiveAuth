// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ActiveAuth.Configuration;
using ActiveAuth.Events;
using ActiveAuth.Extensions;
using ActiveAuth.Models;
using ActiveAuth.Providers;
using ActiveOptions;
using ActiveRoutes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TypeKitchen;

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
				mvcBuilder.AddActiveRoute<TokenController<TUser, TTenant, TApplication, TKey>, IdentityApiFeature, IdentityApiOptions>();

			return mvcBuilder;
		}

		public static IServiceCollection AddSuperUserTokenController<TKey>(this IServiceCollection services, Func<IServiceProvider, DateTimeOffset> timestamps)
			where TKey : IEquatable<TKey>
		{
			var mvcBuilder = services.AddMvcCore();
			mvcBuilder.AddSuperUserTokenController<TKey>(timestamps);
			return services;
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TKey>(this IMvcCoreBuilder mvcBuilder, Func<IServiceProvider, DateTimeOffset> timestamps, IConfiguration configSuperUser)
			where TKey : IEquatable<TKey>
		{
			return mvcBuilder.AddSuperUserTokenController<TKey>(timestamps, null, null, configSuperUser.FastBind);
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TKey>(this IMvcCoreBuilder mvcBuilder, Func<IServiceProvider, DateTimeOffset> timestamps, IConfiguration configTokens, IConfiguration configSuperUser)
			where TKey : IEquatable<TKey>
		{
			return mvcBuilder.AddSuperUserTokenController<TKey>(timestamps, null, configTokens.FastBind, configSuperUser.FastBind);
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TKey>(this IMvcCoreBuilder mvcBuilder, Func<IServiceProvider, DateTimeOffset> timestamps, IConfiguration configClaims, IConfiguration configTokens, IConfiguration configSuperUser)
			where TKey : IEquatable<TKey>
		{
			return mvcBuilder.AddSuperUserTokenController<TKey>(timestamps, configClaims.FastBind, configTokens.FastBind, configSuperUser.FastBind);
		}

		public static IMvcCoreBuilder AddSuperUserTokenController<TKey>(this IMvcCoreBuilder mvcBuilder, Func<IServiceProvider, DateTimeOffset> timestamps, 
			Action<ClaimOptions> configureClaimsAction = null,
			Action<TokenOptions> configureTokensAction = null,
			Action<SuperUserOptions> configureSuperUserAction = null)
			where TKey : IEquatable<TKey>
		{
			if (configureClaimsAction != null)
				mvcBuilder.Services.Configure(configureClaimsAction);

			if (configureTokensAction != null)
				mvcBuilder.Services.Configure(configureTokensAction);

			if (configureSuperUserAction != null)
				mvcBuilder.Services.Configure(configureSuperUserAction);

			var claims = new ClaimOptions();
			configureClaimsAction?.Invoke(claims);

			var tokens = new TokenOptions();
			configureTokensAction?.Invoke(tokens);

			var superUser = new SuperUserOptions();
			configureSuperUserAction?.Invoke(superUser);

			var credentials = new
			{
				SigningKeyString = tokens.SigningKey,
				EncryptingKeyString = tokens.EncryptingKey
			}.QuackLike<ITokenCredentials>();
			
			AuthenticationExtensions.MaybeSetSecurityKeys(credentials);

			var scheme = superUser.Scheme ?? tokens.Scheme;

			mvcBuilder.Services.AddAuthentication().AddJwtBearer(scheme, o =>
			{
				if (tokens.Encrypt)
				{
					o.TokenValidationParameters = new TokenValidationParameters
					{
						TokenDecryptionKeyResolver = (token, securityToken, kid, parameters) => new[] {credentials.EncryptingKey.Key},
						ValidateIssuerSigningKey = false,
						ValidIssuer = tokens.Issuer,
						ValidateLifetime = true,
						ValidateAudience = true,
						ValidAudience = tokens.Audience,
						RequireSignedTokens = false,
						IssuerSigningKey = credentials.SigningKey.Key,
						TokenDecryptionKey = credentials.EncryptingKey.Key,
						ClockSkew = TimeSpan.FromSeconds(tokens.ClockSkewSeconds),
						RoleClaimType = claims.RoleClaim,
						NameClaimType = claims.UserNameClaim
					};
				}
				else
				{
					o.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						ValidIssuer = tokens.Issuer,
						ValidateLifetime = true,
						ValidateAudience = true,
						ValidAudience = tokens.Audience,
						RequireSignedTokens = true,
						IssuerSigningKey = credentials.SigningKey.Key,
						ClockSkew = TimeSpan.FromSeconds(tokens.ClockSkewSeconds),
						RoleClaimType = claims.RoleClaim,
						NameClaimType = claims.UserNameClaim
					};
				}
			});

			mvcBuilder.Services.TryAddSingleton<IIdentityClaimNameProvider, DefaultIdentityClaimNameProvider>();
			mvcBuilder.Services.TryAddSingleton<ITokenFabricator<TKey>>(r => new DefaultTokenFabricator<TKey>(()=> timestamps(r), r.GetRequiredService<IOptionsSnapshot<TokenOptions>>()));
			
			mvcBuilder.AddActiveRoute<SuperUserTokenController<TKey>, SuperUserFeature, SuperUserOptions>();
			return mvcBuilder;
		}
	}
}