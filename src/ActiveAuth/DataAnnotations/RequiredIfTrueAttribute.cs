// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using TypeKitchen;

namespace ActiveAuth.DataAnnotations
{
	public sealed class RequiredIfTrueAttribute : ValidationAttribute
	{
		private readonly string _propertyOrFieldName;

		public RequiredIfTrueAttribute(string propertyOrFieldName)
		{
			_propertyOrFieldName = propertyOrFieldName;
			ErrorMessage = $"{FormatErrorMessage(propertyOrFieldName)} is true, therefore, {{0}} is required.";
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var accessor = ReadAccessor.Create(validationContext.ObjectInstance,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, out var members);
			if (!members.TryGetValue(_propertyOrFieldName, out _))
				return ValidationResult.Success; // user error

			if (!accessor.TryGetValue(validationContext.ObjectInstance, _propertyOrFieldName, out var propertyOrField))
				return ValidationResult.Success; // user error

			if (!(propertyOrField is bool flag))
				return ValidationResult.Success; // user error

			if (!flag)
				return ValidationResult.Success; // not required

			var attribute = new RequiredAttribute();
			return !attribute.IsValid(value) ? Invalid(validationContext) : ValidationResult.Success;
		}

		private ValidationResult Invalid(ValidationContext validationContext)
		{
			var errorMessage = FormatErrorMessage(validationContext.DisplayName);

			return new ValidationResult(errorMessage, new[] {validationContext.MemberName});
		}
	}
}