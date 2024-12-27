// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Microsoft.Azure.SignalR.Management.ClientInvocation
{
    internal class DummyRoutedClientResultsManager : IRoutedClientResultsManager
    {
        public void AddInvocation(string connectionId, string invocationId, string callerServerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void CleanupInvocationsByConnection(string connectionId)
        {
            throw new NotImplementedException();
        }

        public bool TryCompleteResult(string connectionId, CompletionMessage message)
        {
            throw new NotImplementedException();
        }

        public bool TryGetInvocationReturnType(string invocationId, out Type type)
        {
            throw new NotImplementedException();
        }
    }
}
