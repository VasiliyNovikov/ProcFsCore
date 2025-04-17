#if NETSTANDARD2_0
namespace System;

public static class EnumExtensions
{
    extension(Enum)
    {
        public static string[] GetNames<TEnum>() where TEnum : struct, Enum => Enum.GetNames(typeof(TEnum));
    }
}
#endif