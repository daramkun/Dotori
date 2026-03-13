using System.Text;

namespace Dotori.Core.Model;

/// <summary>
/// Expands <c>${VAR}</c> references in DSL string values using the current environment.
/// Unknown variables expand to an empty string.
/// Unclosed <c>${</c> is treated as a literal.
/// </summary>
public static class EnvExpander
{
    /// <summary>Expand all <c>${VAR}</c> occurrences in <paramref name="input"/>.</summary>
    public static string Expand(string input)
    {
        var start = input.IndexOf("${", StringComparison.Ordinal);
        if (start < 0) return input;

        var sb = new StringBuilder(input.Length + 16);
        var i = 0;

        while (i < input.Length)
        {
            if (i + 1 < input.Length && input[i] == '$' && input[i + 1] == '{')
            {
                var end = input.IndexOf('}', i + 2);
                if (end < 0)
                {
                    // Unclosed ${ — treat the rest as literal
                    sb.Append(input.AsSpan(i));
                    break;
                }

                var varName = input.Substring(i + 2, end - i - 2);
                sb.Append(Environment.GetEnvironmentVariable(varName) ?? string.Empty);
                i = end + 1;
            }
            else
            {
                sb.Append(input[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    /// <summary>Expand a nullable string; returns <c>null</c> unchanged.</summary>
    public static string? ExpandNullable(string? input) =>
        input is null ? null : Expand(input);
}
