// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using ActiveAuth.Providers;
using Microsoft.AspNetCore.Http;

namespace ActiveAuth.Stores.Extensions
{
	public static class ServiceProviderExtensions
	{
		public static bool TryGetRequestAbortCancellationToken(this IServiceProvider services,
			out CancellationToken cancelToken)
		{
			cancelToken = CancellationToken.None;
			var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
			var token = accessor?.HttpContext?.RequestAborted;
			if (!token.HasValue)
			{
				return false;
			}

			cancelToken = token.Value;
			return true;
		}

		public static bool TryGetTenantId<TKey>(this IServiceProvider services, out TKey tenantId)
			where TKey : IEquatable<TKey>
		{
			tenantId = !(services?.GetService(typeof(ITenantIdProvider<TKey>)) is ITenantIdProvider<TKey> provider)
				? default
				: provider.Id;
			return tenantId != null;
		}

		public static bool TryGetTenantName(this IServiceProvider services, out string tenantName)
		{
			var provider = services?.GetService(typeof(ITenantNameProvider)) as ITenantNameProvider;
			tenantName = provider?.Name;
			return !string.IsNullOrWhiteSpace(tenantName);
		}

		public static bool TryGetApplicationId<TKey>(this IServiceProvider services, out TKey applicationId)
			where TKey : IEquatable<TKey>
		{
			applicationId =
				!(services?.GetService(typeof(IApplicationIdProvider<TKey>)) is IApplicationIdProvider<TKey> provider)
					? default
					: provider.Id;
			return applicationId != null;
		}

		public static bool TryGetApplicationName(this IServiceProvider services, out string applicationName)
		{
			var provider = services?.GetService(typeof(IApplicationNameProvider)) as IApplicationNameProvider;
			applicationName = provider?.Name;
			return !string.IsNullOrWhiteSpace(applicationName);
		}

		public static bool TryGetClaim<TKey>(this IServiceProvider services, string type, out TKey value)
		{
			var accessor = services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
			var user = accessor?.HttpContext?.User;
			if (user == null || !user.Identity.IsAuthenticated)
			{
				value = default;
				return false;
			}

			var claim = user.FindFirst(type);
			if (claim == null)
			{
				value = default;
				return false;
			}

			value = (TKey) Convert.ChangeType(claim.Value, typeof(TKey));
			return true;
		}
	}
}