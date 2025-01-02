// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Microsoft.Azure.SignalR.Management.Tests
{
    public class WeakCallerClientResultsManagerTests
    {
        private class MockHubProtocolResolver : IHubProtocolResolver
        {
            public IReadOnlyList<IHubProtocol> AllProtocols => throw new NotImplementedException();

            public IHubProtocol GetProtocol(string protocolName, IReadOnlyList<string> supportedProtocols)
            {
                if (protocolName == "json")
                {
                    return new JsonHubProtocol();
                }
                return null;
            }
        }

        [Fact]
        public async Task AddInvocation_ShouldAddAndCompleteSuccessfully()
        {
            var resolver = new MockHubProtocolResolver();
            var manager = new WeakCallerClientResultsManager(resolver);
            var cancellationToken = new CancellationToken();

            var connectionId = "connection1";
            var hub = "testHub";
            var invocationId = manager.GenerateInvocationId(connectionId);

            var task = manager.AddInvocation<string>(hub, connectionId, invocationId, cancellationToken);
            var completionMessage = CompletionMessage.WithResult(invocationId, "TestResult");
            manager.TryCompleteResult(connectionId, completionMessage);

            var result = await task;
            Assert.Equal("TestResult", result);
        }

        [Fact]
        public void CleanupInvocationsByConnection_ShouldRemoveAllMatchingInvocations()
        {
            var resolver = new MockHubProtocolResolver();
            var manager = new WeakCallerClientResultsManager(resolver);
            var cancellationToken = new CancellationToken();

            var connectionId = "connection1";
            var hub = "testHub";
            var invocationId1 = manager.GenerateInvocationId(connectionId);
            var invocationId2 = manager.GenerateInvocationId(connectionId);

            manager.AddInvocation<string>(hub, connectionId, invocationId1, cancellationToken);
            manager.AddInvocation<string>(hub, connectionId, invocationId2, cancellationToken);

            manager.CleanupInvocationsByConnection(connectionId);

            Assert.Throws<InvalidOperationException>(() => manager.GetReturnType(invocationId1));
            Assert.Throws<InvalidOperationException>(() => manager.GetReturnType(invocationId2));
        }

        [Fact]
        public void TryCompleteResult_ShouldReturnFalseForUnknownInvocationId()
        {
            var resolver = new MockHubProtocolResolver();
            var manager = new WeakCallerClientResultsManager(resolver);

            var connectionId = "connection1";
            var completionMessage = CompletionMessage.WithError("unknownId", "Error");

            var result = manager.TryCompleteResult(connectionId, completionMessage);

            Assert.False(result);
        }

        [Fact]
        public void GetReturnType_ShouldThrowForUnknownInvocationId()
        {
            var resolver = new MockHubProtocolResolver();
            var manager = new WeakCallerClientResultsManager(resolver);

            var invocationId = "unknownId";

            Assert.Throws<InvalidOperationException>(() => manager.GetReturnType(invocationId));
        }

        [Fact]
        public void GetReturnType_ShouldReturnCorrectTypeForKnownInvocationId()
        {
            // Arrange
            var resolver = new MockHubProtocolResolver();
            var manager = new WeakCallerClientResultsManager(resolver);
            var cancellationToken = new CancellationToken();

            var connectionId = "connection1";
            var hub = "testHub";
            var invocationId = manager.GenerateInvocationId(connectionId);

            manager.AddInvocation<string>(hub, connectionId, invocationId, cancellationToken);

            // Act
            var returnType = manager.GetReturnType(invocationId);

            // Assert
            Assert.Equal(typeof(string), returnType);
        }
    }
}
