﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#if NET7_0_OR_GREATER
using System;
using Microsoft.AspNetCore.SignalR;

#nullable enable

namespace Microsoft.Azure.SignalR.Management
{
    internal sealed class WeakClientInvocationManager : IClientInvocationManager
    {
        public ICallerClientResultsManager Caller { get; }
        public IRoutedClientResultsManager? Router { get; }

        public WeakClientInvocationManager(IHubProtocolResolver hubProtocolResolver)
        {
            Caller = new WeakCallerClientResultsManager(
                hubProtocolResolver ?? throw new ArgumentNullException(nameof(hubProtocolResolver))
            );
        }

        public void CleanupInvocationsByConnection(string connectionId)
        {
            Caller.CleanupInvocationsByConnection(connectionId);
        }

        public bool TryGetInvocationReturnType(string invocationId, out Type type)
        {
            return Caller.TryGetInvocationReturnType(invocationId, out type);
        }
    }
}
#endif