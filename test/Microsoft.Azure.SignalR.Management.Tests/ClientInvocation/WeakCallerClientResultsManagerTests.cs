﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using Xunit;

namespace Microsoft.Azure.SignalR.Management.Tests
{
    public class WeakCallerClientResultsManagerTests
    {

        private WeakCallerClientResultsManager CreateManager(
            IServiceEndpointManager endpointManager = null,
            IEndpointRouter endpointRouter = null)
        {
            var mockEndpointManager = endpointManager ?? Mock.Of<IServiceEndpointManager>();
            var mockEndpointRouter = endpointRouter ?? Mock.Of<IEndpointRouter>();
            var ackHandler = new AckHandler();

            return new WeakCallerClientResultsManager(mockEndpointManager, mockEndpointRouter, ackHandler);
        }


        [Fact]
        public async Task AddInvocation_ShouldAddAndCompleteSuccessfully()
        {
            var manager = CreateManager();
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
            var manager = CreateManager();
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
            var manager = CreateManager();

            var connectionId = "connection1";
            var completionMessage = CompletionMessage.WithError("unknownId", "Error");

            var result = manager.TryCompleteResult(connectionId, completionMessage);

            Assert.False(result);
        }

        [Fact]
        public void GetReturnType_ShouldThrowForUnknownInvocationId()
        {
            var manager = CreateManager();

            var invocationId = "unknownId";

            Assert.Throws<InvalidOperationException>(() => manager.GetReturnType(invocationId));
        }

        [Fact]
        public void GetReturnType_ShouldReturnCorrectTypeForKnownInvocationId()
        {
            // Arrange
            var manager = CreateManager();
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

        [Fact]
        public async Task AddInvocation_ShouldCompleteSuccessfullyAcrossMultipleEndpoints()
        {
            // Arrange
            var mockEndpointManager = new Mock<IServiceEndpointManager>();
            var mockEndpointRouter = new Mock<IEndpointRouter>();

            // endpoints are not really used here, just need to return the size
            IEnumerable<ServiceEndpoint> endpoints = [null, null];

            var connectionId = "connection1";
            var hub = "testHub";

            mockEndpointRouter
                .Setup(router => router.GetEndpointsForConnection(It.Is<string>(id => id == connectionId), It.IsAny<IEnumerable<ServiceEndpoint>>()))
                .Returns(endpoints);

            var manager = CreateManager(mockEndpointManager.Object, mockEndpointRouter.Object);
            var cancellationToken = new CancellationToken();
            var invocationId = manager.GenerateInvocationId(connectionId);



            // Act
            var task = manager.AddInvocation<string>(hub, connectionId, invocationId, cancellationToken);

            var completionMessage1 = CompletionMessage.Empty(invocationId);
            manager.TryCompleteResult(connectionId, completionMessage1);
            Assert.False(task.IsCompleted, "The task should not be completed after the first message.");


            var completionMessage2 = CompletionMessage.WithResult(invocationId, "FinalResult");
            manager.TryCompleteResult(connectionId, completionMessage2);

            var result = await task;

            // Assert
            Assert.Equal("FinalResult", result);
        }

        [Fact]
        public async Task AddInvocation_ShouldCompleteSuccessfullyWhenFirstMessageHasResult()
        {
            // Arrange
            var mockEndpointManager = new Mock<IServiceEndpointManager>();
            var mockEndpointRouter = new Mock<IEndpointRouter>();

            // endpoints are not really used here, just need to return the size
            IEnumerable<ServiceEndpoint> endpoints = [null, null];

            var connectionId = "connection1";
            var hub = "testHub";

            mockEndpointRouter
                .Setup(router => router.GetEndpointsForConnection(It.Is<string>(id => id == connectionId), It.IsAny<IEnumerable<ServiceEndpoint>>()))
                .Returns(endpoints);

            var manager = CreateManager(mockEndpointManager.Object, mockEndpointRouter.Object);
            var cancellationToken = new CancellationToken();
            var invocationId = manager.GenerateInvocationId(connectionId);



            // Act
            var task = manager.AddInvocation<string>(hub, connectionId, invocationId, cancellationToken);

            var completionMessage1 = CompletionMessage.WithResult(invocationId, "Result");
            manager.TryCompleteResult(connectionId, completionMessage1);
            Assert.True(task.IsCompleted);
            var result = await task;

            // Assert
            Assert.Equal("Result", result);
        }

    }
}
