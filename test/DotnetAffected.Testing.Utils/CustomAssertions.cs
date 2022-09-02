using System;
using System.Linq;
using Xunit;

namespace DotnetAffected.Testing.Utils
{
    public static class CustomAssertions
    {
        public static void LineSequenceEquals(string output, params Action<string>[] callbacks)
        {
            var split = output.Split(Environment.NewLine)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            Assert.Collection(split, callbacks);
        }
    }
}
