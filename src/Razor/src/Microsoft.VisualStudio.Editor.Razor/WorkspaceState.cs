// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [System.Composition.Shared]
    [Export(typeof(WorkspaceStateFactory))]
    internal class DefaultWorkspaceStateFactory : WorkspaceStateFactory
    {
        public override WorkspaceState Create(Workspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            var languageServices = workspace.Services.GetLanguageServices(RazorLanguage.Name);
            var projectSnapshotManager = languageServices.GetRequiredService<ProjectSnapshotManager>();
            var importDocumentManager = languageServices.GetRequiredService<ImportDocumentManager>();
            var workspaceState = new DefaultWorkspaceState(
                workspace,
                projectSnapshotManager,
                importDocumentManager);

            return workspaceState;
        }
    }

    internal abstract class WorkspaceStateFactory
    {
        public abstract WorkspaceState Create(Workspace workspace);
    }

    internal class DefaultWorkspaceState : WorkspaceState
    {
        public DefaultWorkspaceState(
            Workspace workspace,
            ProjectSnapshotManager projectSnapshotManager,
            ImportDocumentManager importDocumentManager)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (projectSnapshotManager == null)
            {
                throw new ArgumentNullException(nameof(projectSnapshotManager));
            }

            if (importDocumentManager == null)
            {
                throw new ArgumentNullException(nameof(importDocumentManager));
            }

            Workspace = workspace;
            ProjectSnapshotManager = projectSnapshotManager;
            ImportDocumentManager = importDocumentManager;
        }

        public override Workspace Workspace { get; }

        public override ProjectSnapshotManager ProjectSnapshotManager { get; }

        public override ImportDocumentManager ImportDocumentManager { get; }
    }

    internal abstract class WorkspaceState
    {
        public abstract Workspace Workspace { get; }

        public abstract ProjectSnapshotManager ProjectSnapshotManager { get; }

        public abstract ImportDocumentManager ImportDocumentManager { get; }
    }
}
