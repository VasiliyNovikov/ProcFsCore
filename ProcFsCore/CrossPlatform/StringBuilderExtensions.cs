#if NETSTANDARD2_0
namespace System.Text;

public static class StringBuilderExtensions
{
    extension(StringBuilder stringBuilder)
    {
        public StringBuilder Append(IFormatProvider? provider, string value) => stringBuilder.Append(value);
    }
}
#endif