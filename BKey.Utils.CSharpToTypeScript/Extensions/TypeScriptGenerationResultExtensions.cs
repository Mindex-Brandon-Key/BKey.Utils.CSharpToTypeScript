using BKey.Utils.CSharpToTypeScript.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKey.Utils.CSharpToTypeScript.Extensions;
public static class TypeScriptGenerationResultExtensions
{
    public static string ToSingleFile(this TypeScriptGenerationResult result)
    {
        return string.Join("\n\n", result.Files.Select(x => x.Content));
    }

    public static TypeScriptGenerationResult Save(this TypeScriptGenerationResult result, string path = ".")
    {
        foreach (var file in result.Files)
        {
            if (!Path.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using var stream = File.Create(Path.Join(path, file.FileName));
            using var writer = new StreamWriter(stream);
            writer.Write(file.Content);
        }
        return result;
    }

    public static async Task<TypeScriptGenerationResult> SaveAsync(this TypeScriptGenerationResult result, string path = ".")
    {
        var tasks = result.Files.Select(async file =>
            {
                if (!Path.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using var stream = File.Create(Path.Join(path, file.FileName));
                using var writer = new StreamWriter(stream);
                await writer.WriteAsync(file.Content);
            });
        await Task.WhenAll(tasks);
        return result;
    }
}
