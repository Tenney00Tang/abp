﻿using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.VirtualFileSystem;
using Volo.Abp.VirtualFileSystem;

namespace Microsoft.AspNetCore.Builder
{
    public static class VirtualFileSystemApplicationBuilderExtensions
    {
        public static void UseVirtualFiles(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<VirtualFileSystemOptions>>().Value;

            IFileProvider fileProvider = new AspNetCoreVirtualFileProvider(
                app.ApplicationServices,
                "/wwwroot"
            );

            if (options.FileSets.PhysicalPaths.Any())
            {
                var fileProviders = options.FileSets.PhysicalPaths
                    .Select(p => new PhysicalFileProvider(p))
                    .Cast<IFileProvider>()
                    .ToList();

                fileProviders.Add(fileProvider);

                fileProvider = new CompositeFileProvider(fileProviders);
            }

            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = fileProvider
                }
            );
        }
    }
}
