// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace ActiveAuth.Stores.Models
{
	public class AspNetPasswordHistory<TKey>
	{
		[Required] public TKey TenantId { get; set; }

		[Required] public TKey UserId { get; set; }

		[Required] public string PasswordHash { get; set; }

		[Required] public DateTimeOffset StartedAt { get; set; }

		public DateTimeOffset? EndedAt { get; set; }
	}
}