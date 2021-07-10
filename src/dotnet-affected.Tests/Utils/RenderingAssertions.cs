using System;
using System.Linq;
using Xunit;

namespace Affected.Cli.Tests
{
    internal static class RenderingAssertions
    {
        public static void LineSequenceEquals(string output, params Action<string>[] callbacks)
        {
            var split = output.Split(Environment.NewLine)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            Assert.Collection(split, callbacks);
        }
    }
}
