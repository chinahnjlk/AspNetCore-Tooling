//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.ComponentModel.Composition;
//using System.Linq;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Text;
//using Microsoft.VisualStudio.Editor.Razor;
//using Microsoft.VisualStudio.Text;
//using Microsoft.VisualStudio.Text.Projection;

//namespace Microsoft.VisualStudio.LanguageServices.Razor
//{
//    [System.Composition.Shared]
//    [Export(typeof(VisualStudioWorkspaceAccessor))]
//    internal class DefaultVisualStudioWorkspaceAccessor : VisualStudioWorkspaceAccessor
//    {
//        private readonly IBufferGraphFactoryService _bufferGraphService;

//        [ImportingConstructor]
//        public DefaultVisualStudioWorkspaceAccessor(IBufferGraphFactoryService bufferGraphService)
//        {
//            if (bufferGraphService == null)
//            {
//                throw new ArgumentNullException(nameof(bufferGraphService));
//            }

//            _bufferGraphService = bufferGraphService;
//        }

//        public override Workspace GetWorkspace(ITextBuffer textBuffer)
//        {
//            if (textBuffer == null)
//            {
//                throw new ArgumentNullException(nameof(textBuffer));
//            }

//            var graph = _bufferGraphService.CreateBufferGraph(textBuffer);
//            var projectedCSharpBuffer = graph.GetTextBuffers(buffer => buffer.ContentType.IsOfType("CSharp")).FirstOrDefault();

//            if (projectedCSharpBuffer == null)
//            {
//                throw new InvalidOperationException("A CSharp projection buffer has not been associated with the Razor buffer.");
//            }

//            var workspace = projectedCSharpBuffer.GetWorkspace();
//            return workspace;
//        }
//    }
//}
