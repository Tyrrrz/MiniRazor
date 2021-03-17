### v2.1.0 (16-Mar-2021)

- Change packages to prevent `Microsoft.AspNetCore.Razor.Language` from being a dependency for *compile time generation*. Fixes this [issue](https://github.com/Tyrrrz/MiniRazor/issues/6)
- Better namespace reconstruction.

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