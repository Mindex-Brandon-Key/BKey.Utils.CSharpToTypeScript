using System;

namespace BKey.Utils.CSharpToTypeScript.Models;
public class TypeScriptFile
{
    /// <summary>
    /// The CLR type this file was generated from.
    /// </summary>
    public required Type ClrType { get; init; }

    /// <summary>
    /// The suggested file name (e.g. "Person.ts").
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The TypeScript code for this type (enum or class).
    /// </summary>
    public required string Content { get; init; }
}