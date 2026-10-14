using System.Text;
using System.Text.RegularExpressions;

namespace UmbCheckout.Testing;

/// <summary>
/// Reads the production C# source (not the test projects, test helpers or test sites) for the architecture tests that scan it, e.g. that
/// nothing hides a possible null with the null-forgiving operator.
/// </summary>
public static class ProductionSource
{
    private static readonly Regex NullForgivingOperator = new(
        @"\b(?:null|default)\s*!|(?<=[\w\)\]>])!(?=[.;,)\]\[]|\?\.|\s*$|\s*\?\?|\s+(?:is|as|and|or)\b|\s*\})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> SkippedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "node_modules", "packages", ".git", ".vs", ".idea", "UmbCheckout.Testing"
    };

    /// <summary>The directory above <paramref name="callerFilePath"/> that contains <paramref name="marker"/> (a file or folder name, or a relative path).</summary>
    public static string FindRoot(string callerFilePath, string marker)
    {
        for (var directory = Path.GetDirectoryName(callerFilePath); directory is not null; directory = Path.GetDirectoryName(directory))
        {
            var candidate = Path.Combine(directory, marker);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return directory;
            }
        }

        throw new DirectoryNotFoundException($"No directory above {callerFilePath} contains {marker}.");
    }

    /// <summary>Every production .cs file under <paramref name="root"/>: not in a test project (*.Tests, *.Testing, *.TestSite*) or a build output folder.</summary>
    public static IReadOnlyList<string> Files(string root)
    {
        var files = new List<string>();
        Collect(root, files);
        files.Sort(StringComparer.Ordinal);
        return files;
    }

    /// <summary>
    /// Where <c>null!</c>, <c>default!</c> or any other null-forgiving <c>!</c> is used in the production source under
    /// <paramref name="root"/>, as <c>relative path:line: code</c>. Comments and string literals are ignored.
    /// </summary>
    public static IReadOnlyList<string> NullForgiving(string root)
    {
        var offenders = new List<string>();
        foreach (var file in Files(root))
        {
            foreach (var (line, text) in NullForgivingIn(File.ReadAllText(file)))
            {
                offenders.Add($"{Path.GetRelativePath(root, file)}:{line}: {text}");
            }
        }

        return offenders;
    }

    /// <summary>The lines of <paramref name="source"/> that use the null-forgiving operator, with their line numbers.</summary>
    public static IReadOnlyList<(int Line, string Text)> NullForgivingIn(string source)
    {
        var original = source.Split('\n');
        var code = CodeOnly(source).Split('\n');
        var found = new List<(int, string)>();
        for (var i = 0; i < code.Length; i++)
        {
            if (NullForgivingOperator.IsMatch(code[i]))
            {
                found.Add((i + 1, original[i].Trim()));
            }
        }

        return found;
    }

    private static void Collect(string directory, List<string> files)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.cs"))
        {
            files.Add(file);
        }

        foreach (var child in Directory.EnumerateDirectories(directory))
        {
            var name = Path.GetFileName(child);
            if (SkippedDirectories.Contains(name)
                || name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("UmbCheckout.TestSite", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Collect(child, files);
        }
    }

    /// <summary>The source with comments and the contents of string and character literals blanked out; line breaks are kept.</summary>
    private static string CodeOnly(string source)
    {
        var result = new StringBuilder(source.Length);
        var i = 0;
        while (i < source.Length)
        {
            var c = source[i];
            var next = i + 1 < source.Length ? source[i + 1] : '\0';
            if (c == '/' && next == '/')
            {
                while (i < source.Length && source[i] != '\n')
                {
                    i++;
                }
            }
            else if (c == '/' && next == '*')
            {
                i += 2;
                while (i < source.Length && !(source[i] == '*' && i + 1 < source.Length && source[i + 1] == '/'))
                {
                    result.Append(source[i] == '\n' ? '\n' : ' ');
                    i++;
                }

                i += 2;
            }
            else if (c == '"' && next == '"' && i + 2 < source.Length && source[i + 2] == '"')
            {
                // Raw string literal: runs to the same number of closing quotes.
                var quotes = 0;
                while (i + quotes < source.Length && source[i + quotes] == '"')
                {
                    quotes++;
                }

                var closing = new string('"', quotes);
                var end = source.IndexOf(closing, i + quotes, StringComparison.Ordinal);
                end = end < 0 ? source.Length : end + quotes;
                Blank(source, i, end, result);
                i = end;
            }
            else if (c == '"' || (c == '@' && next == '"') || ((c == '$' || c == '@') && (next == '@' || next == '$') && i + 2 < source.Length && source[i + 2] == '"'))
            {
                var verbatim = c == '@' || next == '@';
                while (source[i] != '"')
                {
                    i++;
                }

                var start = i;
                i++;
                while (i < source.Length)
                {
                    if (verbatim && source[i] == '"' && i + 1 < source.Length && source[i + 1] == '"')
                    {
                        i += 2;
                    }
                    else if (!verbatim && source[i] == '\\')
                    {
                        i += 2;
                    }
                    else if (source[i] == '"')
                    {
                        i++;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

                Blank(source, start, Math.Min(i, source.Length), result);
            }
            else if (c == '$' && next == '"')
            {
                result.Append(' ');
                i++;
            }
            else if (c == '\'' && next != '\0')
            {
                var start = i;
                i++;
                while (i < source.Length && source[i] != '\'' && source[i] != '\n')
                {
                    i += source[i] == '\\' ? 2 : 1;
                }

                i = Math.Min(i + 1, source.Length);
                Blank(source, start, i, result);
            }
            else
            {
                result.Append(c);
                i++;
            }
        }

        return result.ToString();
    }

    private static void Blank(string source, int start, int end, StringBuilder result)
    {
        for (var i = start; i < end && i < source.Length; i++)
        {
            result.Append(source[i] == '\n' ? '\n' : ' ');
        }
    }
}
