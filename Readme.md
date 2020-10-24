# MiniRazor

[![Build](https://github.com/Tyrrrz/MiniRazor/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/MiniRazor/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/MiniRazor/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/MiniRazor)
[![Version](https://img.shields.io/nuget/v/MiniRazor.svg)](https://nuget.org/packages/MiniRazor)
[![Downloads](https://img.shields.io/nuget/dt/MiniRazor.svg)](https://nuget.org/packages/MiniRazor)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

**Project status: maintenance mode** (bug fixes only).

MiniRazor is a tiny wrapper around the Razor templating engine, which provides a way to compile and render templates on demand. This library focuses specifically on providing the lightest possible implementation that can be used in console, desktop, or other non-web applications.

## Download

- [NuGet](https://nuget.org/packages/MiniRazor): `dotnet add package MiniRazor`

## Features

- Easy to use, no need to configure Roslyn and Razor yourself
- Supports all C# features, including `async`/`await`, local functions, and more
- Supports internal types references within templates
- Supports dynamic, anonymous, or statically-defined models
- Uses an isolated assembly context for compiled code
- No dependency on `Microsoft.AspNetCore.App` shared framework or runtime
- Works with .NET Standard 2.0+

## Usage

### Simple usage

The following example compiles a template and renders it using a model:

```csharp
var engine = new MiniRazorTemplateEngine();

// Compile template (you may want to cache this instance)
var template = engine.Compile("<p>Hello, @Model.Subject!</p>");

// Render template
var result = await template.RenderAsync(new MyModel { Subject = "World" });

// -- result:
// <p>Hello, World!</p>
```

The entry point, `MiniRazorTemplateEngine` is responsible for compiling Razor templates into IL code. Each compilation creates a new dynamic assembly, so it's strongly recommended to cache compiled templates as much as possible. How exactly you do this is up to you.

Once compiled, the template can be rendered as many times as needed. Keep in mind that the `RenderAsync` method is asynchronous as it needs to be able to render templates that contain asynchronous method invocations inside them.

### Anonymous model

You can also render an anonymous model as well:

```csharp
var result = await template.RenderAsync(new { Foo = "Bar" });
```

### Cleaning up

Under the hood, MiniRazor uses Roslyn to compile Razor templates into IL code. In doing so, it creates a dynamic assembly for each compiled template.

To avoid memory leaks, you will likely want to get rid of the generated assemblies once you're done with the templates. You can do so by calling `Dispose()` on the `MiniRazorTemplateEngine`:

```csharp
// Frees up resources used by compiled templates
engine.Dispose();
```

By disposing the engine, all templates compiled by that engine will become unusable.

_Note: this only works on projects targeting .NET Core 3.1+. On older frameworks that don't support assembly unloading, calling `Dispose()` will not do anything._

### Referencing internal types

Sometimes you may want to reference internal types in a template. Normally, since the template is compiled into a separate dynamic assembly in memory, it can't access internal types defined in other assemblies.

You can work around this, however, by using the `InternalsVisibleTo` attribute on the assembly that contains those internal types. Although, by default, assembly names are generated randomly, you can specify one yourself so that the template uses the same assembly name as the one referenced in the attribute.

```csharp
var template = engine.Compile("<p>@Model.Foo</p>", "RazorTemplateAssembly");

// ...
// Add this attribute to the assembly whose internal types you want to expose to the template
[assembly: InternalsVisibleTo("RazorTemplateAssembly")]
```

### IDE support

In order to have code completion inside a template, you need to let the IDE know what type of model it expects. In regular Razor templates you would do that via the `@model` directive, however with MiniRazor you need to use `@inherits` instead:

```razor
@inherits MiniRazor.MiniRazorTemplateBase<MyModel>

<p>Statically-typed model: @Model.Foo</p>
```

### HTML encoding

Output rendered with Razor templates is HTML-encoded by default. If you want to print raw HTML content, for example if it's sourced from somewhere else, you can use the `Raw()` method:

```razor
@{
    string GetHtml() => "<p>Hello world!</p>";
}

@GetHtml() // &lt;p&gt;Hello world!&lt;/p&gt; 
@Raw(GetHtml()) // <p>Hello world!</p>
```

### Parent assembly

The `MiniRazorTemplateEngine` object has a `ParentAssembly` property, which is used to determine the compilation context for the templates. Any assembly referenced by the parent assembly is also referenced by the template assembly.

By default, the parent assembly is resolved as the assembly that called the `MiniRazorTemplateEngine` constructor, but you can override this:

```csharp
var parent = Assembly.Load("...");
var engine = new MiniRazorTemplateEngine(parent);
```