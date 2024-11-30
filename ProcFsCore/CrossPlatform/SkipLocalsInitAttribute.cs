#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Module |
                AttributeTargets.Class |
                AttributeTargets.Struct |
                AttributeTargets.Interface |
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Event,
                Inherited = false)]
public sealed class SkipLocalsInitAttribute : Attribute;
#endif