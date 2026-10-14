using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Xunit;

namespace UmbCheckout.Tests.Architecture;

/// <summary>
/// Quality Phase 5: no thread blocks on a task (<c>.Result</c>, <c>.Wait()</c>, <c>GetAwaiter().GetResult()</c>) anywhere in UmbCheckout
/// apart from the documented obsolete sync methods kept for compatibility. Scans every UmbCheckout assembly (and any named by the
/// "NoSyncOverAsync.ScanAlso" assembly metadata of the test project) this test project ships with. The file may be linked into other test projects.
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

    // Names from the test project's own assembly metadata, so a project that links this file can add assemblies without this file naming them.
    private static string[] Metadata(string key)
        => typeof(NoSyncOverAsyncTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(a => a.Key == key)
            .SelectMany(a => (a.Value ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

    private static readonly Assembly[] Assemblies = Directory.GetFiles(AppContext.BaseDirectory, "*.dll")
        .Select(Path.GetFileNameWithoutExtension)
        .Where(name => (name!.StartsWith("UmbCheckout", StringComparison.Ordinal) || Metadata("NoSyncOverAsync.ScanAlso").Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                       && !name.Contains("Test", StringComparison.Ordinal))
        .Select(name => Assembly.Load(name!))
        .ToArray();

    [Fact]
    public void No_method_blocks_on_a_task()
    {
        var offenders = Assemblies
            .SelectMany(SafeTypes)
            .SelectMany(OffendersIn)
            .Where(name => !Allowed.Contains(name))
            .Distinct()
            .ToList();

        Assert.True(offenders.Count == 0, "Blocking on a task: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Every_shipped_open_assembly_is_scanned()
    {
        // Quality Phase 5 review: the Forms workflow's assembly was referenced by no test project, so it was never scanned.
        var scanned = Assemblies.Select(a => a.GetName().Name).ToHashSet();
        var expected = typeof(NoSyncOverAsyncTests).Assembly.GetName().Name == "UmbCheckout.Tests"
            ? new HashSet<string?> { "UmbCheckout.Core", "UmbCheckout.Shared", "UmbCheckout.Stripe", "UmbCheckout.Stripe.Forms", "UmbCheckout.uSync", "UmbCheckout.Stripe.uSync" }
            : Metadata("NoSyncOverAsync.Expect").Select(n => (string?)n).ToHashSet();

        Assert.Superset(expected, scanned);
    }

    [Fact]
    public void The_scan_finds_a_blocking_call()
        => Assert.Contains($"{typeof(Offender).FullName}.{nameof(Offender.Blocking)}", OffendersIn(typeof(Offender)));

    [Theory]
    [InlineData(nameof(AsyncOffender.ResultInAsync))]
    [InlineData(nameof(AsyncOffender.GetResultInAsync))]
    [InlineData(nameof(AsyncOffender.WaitAllInAsync))]
    [InlineData(nameof(AsyncOffender.ConfiguredGetResultInAsync))]
    [InlineData(nameof(AsyncOffender.ValueTaskResultInAsync))]
    public void The_scan_finds_a_blocking_call_inside_an_async_method(string method)
        => Assert.Contains($"{typeof(AsyncOffender).FullName}.{method}", AsyncOffenderFindings());

    [Fact]
    public void Awaiting_is_not_blocking()
        => Assert.DoesNotContain($"{typeof(AsyncOffender).FullName}.{nameof(AsyncOffender.OnlyAwaits)}", AsyncOffenderFindings());

    private static List<string> AsyncOffenderFindings()
        => typeof(AsyncOffender).GetNestedTypes(BindingFlags.NonPublic).Append(typeof(AsyncOffender)).SelectMany(OffendersIn).ToList();

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

    /// <summary>The methods of <paramref name="type"/> that block, named as the source names them (an async method's state machine by its method).</summary>
    private static IEnumerable<string> OffendersIn(Type type)
    {
        if (typeof(IAsyncStateMachine).IsAssignableFrom(type))
        {
            var moveNext = type.GetMethod(nameof(IAsyncStateMachine.MoveNext), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return moveNext != null && Blocks(moveNext, insideAsync: true) ? [SourceName(type)] : [];
        }

        return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Cast<MethodBase>()
            .Concat(type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(m => Blocks(m, insideAsync: false))
            .Select(m => $"{m.DeclaringType!.FullName}.{m.Name}");
    }

    /// <summary><c>Owner+&lt;GetSessionAsync&gt;d__5</c> as <c>Owner.GetSessionAsync</c>; async lambdas by the method that holds them.</summary>
    private static string SourceName(Type stateMachine)
    {
        var method = System.Text.RegularExpressions.Regex.Match(stateMachine.Name, "^<+([^>]+)>").Groups[1].Value;
        var owner = stateMachine.DeclaringType!;
        while (owner.Name.StartsWith('<') && owner.DeclaringType != null)
        {
            owner = owner.DeclaringType;
        }

        return $"{owner.FullName}.{method}";
    }

    private static readonly HashSet<Type> Awaiters =
    [
        typeof(TaskAwaiter), typeof(TaskAwaiter<>), typeof(ValueTaskAwaiter), typeof(ValueTaskAwaiter<>),
        typeof(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter), typeof(ConfiguredTaskAwaitable<>.ConfiguredTaskAwaiter),
        typeof(ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter), typeof(ConfiguredValueTaskAwaitable<>.ConfiguredValueTaskAwaiter),
    ];

    /// <summary>
    /// <c>.Result</c> and <c>Wait*</c> always block: <c>await</c> never emits them. <c>GetResult()</c> is what <c>await</c> emits too, so inside
    /// an async method it counts as blocking only when there are more <c>GetAwaiter()</c> calls than awaiter <c>IsCompleted</c> checks
    /// (each compiler-generated await checks <c>IsCompleted</c> once; <c>.GetAwaiter().GetResult()</c> doesn't).
    /// </summary>
    private static bool Blocks(MethodBase method, bool insideAsync)
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

        int getAwaiter = 0, isCompleted = 0;
        var getResult = false;
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
            if (((declaring == typeof(Task<>) || declaring == typeof(ValueTask<>)) && called.Name == "get_Result")
                || (declaring == typeof(Task) && called.Name is "Wait" or "WaitAll" or "WaitAny"))
            {
                return true;
            }

            if (called.Name == "GetAwaiter")
            {
                getAwaiter++;
            }
            else if (called.Name == "get_IsCompleted" && (Awaiters.Contains(declaring) || declaring.Name.EndsWith("Awaiter", StringComparison.Ordinal)))
            {
                isCompleted++;
            }
            else if (called.Name == "GetResult" && Awaiters.Contains(declaring))
            {
                getResult = true;
            }
        }

        return getResult && (!insideAsync || getAwaiter > isCompleted);
    }
    private static class Offender
    {
        public static int Blocking() => Task.FromResult(1).GetAwaiter().GetResult();
    }

#pragma warning disable xUnit1031 // these block on purpose: they are what the scan must find
    private static class AsyncOffender
    {
        public static async Task<int> ResultInAsync()
        {
            await Task.Yield();
            return Task.FromResult(1).Result;
        }

        public static async Task<int> GetResultInAsync()
        {
            await Task.Delay(1);
            return Task.FromResult(1).GetAwaiter().GetResult();
        }

        public static async Task WaitAllInAsync()
        {
            await Task.Delay(1);
            Task.WaitAll(Task.CompletedTask);
        }

        public static async Task ConfiguredGetResultInAsync()
        {
            await Task.Delay(1).ConfigureAwait(false);
            Task.Delay(1).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task<int> ValueTaskResultInAsync()
        {
            await Task.Delay(1);
            return new ValueTask<int>(Task.FromResult(1)).Result;
        }

        public static async Task<int> OnlyAwaits()
        {
            await Task.Delay(1);
            await Task.Delay(1).ConfigureAwait(false);
            await new ValueTask(Task.CompletedTask);
            return await Task.FromResult(1);
        }
    }
#pragma warning restore xUnit1031
}
