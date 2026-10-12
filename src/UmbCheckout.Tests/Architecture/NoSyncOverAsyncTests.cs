using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Xunit;

namespace UmbCheckout.Tests.Architecture;

/// <summary>
/// Quality Phase 5: no thread blocks on a task (<c>.Result</c>, <c>.Wait()</c>, <c>GetAwaiter().GetResult()</c>) anywhere in UmbCheckout
/// or its licensing, apart from the documented obsolete sync methods kept for compatibility. Scans every UmbCheckout and
/// UmbHost.Licensing assembly this test project ships with (the file is also linked into the closed add-on tests).
/// </summary>
public sealed class NoSyncOverAsyncTests
{
    /// <summary>The one documented sync boundary: obsolete public sync methods kept so callers don't break.</summary>
    private static readonly HashSet<string> Allowed = new(StringComparer.Ordinal)
    {
        "UmbCheckout.Stripe.Services.StripeSessionService.GetSession",
        "UmbCheckout.Stripe.Services.StripeSessionService.CreateSession",
        "UmbCheckout.Stripe.Services.StripeSessionService.ClearSession",
    };

    private static readonly Assembly[] Assemblies = Directory.GetFiles(AppContext.BaseDirectory, "*.dll")
        .Select(Path.GetFileNameWithoutExtension)
        .Where(name => (name!.StartsWith("UmbCheckout", StringComparison.Ordinal) || name.StartsWith("UmbHost.Licensing", StringComparison.Ordinal))
                       && !name.Contains("Test", StringComparison.Ordinal))
        .Select(name => Assembly.Load(name!))
        .ToArray();

    [Fact]
    public void No_method_blocks_on_a_task()
    {
        var offenders = Assemblies
            .SelectMany(SafeTypes)
            .Where(t => !typeof(IAsyncStateMachine).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Cast<MethodBase>()
                .Concat(t.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)))
            .Where(m => !Allowed.Contains($"{m.DeclaringType!.FullName}.{m.Name}") && Blocks(m))
            .Select(m => $"{m.DeclaringType!.FullName}.{m.Name}")
            .Distinct()
            .ToList();

        Assert.True(offenders.Count == 0, "Blocking on a task: " + string.Join(", ", offenders));
    }

    [Fact]
    public void The_scan_finds_a_blocking_call()
        => Assert.True(Blocks(typeof(Offender).GetMethod(nameof(Offender.Blocking))!));

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    private static bool Blocks(MethodBase method)
    {
        byte[]? il;
        try
        {
            il = method.GetMethodBody()?.GetILAsByteArray();
        }
        catch (Exception)
        {
            return false;
        }

        if (il == null)
        {
            return false;
        }

        for (var i = 0; i < il.Length - 4; i++)
        {
            if (il[i] != OpCodes.Call.Value && il[i] != OpCodes.Callvirt.Value)
            {
                continue;
            }

            MethodBase? called;
            try
            {
                called = method.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1), method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null);
            }
            catch (Exception)
            {
                continue;
            }

            if (called?.DeclaringType is not { } type)
            {
                continue;
            }

            var declaring = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if ((declaring == typeof(Task<>) && called.Name == "get_Result")
                || (declaring == typeof(Task) && called.Name == "Wait")
                || ((declaring == typeof(TaskAwaiter) || declaring == typeof(TaskAwaiter<>)
                     || declaring == typeof(ValueTaskAwaiter<>) || declaring == typeof(ConfiguredTaskAwaitable<>.ConfiguredTaskAwaiter)) && called.Name == "GetResult"))
            {
                return true;
            }
        }

        return false;
    }

    private static class Offender
    {
        public static int Blocking() => Task.FromResult(1).GetAwaiter().GetResult();
    }
}
