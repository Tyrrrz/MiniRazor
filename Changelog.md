### v2.1.3 (19-Jul-2021)

- [Codegen] Hint names for generated files now end with `.g.cs` to follow the generally accepted conventions. By extension, this also fixes an issue which caused Coverlet to fail when instrumenting assemblies that used MiniRazor's compile-time generation.
- [Codegen] Added `Microsoft.CodeAnalysis.CSharp` assembly file inside the package for better compatibility.
- [Compiler] Fixed an issue where calling `Razor.Compile(...)` threw an exception when it failed to load a transitive assembly.
- [Compiler] Changed `accessModifier` parameter in `Razor.Transpile(...)` to nullable and optional.
- Added `netcoreapp3.0` and `netstandard2.1` targets to MiniRazor meta package to match the targets available for MiniRazor.Compiler.
- Added readme file to the package.

### v2.1.2 (23-Mar-2021)

- [CodeGen] Fixed an issue where MiniRazor.CodeGen was incorrectly installed as development dependency by default.

### v2.1.1 (23-Mar-2021)

- [CodeGen] Fixed an issue where the source generator didn't work because the package was missing some of the embedded binaries.

### v2.1 (23-Mar-2021)

- Split MiniRazor package into additional specialized packages: (Thanks [@Kevin Gliewe](https://github.com/KevinGliewe))
  - MiniRazor.Runtime, which contains runtime types required by MiniRazor templates. You shouldn't ever need to reference this package yourself.
  - MiniRazor.Compiler, which contains methods to compile Razor templates into IL. You can reference this package if you're only using MiniRazor for runtime template compilation.
  - MiniRazor.CodeGen, which contains source generator that converts Razor templates into C# classes. You can reference this package if you're only using MiniRazor for build-time template compilation.
  - MiniRazor, which is now just a meta package that links both MiniRazor.Compiler and MiniRazor.CodeGen. You can reference this package if you're using MiniRazor for both runtime and build-time template compilation.
- [CodeGen] Added `AccessModifier` property that can be specified on `AdditionalFiles` to configure access modifier used for generated code. Default access modifier is `internal`. (Thanks [@TheJayMann](https://github.com/TheJayMann))
- [CodeGen] Fixed an issue where adding two templates with same name but different namespaces resulted in a compilation error. (Thanks [@TheJayMann](https://github.com/TheJayMann))

### v2.0.3 (15-Dec-2020)

- Fixed more issues in generated code.

### v2.0.2 (15-Dec-2020)

- Added `#nullable disable` to generated code.

### v2.0.1 (15-Dec-2020)

- Fixed a build error that occurred when attempting to use MiniRazor's source generator.

### v2.0 (15-Dec-2020)

- Reworked the library. Please refer to the readme for updated usage instructions.
- Implemented support for build time template compilation via C# source generators.

### v1.1 (05-Aug-2020)

- Added assembly unloading.

### v1.0.1 (29-Jul-2020)

- Fixed an issue where the parent assembly was not added to the template assembly metadata references.