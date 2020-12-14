using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace MiniRazor
{
    public static partial class Razor
    {
        [ExcludeFromCodeCoverage]
        private class NotFoundProjectItem : RazorProjectItem
        {
            public override string BasePath { get; }

            public override string FilePath { get; }

            public override string FileKind { get; }

            public override bool Exists => false;

            public override string PhysicalPath => throw new NotSupportedException();

            public NotFoundProjectItem(string basePath, string path, string? fileKind)
            {
                BasePath = basePath;
                FilePath = path;
                FileKind = fileKind ?? FileKinds.GetFileKindFromFilePath(path);
            }

            public override Stream Read() => throw new NotSupportedException();
        }

        [ExcludeFromCodeCoverage]
        private class EmptyRazorProjectFileSystem : RazorProjectFileSystem
        {
            public static EmptyRazorProjectFileSystem Instance { get; } = new EmptyRazorProjectFileSystem();

            public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath) =>
                Enumerable.Empty<RazorProjectItem>();

            [Obsolete("Use GetItem(string path, string fileKind) instead.")]
            public override RazorProjectItem GetItem(string path) =>
                GetItem(path, null);

            public override RazorProjectItem GetItem(string path, string? fileKind) =>
                new NotFoundProjectItem(string.Empty, path, fileKind);
        }
    }
}