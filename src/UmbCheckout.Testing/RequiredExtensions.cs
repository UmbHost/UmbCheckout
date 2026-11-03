using System.Runtime.CompilerServices;

namespace UmbCheckout.Testing.Nullability;

/// <summary>
/// For tests, where a null is a failed expectation rather than something to handle: the value, or an exception naming the expression that
/// was null. Used instead of the null-forgiving operator, which would hide the null until it is dereferenced.
/// </summary>
public static class RequiredExtensions
{
    public static T Required<T>(this T? value, [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class
        => value ?? throw new InvalidOperationException($"Expected {expression} not to be null.");
}
