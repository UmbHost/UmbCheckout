using System.Text;
using System.Text.RegularExpressions;

namespace UmbCheckout.Testing;

/// <summary>
/// Reads the production C# source (not the test projects, test helpers or test sites) for the architecture tests that scan it, e.g. that
/// nothing hides a possible null with the null-forgiving operator.
/// </summary>
public static class ProductionSource
{
    // A "!" straight after an identifier, ")", "]" or ">" is the null-forgiving operator whatever follows it (".", "(", "[", ";", an
    // operator, ":", a line end...), except "!=" (not equal). A logical not is never directly after one of those characters.
    // "null !" and "default !" are caught with a space too. A ">" counts only when it is not a lambda arrow ("x=>!y" is a logical not).
    private static readonly Regex NullForgivingOperator = new(
        @"\b(?:null|default)\s*!(?!=)|(?<=[\w\)\]]|(?<!=)>)!(?!=)",
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
            else if ((c == '$' || c == '@') && TryInterpolated(source, ref i, result))
            {
                // An interpolated string: its text is blanked, the code in its holes is kept.
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
            else if (c == '"' || (c == '@' && next == '"'))
            {
                var verbatim = c == '@';
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

    /// <summary>
    /// An interpolated string starting at <paramref name="i"/> (<c>$"…"</c>, <c>$@"…"</c>, <c>@$"…"</c> or a raw <c>$"""…"""</c>, with any number
    /// of <c>$</c>): its literal text is blanked, but the code in its holes (<c>{x!.Y}</c>) is kept, as code. False if there is no
    /// interpolated string here.
    /// </summary>
    private static bool TryInterpolated(string source, ref int i, StringBuilder result)
    {
        var j = i;
        var dollars = 0;
        var verbatim = false;
        while (j < source.Length && (source[j] == '$' || source[j] == '@'))
        {
            if (source[j] == '$')
            {
                dollars++;
            }
            else
            {
                verbatim = true;
            }

            j++;
        }

        if (dollars == 0 || j >= source.Length || source[j] != '"')
        {
            return false;
        }

        var quotes = 0;
        while (j + quotes < source.Length && source[j + quotes] == '"')
        {
            quotes++;
        }

        var raw = quotes >= 3;
        var closing = new string('"', raw ? quotes : 1);
        var k = j + (raw ? quotes : 1);
        Blank(source, i, k, result);
        while (k < source.Length)
        {
            if (string.CompareOrdinal(source, k, closing, 0, closing.Length) == 0 && !(verbatim && !raw && k + 1 < source.Length && source[k + 1] == '"'))
            {
                Blank(source, k, k + closing.Length, result);
                k += closing.Length;
                break;
            }

            if (verbatim && !raw && source[k] == '"')
            {
                Blank(source, k, k + 2, result);
                k += 2;
            }
            else if (!verbatim && !raw && source[k] == '\\')
            {
                Blank(source, k, k + 2, result);
                k += 2;
            }
            else if (source[k] == '{')
            {
                var run = 0;
                while (k + run < source.Length && source[k + run] == '{')
                {
                    run++;
                }

                // Regular and verbatim strings: "{{" is a literal brace, so an odd run ends in a hole. Raw strings: a run of fewer braces
                // than the string has "$" is literal text; otherwise the last "$"-many braces open a hole.
                var braces = raw ? dollars : 1;
                var hole = raw ? run >= dollars : run % 2 == 1;
                var literal = hole ? run - braces : run;
                Blank(source, k, k + literal, result);
                k += literal;
                if (hole)
                {
                    Blank(source, k, k + braces, result);
                    k += braces;
                    k = Hole(source, k, braces, result);
                }
            }
            else
            {
                Blank(source, k, k + 1, result);
                k++;
            }
        }

        i = Math.Min(k, source.Length);
        return true;
    }

    /// <summary>The code of one interpolation hole starting at <paramref name="start"/>, appended as code; returns where the hole's closing braces end.</summary>
    private static int Hole(string source, int start, int closingBraces, StringBuilder result)
    {
        var depth = 0;
        var nesting = 0;
        var formatAt = -1;
        var k = start;
        while (k < source.Length)
        {
            var c = source[k];
            if (c == '"' || c == '\'')
            {
                // A string or character literal inside the hole: skipped as a unit (its braces and colons are not the hole's).
                if (c == '"')
                {
                    k = SkipLiteral(source, k);
                }
                else
                {
                    k++;
                    while (k < source.Length && source[k] != '\'' && source[k] != '\n')
                    {
                        k += source[k] == '\\' ? 2 : 1;
                    }

                    k = Math.Min(k + 1, source.Length);
                }

                continue;
            }

            if (c == '(' || c == '[')
            {
                nesting++;
            }
            else if (c == ')' || c == ']')
            {
                nesting--;
            }
            else if (c == '{')
            {
                depth++;
            }
            else if (c == '}' && depth > 0)
            {
                depth--;
            }
            else if (c == '}')
            {
                break;
            }
            else if (c == ':' && depth == 0 && nesting == 0 && formatAt < 0 && !(k + 1 < source.Length && source[k + 1] == ':'))
            {
                formatAt = k;
            }

            k++;
        }

        var codeEnd = formatAt >= 0 ? formatAt : k;
        result.Append(CodeOnly(source.Substring(start, codeEnd - start)));
        Blank(source, codeEnd, Math.Min(k + closingBraces, source.Length), result);
        return Math.Min(k + closingBraces, source.Length);
    }

    /// <summary>The end of the regular string literal that starts at <paramref name="start"/> (a hole's nested string).</summary>
    private static int SkipLiteral(string source, int start)
    {
        var k = start + 1;
        while (k < source.Length)
        {
            if (source[k] == '\\')
            {
                k += 2;
            }
            else if (source[k] == '"')
            {
                return k + 1;
            }
            else
            {
                k++;
            }
        }

        return source.Length;
    }

    private static void Blank(string source, int start, int end, StringBuilder result)
    {
        for (var i = start; i < end && i < source.Length; i++)
        {
            result.Append(source[i] == '\n' ? '\n' : ' ');
        }
    }
}
