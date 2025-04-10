namespace System;

public static class EnumExtensions
{
    public static string[] GetNames<TEnum>() where TEnum : struct, Enum
    {
        return Enum
#if NETSTANDARD2_0
            .GetNames(typeof(TEnum));
#else
            .GetNames<TEnum>();
#endif
    }
}