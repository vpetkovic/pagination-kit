// Polyfill required for C# 9 record types when targeting netstandard2.0 or netstandard2.1.
// System.Runtime.CompilerServices.IsExternalInit was introduced in .NET 5 and is absent
// from all .NET Standard TFMs. Without this, the compiler cannot emit init-only setters.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
