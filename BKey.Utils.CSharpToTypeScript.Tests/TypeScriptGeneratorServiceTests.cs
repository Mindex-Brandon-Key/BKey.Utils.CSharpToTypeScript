using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunit;

namespace BKey.Utils.CSharpToTypeScript.Tests;
public class TypeScriptGeneratorServiceTests
{
    private readonly TypeScriptGeneratorService _service;

    public TypeScriptGeneratorServiceTests()
    {
        _service = new TypeScriptGeneratorService();
    }

    [Fact]
    public void Generate_SimpleClass_ProducesSingleFile()
    {
        // Act
        var result = _service.Generate<SimpleModel>();

        // Assert
        Assert.Single(result.Files);
        var file = result.Files[0];
        Assert.Equal(typeof(SimpleModel), file.ClrType);
        Assert.Equal("SimpleModel.model.ts", file.FileName);
        Assert.Contains("export class SimpleModel", file.Content);
        Assert.Contains("name: string;", file.Content);
    }

    private class SimpleModel
    {
        public string? Name { get; set; }
    }
    private class NullableTypesModel
    {
        public int? Count { get; set; }
        public bool? IsActive { get; set; }
    }

    [Fact]
    public void Generate_NullableValueTypes_MapsToNullableTsTypes()
    {
        var result = _service.Generate<NullableTypesModel>();
        var file = Assert.Single(result.Files);

        Assert.Contains("count: number | null;", file.Content);
        Assert.Contains("isActive: boolean | null;", file.Content);
    }

    [Fact]
    public void Generate_NumericTypes_MapToNumber()
    {
        var result = _service.Generate<NumericModel>();
        var file = Assert.Single(result.Files);

        Assert.Contains("i: number;", file.Content);
        Assert.Contains("l: number;", file.Content);
        Assert.Contains("f: number;", file.Content);
        Assert.Contains("d: number;", file.Content);
        Assert.Contains("dec: number;", file.Content);
        Assert.Contains("s: number;", file.Content);
    }

    private class NumericModel
    {
        public int I { get; set; }
        public long L { get; set; }
        public float F { get; set; }
        public double D { get; set; }
        public decimal Dec { get; set; }
        public short S { get; set; }
    }

    [Fact]
    public void Generate_ArrayAndEnumerableProperties_MapsToTsArrays()
    {
        var result = _service.Generate<ArrayModelsHolder>();
        var file = Assert.Single(result.Files);

        Assert.Contains("values: number[];", file.Content);
        Assert.Contains("names: string[];", file.Content);
        Assert.Contains("ids: string[];", file.Content);
    }

    private class ArrayModelsHolder
    {
        public int[] Values { get; set; }
        public string[] Names { get; set; }
        public IEnumerable<Guid> Ids { get; set; }
    }

    [Fact]
    public void Generate_NestedClass_ProducesMultipleFilesInOrder()
    {
        // Act
        var result = _service.Generate<ParentModel>();

        // Assert two files: ChildModel then ParentModel
        Assert.Equal(2, result.Files.Count);
        Assert.Equal("ChildModel.model.ts", result.Files[0].FileName);
        Assert.Equal("ParentModel.model.ts", result.Files[1].FileName);

        // Content checks
        Assert.Contains("export class ChildModel", result.Files[0].Content);
        Assert.Contains("value: number;", result.Files[0].Content);
        Assert.Contains("export class ParentModel", result.Files[1].Content);
        Assert.Contains("child: ChildModel;", result.Files[1].Content);
    }

    private class ChildModel
    {
        public int Value { get; set; }
    }

    private class ParentModel
    {
        public ChildModel? Child { get; set; }
    }

    [Fact]
    public void Generate_Enum_ProducesEnumFile()
    {
        // Act
        var result = _service.Generate<Color>();

        // Assert
        Assert.Single(result.Files);
        var file = result.Files[0];
        Assert.Equal("Color.model.ts", file.FileName);
        Assert.Contains("export enum Color", file.Content);
        Assert.Contains("Red = 0", file.Content);
        Assert.Contains("Blue = 2", file.Content);
    }

    private enum Color
    {
        Red,
        Green,
        Blue
    }

    [Fact]
    public void Generate_CollectionProperty_MapsToArrayType()
    {
        // Act
        var result = _service.Generate<CollectionModel>();

        // Assert
        var file = Assert.Single(result.Files);
        Assert.Contains("items: string[];", file.Content);
    }

    private class CollectionModel
    {
        public List<string>? Items { get; set; }
    }

    [Fact]
    public void Generate_NullableProperty_MapsToNullableType()
    {
        // Act
        var result = _service.Generate<NullableModel>();

        // Assert
        var file = Assert.Single(result.Files);
        Assert.Contains("count: number | null;", file.Content);
    }

    private class NullableModel
    {
        public int? Count { get; set; }
    }

    [Fact]
    public void Generate_JsonPropertyName_ShouldOverride()
    {
        // Arrange
        var result = _service.Generate<JsonNameModel>();

        // Assert
        var file = Assert.Single(result.Files);
        Assert.Contains("STEVE: string;", file.Content);
    }

    private class JsonNameModel
    {
        [JsonPropertyName("STEVE")]
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Generate_JsonIgnore_ShouldIgnore()
    {
        // Arrange
        var result = _service.Generate<JsonIgnoreModel>();

        // Assert
        var file = Assert.Single(result.Files);
        Assert.Contains("name: string;", file.Content);
        Assert.DoesNotContain("name2", file.Content);
    }

    private class JsonIgnoreModel
    {
        public string Name { get; set; } = string.Empty;
        [JsonIgnore]
        public string Name2 { get; set; } = string.Empty;
    }
}
