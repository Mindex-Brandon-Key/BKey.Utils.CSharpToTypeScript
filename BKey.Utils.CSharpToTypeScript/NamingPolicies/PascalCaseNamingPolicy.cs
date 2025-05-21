using System.Text.Json;

namespace BKey.Utils.CSharpToTypeScript.NamingPolicies;
public sealed class PascalCaseNamingPolicy : JsonNamingPolicy
{
    public static PascalCaseNamingPolicy Instance { get; } = new PascalCaseNamingPolicy();

    public override string ConvertName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        static bool IsSeparator(char c) =>
            c == '_' || c == '-' || char.IsWhiteSpace(c);

        // Quick path for names with no separators: just uppercase first letter if needed
        bool hasSeparator = false;
        foreach (char c in name)
        {
            if (IsSeparator(c))
            {
                hasSeparator = true;
                break;
            }
        }
        if (!hasSeparator)
        {
            // Only change first char; leave the rest exactly as-is
            char first = name[0];
            if (char.IsLower(first))
                return char.ToUpperInvariant(first) + name.Substring(1);
            return name;
        }

        // Otherwise we have separators — strip them and PascalCase each segment
        int outputLen = 0;
        foreach (char c in name)
            if (!IsSeparator(c))
                outputLen++;

#if NETCOREAPP
        // Fast path: write directly into the string buffer
        return string.Create(outputLen, name, (span, original) =>
        {
            int dst = 0, srcLen = original.Length;
            for (int i = 0; i < srcLen;)
            {
                // skip any separators
                if (IsSeparator(original[i]))
                {
                    i++;
                    continue;
                }

                // scan to end of this segment
                int j = i;
                while (j < srcLen && !IsSeparator(original[j])) j++;

                // uppercase first char of segment
                span[dst++] = char.ToUpperInvariant(original[i]);

                // copy remainder of segment unchanged
                for (int k = i + 1; k < j; k++)
                    span[dst++] = original[k];

                i = j;
            }
        });
#else
        // Fallback for earlier frameworks
        var sb = new StringBuilder(outputLen);
        int len = name.Length;
        for (int i = 0; i < len; )
        {
            if (IsSeparator(name[i]))
            {
                i++;
                continue;
            }

            int j = i;
            while (j < len && !IsSeparator(name[j])) j++;

            // uppercase first char
            sb.Append(char.ToUpperInvariant(name[i]));
            // copy rest of segment unchanged
            if (j > i + 1)
                sb.Append(name.Substring(i + 1, j - i - 1));

            i = j;
        }
        return sb.ToString();
#endif
    }
}
