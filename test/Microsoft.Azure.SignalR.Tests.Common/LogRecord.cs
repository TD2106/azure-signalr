// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging.Testing;

namespace Microsoft.Azure.SignalR.Tests.Common
{
    // WriteContext, but with a timestamp...
    public class LogRecord
    {
        public DateTime Timestamp { get; }

        public WriteContext Write { get; }

        public LogRecord(DateTime timestamp, WriteContext write)
        {
            Timestamp = timestamp;
            Write = write;
        }
    }
}
