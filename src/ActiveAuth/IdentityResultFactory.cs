// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using TypeKitchen;

namespace ActiveAuth
{
	internal static class IdentityResultFactory
	{
		/// <summary>
		///     Provides an <see cref="IdentityResult" /> in a failed state, with errors.
		///     This prevents an allocation by forcing the error collection to an array.
		/// </summary>
		/// <param name="errors"></param>
		/// <returns></returns>
		public static IdentityResult Failed(ICollection<IdentityError> errors)
		{
			var read = ReadAccessor.Create(typeof(IdentityResult));
			var write = WriteAccessor.Create(typeof(IdentityResult));
			var result = new IdentityResult();
			var list = read[result, "_errors"] as List<IdentityError>;
			list?.AddRange(errors);
			write[result, "Succeeded"] = false;
			return result;
		}
	}
}