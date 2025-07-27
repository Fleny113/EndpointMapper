// We need to polyfill some compiler services attributes that are not available in .NET Standard 2.0.
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

public class RequiredMemberAttribute : Attribute;
public class CompilerFeatureRequiredAttribute : Attribute;
public class IsExternalInit : Attribute;