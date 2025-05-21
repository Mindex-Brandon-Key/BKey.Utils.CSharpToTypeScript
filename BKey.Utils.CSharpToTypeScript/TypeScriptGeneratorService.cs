using BKey.Utils.CSharpToTypeScript.Models;
using BKey.Utils.CSharpToTypeScript.NamingPolicies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BKey.Utils.CSharpToTypeScript;
public class TypeScriptGeneratorService
{
    private readonly JsonNamingPolicy _classNamingPolicy;
    private readonly JsonNamingPolicy _propertyNamingPolicy;
    private readonly JsonNamingPolicy _filenameNamingPolicy;

    public TypeScriptGeneratorService(
        JsonNamingPolicy? classNamingPolicy = null,
        JsonNamingPolicy? propertyNamingPolicy = null,
        JsonNamingPolicy? fileNamingPolicy = null
        )
    {
        _classNamingPolicy = classNamingPolicy ?? PascalCaseNamingPolicy.Instance;
        _propertyNamingPolicy = propertyNamingPolicy ?? JsonNamingPolicy.CamelCase;
        _filenameNamingPolicy = fileNamingPolicy ?? PascalCaseNamingPolicy.Instance;
    }

    /// <summary>
    /// Walks T and all referenced types, returning one file per type/enum.
    /// </summary>
    public TypeScriptGenerationResult Generate<T>() => Generate(typeof(T));

    public TypeScriptGenerationResult Generate(Type rootType)
    {
        HashSet<Type> visited = new();
        List<TypeScriptFile> files = new();

        ProcessType(visited, files, rootType);
        return new TypeScriptGenerationResult(files);
    }

    private void ProcessType(HashSet<Type> visited, List<TypeScriptFile> files, Type type)
    {
        if (visited.Contains(type) || IsSimple(type))
        {
            return;
        }

        visited.Add(type);

        var typeName = ToClassName(type);

        // Enums
        if (type.IsEnum)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"export enum {typeName} {{");
            foreach (var name in Enum.GetNames(type))
            {
                var raw = Convert.ToInt64(Enum.Parse(type, name));
                sb.AppendLine($"  {name} = {raw},");
            }
            sb.AppendLine("}");
            files.Add(new TypeScriptFile
            {
                ClrType = type,
                FileName = $"{_filenameNamingPolicy.ConvertName(typeName)}.model.ts",
                Content = sb.ToString()
            });
            return;
        }

        // Classes / Structs: first recurse into property types so dependencies come first
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() == null);
        foreach (var prop in props)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType)
                           ?? prop.PropertyType;
            if (!IsSimple(propType)) {
                ProcessType(visited, files, propType);
            }
        }

        // Now emit this type's file
        var builder = new StringBuilder();
        builder.AppendLine($"export class {typeName} {{");
        foreach (var prop in props)
        {
            var propertyName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? _propertyNamingPolicy.ConvertName(prop.Name);
            var tsType = ToTypeScriptType(prop.PropertyType);
            builder.AppendLine($"  {propertyName}: {tsType};");
        }
        builder.AppendLine("}");

        files.Add(new TypeScriptFile
        {
            ClrType = type,
            FileName = $"{_filenameNamingPolicy.ConvertName(type.Name)}.model.ts",
            Content = builder.ToString()
        });
    }

    private string ToTypeScriptType(Type t)
    {
        bool isNullable = false;
        var u = t;
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            isNullable = true;
            u = Nullable.GetUnderlyingType(t) ?? t;
        }

        string ts;
        if (u == typeof(string) || u == typeof(Guid))
        {
            ts = "string";
        }
        else if (u == typeof(bool))
        {
            ts = "boolean";
        }
        else if (u.IsEnum)
        {
            ts = u.Name;
        }
        else if (u == typeof(int) || u == typeof(long)
              || u == typeof(float) || u == typeof(double)
              || u == typeof(decimal) || u == typeof(short))
        {
            ts = "number";
        }
        else if (TryGetArrayType(u, out var arrayType))
        {
            ts = ToTypeScriptType(arrayType) + "[]";
        }
        else if (typeof(IEnumerable).IsAssignableFrom(u) && u.IsGenericType)
        {
            var elt = u.GetGenericArguments()[0];
            ts = ToTypeScriptType(elt) + "[]";
        }
        else
        {
            ts = ToClassName(u);
        }

        return isNullable ? $"{ts} | null" : ts;
    }

    private string ToClassName(Type type)
    {
        return _classNamingPolicy.ConvertName(type.Name)
                           ?? type.Name;
    }

    private static bool IsSimple(Type t)
    {
        var u = Nullable.GetUnderlyingType(t) ?? t;
        return u.IsPrimitive
            || u == typeof(string)
            || u == typeof(decimal)
            || u == typeof(DateTime)
            || u == typeof(Guid)
            || u.IsArray
            || typeof(IEnumerable).IsAssignableFrom(u);
    }

    private static bool TryGetArrayType(Type t, [NotNullWhen(true)] out Type? arrayType)
    {
        if (!t.IsArray)
        {
            arrayType = null;
            return false;
        }

        var elementType = t.GetElementType();
        if (elementType is not null)
        {
            arrayType = elementType;
            return true;
        }

        arrayType = null;
        return false;
    }
}
