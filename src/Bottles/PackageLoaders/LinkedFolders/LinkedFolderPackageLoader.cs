using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using Bottles.Manifest;
using FubuCore;
using FubuCore.Descriptions;
using System.IO;

namespace Bottles.PackageLoaders.LinkedFolders
{
    /// <summary>
    /// To be used in other projects (ie FubuMVC) to allow additional packages to be loaded
    /// via the .links file.
    /// </summary>
    public class LinkedFolderPackageLoader : IPackageLoader, DescribesItself
    {
        private readonly string _applicationDirectory;
        private readonly IFileSystem _fileSystem = new FileSystem();
        private readonly PackageManifestReader _reader;

        public LinkedFolderPackageLoader(string applicationDirectory, Func<string, string> getContentFolderFromPackageFolder)
        {
            _applicationDirectory = applicationDirectory;
            _reader = new PackageManifestReader(_fileSystem, getContentFolderFromPackageFolder);
        }


        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            var packages = new List<IPackageInfo>();

            var manifestFile = _applicationDirectory.AppendPath(LinkManifest.FILE);
            var manifest = _fileSystem.LoadFromFile<LinkManifest>(manifestFile);
            if (manifest == null)
            {
                log.Trace("No package manifest found at {0}", manifestFile);
                return packages;
            }

            if (manifest.LinkedFolders.Any())
            {
                log.Trace("Loading linked folders via the package manifest at " + _applicationDirectory);
                manifest.LinkedFolders.Each(folder =>
                {
                    if (Platform.IsUnix()) {
                        folder = folder.Replace('\\', Path.DirectorySeparatorChar);
                    }

                    var linkedFolder = FileSystem.Combine(_applicationDirectory, folder).ToFullPath();
                    log.Trace("  - linking folder " + linkedFolder);

                    var package = _reader.LoadFromFolder(linkedFolder);
                    packages.Add(package);
                });
            }
            else
            {
                log.Trace("No linked folders found in the package manifest file at " + _applicationDirectory);
            }

            return packages;
        }

        public void Describe(Description description)
        {
            description.Title = "Linked Folder";
            description.ShortDescription = "Link to " + _applicationDirectory;
        }
    }
}