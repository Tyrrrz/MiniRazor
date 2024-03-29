# MiniRazor

[![Status](https://img.shields.io/badge/status-discontinued-e4181c.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://vshymanskyy.github.io/StandWithUkraine)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/MiniRazor/main.yml?branch=master)](https://github.com/Tyrrrz/MiniRazor/actions)
[![Coverage](https://img.shields.io/codecov/c/github/Tyrrrz/MiniRazor/master)](https://codecov.io/gh/Tyrrrz/MiniRazor)
[![Version](https://img.shields.io/nuget/v/MiniRazor.svg)](https://nuget.org/packages/MiniRazor)
[![Downloads](https://img.shields.io/nuget/dt/MiniRazor.Runtime.svg)](https://nuget.org/packages/MiniRazor.Runtime)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

> **Note**:
> As an alternative, consider using [RazorBlade](https://github.com/ltrzesniewski/RazorBlade) for build-time template compilation and [RazorLight](https://github.com/toddams/RazorLight) for run-time template compilation.

**MiniRazor** is a tiny abstraction over the Razor engine, designed to provide a simple interface to compile and render templates, both during build and at run-time.

## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me). Glory to Ukraine! 🇺🇦

## Install

All-in-one meta package:

- 📦 [NuGet](https://nuget.org/packages/MiniRazor): `dotnet add package MiniRazor`

Specialized packages:

- 📦 [NuGet](https://nuget.org/packages/MiniRazor.Compiler): `dotnet add package MiniRazor.Compiler` (**run-time compilation** only)
- 📦 [NuGet](https://nuget.org/packages/MiniRazor.CodeGen): `dotnet add package MiniRazor.CodeGen` (**build-time compilation** only)

> ⚠ If you're referencing **MiniRazor.CodeGen**, ensure that it's **NOT** marked as a development dependency!
> Although the source generator assembly itself is only used during build, this package also contains binaries which are required by the generated code at run-time.

## Usage

### Compiling templates at build-time

> **Warning**:
> Compiling at build time requires [MiniRazor](#install) or [MiniRazor.CodeGen](#install).

**MiniRazor** comes with a source generator that can parse Razor templates and transpile them into C# classes directly at build time.
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
  If this directive is not included, the template will inherit from `MiniRazor.TemplateBase<dynamic>` instead — providing no type-safety when working with the model.
- `@namespace` directive instructs the compiler to put the generated template class into the `MyNamespace.Templates` namespace.
  If this directive is not included, the default namespace of `MiniRazor.GeneratedTemplates` will be used instead.

In order to make the template accessible by **MiniRazor**'s source generator, you need to add it to the project using the `AdditionalFiles` element and mark it with the `IsRazorTemplate="true"` attribute:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Include a single template -->
    <AdditionalFiles Include="Templates/TemplateFoo.cshtml" IsRazorTemplate="true" />

    <!-- Include multiple templates at once -->
    <AdditionalFiles Include="Templates/*.cshtml" IsRazorTemplate="true" />
  </ItemGroup>

  <!-- ... -->

</Project>
```

After that, you should be able to run `dotnet build` to trigger the source generator and generate the corresponding template classes.
The template from the above example will then become available as the `MyNamespace.Templates.TemplateFoo` class.
To render it, you can call its `RenderAsync(...)` static method:

```csharp
// Reference the namespace where the template is located
using MyNamespace.Templates;

// Render the template to a string, with @Model set to "world"
var output = await TemplateFoo.RenderAsync("world");

// Or, alternatively, render it to the specified TextWriter
await TemplateFoo.RenderAsync(Console.Out, "world");
```

Note that the type of the `model` parameter in `RenderAsync(...)` is automatically inferred based on the `@inherits` directive specified in the template.
Here, since the template is derived from `MiniRazor.TemplateBase<string>`, the method expects a parameter of type `string`.

### Compiling templates at run-time

> **Warning**:
> Compiling at run-time requires [MiniRazor](#install) or [MiniRazor.Compiler](#install).

If the previous approach doesn't fit your usage scenario, you can also compile templates at run-time.
To do that, call `Razor.Compile(...)` with the template's source code:

```csharp
using MiniRazor;

// Compile the template into an in-memory assembly
var template = Razor.Compile("<p>Hello, @Model.Subject!</p>");

// Render the template to a string
var output = await template.RenderAsync(new MyModel { Subject = "World" });
// <p>Hello, World!</p>
```

Calling `Razor.Compile(...)` transforms the provided Razor template directly into IL code hosted in a dynamic in-memory assembly.
This returns an instance of `TemplateDescriptor`, which you can then use to render output.

By default, **MiniRazor** uses the default assembly load context, which means that the compiled IL code will stay in memory forever.
To avoid that, you can pass a custom instance of `AssemblyLoadContext` that lets you control the lifetime of the generated assemblies:

```csharp
// Create an isolated assembly load context
var alc = new AssemblyLoadContext("MyALC", true);

// Compile the template
var template = Razor.Compile("<p>Hello, @Model.Subject!</p>", alc);

// Unload the ALC once it's no longer needed
alc.Unload();
```

### Templating features

#### HTML encoding

Output rendered with Razor templates is HTML-encoded by default.
If you want to print raw HTML content, for example if it's sourced from somewhere else, you can use the `Raw(...)` method:

```razor
@{
    string GetHtml() => "<p>Hello world!</p>";
}

@GetHtml() // &lt;p&gt;Hello world!&lt;/p&gt;
@Raw(GetHtml()) // <p>Hello world!</p>
```
