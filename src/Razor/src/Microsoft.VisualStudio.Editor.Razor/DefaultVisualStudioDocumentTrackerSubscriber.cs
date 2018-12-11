// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [System.Composition.Shared]
    [Export(typeof(VisualStudioDocumentTrackerSubscriber))]
    internal class DefaultVisualStudioDocumentTrackerSubscriber : VisualStudioDocumentTrackerSubscriber
    {
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly IBufferGraphFactoryService _bufferGraphService;
        private readonly WorkspaceStateFactory _workspaceStateFactory;

        [ImportingConstructor]
        public DefaultVisualStudioDocumentTrackerSubscriber(
            ForegroundDispatcher foregroundDispatcher,
            IBufferGraphFactoryService bufferGraphService,
            WorkspaceStateFactory workspaceStateFactory)
        {
            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (bufferGraphService == null)
            {
                throw new ArgumentNullException(nameof(bufferGraphService));
            }

            if (workspaceStateFactory == null)
            {
                throw new ArgumentNullException(nameof(workspaceStateFactory));
            }

            _foregroundDispatcher = foregroundDispatcher;
            _bufferGraphService = bufferGraphService;
            _workspaceStateFactory = workspaceStateFactory;
        }

        public override void Unsubscribe(DefaultVisualStudioDocumentTracker documentTracker)
        {
            if (documentTracker == null)
            {
                throw new ArgumentNullException(nameof(documentTracker));
            }

            _foregroundDispatcher.AssertForegroundThread();

            documentTracker.Unsubscribe();
        }

        public override void Subscribe(DefaultVisualStudioDocumentTracker documentTracker)
        {
            if (documentTracker == null)
            {
                throw new ArgumentNullException(nameof(documentTracker));
            }

            _foregroundDispatcher.AssertForegroundThread();

            var graph = documentTracker.TextViews[0].BufferGraph;
            var candidateBuffers = graph.GetTextBuffers(IsCandidateBuffer);
            if (TrySubscribeDocumentTracker(documentTracker, candidateBuffers))
            {
                return;
            }

            // Things have not initialized all the way yet, we need to wait for buffers to be initialized to properly subscribe.

            EventHandler<GraphBuffersChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                var candidates = args.AddedBuffers.Where(IsCandidateBuffer).ToList();
                if (TrySubscribeDocumentTracker(documentTracker, candidates))
                {
                    graph.GraphBuffersChanged -= handler;
                }
            };

            //GC.KeepAlive(graph);
            GC.KeepAlive(handler);

            graph.GraphBuffersChanged += handler;
        }

        private static bool IsCandidateBuffer(ITextBuffer textBuffer) => textBuffer.ContentType.IsOfType("CSharp");

        private bool TrySubscribeDocumentTracker(DefaultVisualStudioDocumentTracker documentTracker, IReadOnlyCollection<ITextBuffer> candidateBuffers)
        {
            _foregroundDispatcher.AssertForegroundThread();

            if (candidateBuffers.Count == 0)
            {
                return false;
            }

            var unregisteredBuffers = new List<ITextBuffer>();
            foreach (var candidateBuffer in candidateBuffers)
            {
                var workspace = candidateBuffer.GetWorkspace();
                if (workspace == null)
                {
                    unregisteredBuffers.Add(candidateBuffer);
                    continue;
                }

                var workspaceState = _workspaceStateFactory.Create(workspace);
                documentTracker.Subscribe(workspaceState);
                return true;
            }

            if (unregisteredBuffers.Count == 0)
            {
                return false;
            }

            // TODO: Track handlers to unregister
            foreach (var unregisteredBuffer in unregisteredBuffers)
            {
                // Unregistered buffer is a C# buffer whos .GetWorkspace() returned null
                var bufferContainer = unregisteredBuffer.AsTextContainer();
                var registration = Workspace.GetWorkspaceRegistration(bufferContainer);
                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    // This is never called
                    var workspace = unregisteredBuffer.GetWorkspace();
                    if (workspace == null)
                    {
                        return;
                    }

                    registration.WorkspaceChanged -= handler;

                    var workspaceState = _workspaceStateFactory.Create(workspace);
                    documentTracker.Subscribe(workspaceState);
                };
                GC.KeepAlive(registration);
                registration.WorkspaceChanged += handler;
            }

            return false;
        }
    }
}
