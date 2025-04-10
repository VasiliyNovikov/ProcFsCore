#if NETSTANDARD2_0
namespace System.Text;

public static class StringBuilderExtensions
{
    public static StringBuilder Append(this StringBuilder stringBuilder, IFormatProvider? provider, string value) => stringBuilder.Append(value);
}
#endif