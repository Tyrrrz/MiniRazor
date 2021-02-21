# MiniRazor

[![Build](https://github.com/Tyrrrz/MiniRazor/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/MiniRazor/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/MiniRazor/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/MiniRazor)
[![Version](https://img.shields.io/nuget/v/MiniRazor.svg)](https://nuget.org/packages/MiniRazor)
[![Downloads](https://img.shields.io/nuget/dt/MiniRazor.svg)](https://nuget.org/packages/MiniRazor)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

⚠️ **Project status: maintenance mode** (bug fixes only).

MiniRazor is a tiny abstraction over the Razor engine, designed to provide a simple interface to compile and render templates, both at build time and at run time.

## Download

📦 [NuGet](https://nuget.org/packages/MiniRazor): `dotnet add package MiniRazor`

## Usage

### Compiling templates at build time

MiniRazor comes with a source generator that can parse Razor templates and transpile them into C# classes directly at build time.
This workflow is suitable and highly recommended for scenarios where your templates are not expected to change.

To do that, first create a Razor template as shown here:

```razor
@inherits MiniRazor.TemplateBase<string>
@namespace MyNamespace.Templates

<html>

<head>
    <title>Hello @Model</title>
</head>

<body>
    <p>Hello @Model</p>
</body>

</html>
```

Note the usage of two important directives at the top of the file:

- `@inherits` directive indicates that the base type of this template is `MiniRazor.TemplateBase<TModel>`, with the model of type `string`.
  If this directive is not included, the template will instead inherit from `MiniRazor.TemplateBase<dynamic>` by default, which doesn't offer the same level of type-safety when working with the model.
  
- `@namespace` directive instructs the compiler to put the generated class into the `MyNamespace.Templates` namespace.
  If this directive is omitted, the default namespace of `MiniRazor.GeneratedTemplates` will be used instead.

In order to make the template accessible by MiniRazor's source generator, you need to add it to the project as `AdditionalFiles` and mark it with the `IsRazorTemplate="true"` attribute:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Include a single template -->
    <AdditionalFiles Include="Templates/TemplateFoo.cshtml" IsRazorTemplate="true" />
    
    <!-- Optional: Include multiple templates at once -->
    <AdditionalFiles Include="Templates/*.cshtml" IsRazorTemplate="true" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="MiniRazor" Version="x.x.x" />
  </ItemGroup>

</Project>
```

Once that's done, you should be able to run `dotnet build` to build the project and trigger the source generator.
Given that the template's file name is `TemplateFoo.cshtml`, the generated class should be accessible as `MyNamespace.Templates.TemplateFoo`.
To render it, you can call the `RenderAsync(...)` static method:

```csharp
// Reference the namespace where the template is located
using MyNamespace.Templates;

// Render the template to string, with @Model set to "world"
var output = await TemplateFoo.RenderAsync("world");

// Or, alternatively, render it to the specified TextWriter
await TemplateFoo.RenderAsync(Console.Out, "world");
```

Note that the type of the `model` parameter in `RenderAsync(...)` is automatically inferred from the `@inherits` directive specified in the template.
Here, since the template is derived from `MiniRazor.TemplateBase<string>`, the method expects a parameter of type `string`.

### Compiling templates at run time

If the previous approach doesn't fit your usage scenario, you can also compile templates at runtime.
To do that, call `Razor.Compile(...)` with the template's source code:

```csharp
// Compile the template into an in-memory assembly
var template = Razor.Compile("<p>Hello, @Model.Subject!</p>");

// Render the template to string
var output = await template.RenderAsync(new MyModel { Subject = "World" });
// <p>Hello, World!</p>
```

Calling `Razor.Compile(...)` transforms the provided Razor template directly into IL code and hosts it in a generated in-memory assembly.
This returns an instance of `TemplateDescriptor`, which can then be used to render output against a model.

By default, MiniRazor uses the default assembly load context, which means that, once compiled, the generated IL code will stay in memory forever.
To avoid that, you can pass a custom instance of `AssemblyLoadContext` to control the lifetime of generated assemblies:

```csharp
// Create an isolated assembly load context
var alc = new AssemblyLoadContext("MyALC", true);

// Compile the template
var template = Razor.Compile("<p>Hello, @Model.Subject!</p>", alc);

// Unload the ALC once it's no longer needed
alc.Unload();
```

### HTML encoding

Output rendered with Razor templates is HTML-encoded by default.
If you want to print raw HTML content, for example if it's sourced from somewhere else, you can use the `Raw(...)` method:

```razor
@{
    string GetHtml() => "<p>Hello world!</p>";
}

@GetHtml() // &lt;p&gt;Hello world!&lt;/p&gt; 
@Raw(GetHtml()) // <p>Hello world!</p>
```
