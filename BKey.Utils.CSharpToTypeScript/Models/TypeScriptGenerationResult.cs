using System.Collections.Generic;

namespace BKey.Utils.CSharpToTypeScript.Models;
public class TypeScriptGenerationResult
{
    public IReadOnlyList<TypeScriptFile> Files { get; }

    public TypeScriptGenerationResult(List<TypeScriptFile> files)
    {
        Files = files;
    }
}
