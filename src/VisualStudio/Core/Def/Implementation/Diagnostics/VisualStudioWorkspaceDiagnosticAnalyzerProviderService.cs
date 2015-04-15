// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Diagnostics
{
    /// <summary>
    /// This service provides diagnostic analyzers from the analyzer assets specified in the manifest files of installed VSIX extensions.
    /// These analyzers are used across this workspace session.
    /// </summary>
    [Export(typeof(IWorkspaceDiagnosticAnalyzerProviderService))]
    internal class VisualStudioWorkspaceDiagnosticAnalyzerProviderService : IWorkspaceDiagnosticAnalyzerProviderService
    {
        private const string AnalyzerContentTypeName = "Microsoft.VisualStudio.Analyzer";

        private readonly ImmutableArray<HostDiagnosticAnalyzerPackage> _hostDiagnosticAnalyzerInfo;

        [ImportingConstructor]
        public VisualStudioWorkspaceDiagnosticAnalyzerProviderService(VisualStudioWorkspaceImpl workspace)
        {
            // Get the analyzer assets for installed VSIX extensions through the VSIX extension manager.
            var extensionManager = workspace.GetVsService<SVsExtensionManager, IVsExtensionManager>();

            // get rootfolder and shellfolder location
            string rootFolder;
            string shellFolder;
            if (TryGetRootAndShellFolder(extensionManager, out shellFolder, out rootFolder))
            {
                var builder = ImmutableArray.CreateBuilder<HostDiagnosticAnalyzerPackage>();
                foreach (var extension in extensionManager.GetEnabledExtensions(AnalyzerContentTypeName))
                {
                    var name = extension.Header.LocalizedName;
                    var assemblies = extension.Content.Where(ShouldInclude)
                                                      .Select(c => GetContentLocation(shellFolder, rootFolder, extension.InstallPath, c.RelativePath))
                                                      .WhereNotNull();

                    builder.Add(new HostDiagnosticAnalyzerPackage(name, assemblies.ToImmutableArray()));
                }

                _hostDiagnosticAnalyzerInfo = builder.ToImmutable();
                return;
            }

            // if we can't get package name, use old style.
            var references = ImmutableArray.CreateBuilder<string>();
            foreach (var reference in extensionManager.GetEnabledExtensionContentLocations(AnalyzerContentTypeName))
            {
                if (string.IsNullOrEmpty(reference))
                {
                    continue;
                }

                references.Add(reference);
            }

            _hostDiagnosticAnalyzerInfo = ImmutableArray.Create(new HostDiagnosticAnalyzerPackage(name: null, assemblies: references.ToImmutable()));
        }

        public IEnumerable<HostDiagnosticAnalyzerPackage> GetHostDiagnosticAnalyzerPackages()
        {
            return _hostDiagnosticAnalyzerInfo;
        }

        private bool TryGetRootAndShellFolder(IVsExtensionManager extensionManager, out string shellFolder, out string rootFolder)
        {
            // use reflection to get this information. currently there is no other way to get this information
            shellFolder = GetProperty(extensionManager, "ShellFolder");
            rootFolder = GetProperty(extensionManager, "RootFolder");

            return !string.IsNullOrEmpty(rootFolder) && !string.IsNullOrEmpty(shellFolder);
        }

        private string GetContentLocation(string shellFolder, string rootFolder, string installPath, string relativePath)
        {
            // extension manager should expose an API that doesnt require this.
            const string ShellFolderToken = "$ShellFolder$";
            const string RootFolderToken = "$RootFolder$";

            if (relativePath.StartsWith(ShellFolderToken))
            {
                return relativePath.Replace(ShellFolderToken, shellFolder);
            }
            else if (relativePath.StartsWith(RootFolderToken))
            {
                return relativePath.Replace(RootFolderToken, rootFolder);
            }

            string contentLocation = null;
            try
            {
                contentLocation = Path.Combine(installPath, relativePath);
            }
            //Path.Combine will throw an ArgumentException if either of the two path arguments contain illegal characters.
            //We'll just catch this exception here and ignore the paths with illegal characters.
            catch (ArgumentException)
            {
            }

            return contentLocation;
        }

        private string GetProperty(IVsExtensionManager extensionManager, string propertyName)
        {
            try
            {
                return (string)extensionManager.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(extensionManager);
            }
            catch
            {
                return null;
            }
        }

        private bool ShouldInclude(IExtensionContent content)
        {
            return string.Equals(content.ContentTypeName, AnalyzerContentTypeName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
