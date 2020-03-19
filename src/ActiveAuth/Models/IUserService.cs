// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveErrors;

namespace ActiveAuth.Models
{
	public interface IUserService
	{
		Task<Operation<int>> GetCountAsync();
	}

	public interface IUserService<TUser> : IUserService
	{
		IQueryable<TUser> Users { get; }

		Task<Operation<IEnumerable<TUser>>> GetAsync();
		Task<Operation<TUser>> CreateAsync(CreateUserModel model);
		Task<Operation> DeleteAsync(string id);

		Task<Operation<TUser>> FindByIdAsync(string id);
		Task<Operation<TUser>> FindByEmailAsync(string email);
		Task<Operation<TUser>> FindByNameAsync(string username);
		Task<Operation<TUser>> FindByPhoneNumberAsync(string phoneNumber);
		Task<Operation<TUser>> FindByLoginAsync(string loginProvider, string providerKey);
		Task<Operation<TUser>> FindByAsync(Expression<Func<TUser, bool>> predicate);
		Task<Operation<TUser>> FindByIdentity(IdentityType identityType, string identity);

		Task<Operation<IList<string>>> GetRolesAsync(TUser user);
		Task<Operation> AddToRoleAsync(TUser user, string role);
		Task<Operation> RemoveFromRoleAsync(TUser user, string role);

		Task<Operation<IList<Claim>>> GetClaimsAsync(TUser user);
		Task<Operation> AddClaimAsync(TUser user, Claim claim);
		Task<Operation> RemoveClaimAsync(TUser user, Claim claim);
		Task<Operation> AddClaimsAsync(TUser user, IEnumerable<Claim> claims);
		Task<Operation> RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims);

		Task<Operation<string>> GenerateChangePhoneNumberTokenAsync(TUser user, string phoneNumber);
		Task<Operation<string>> GenerateChangeEmailTokenAsync(TUser user, string newEmail);
		Task<Operation<string>> GenerateEmailConfirmationTokenAsync(TUser user);
		Task<Operation<string>> GeneratePasswordResetTokenAsync(TUser user);
		Task<Operation<IEnumerable<string>>> GenerateNewTwoFactorRecoveryCodesAsync(TUser user, int number);

		Task<Operation> ChangePhoneNumberAsync(TUser user, string phoneNumber, string token);
		Task<Operation> ChangeEmailAsync(TUser user, string newEmail, string token);
		Task<Operation> ChangePasswordAsync(TUser user, string token, string newPassword);
		Task<Operation> ConfirmEmailAsync(TUser user, string token);
		Task<Operation> ResetPasswordAsync(TUser user, string token, string newPassword);

		Task<Operation> UpdateAsync(TUser user);

		Task<Operation<TUser>> LinkExternalIdentityAsync(ClaimsPrincipal principal, string loginProvider,
			string providerKeyClaimType = null, string displayName = null);
	}
}