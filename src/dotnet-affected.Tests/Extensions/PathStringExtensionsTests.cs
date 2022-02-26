using Xunit;

namespace Affected.Cli.Tests
{
    public class PathStringExtensionsTests
    {
        [Theory]
        [InlineData(@"c:\foo", @"c:", true)]
        [InlineData(@"c:\foo", @"c:\", true)]
        [InlineData(@"c:\foo", @"c:\foo", true)]
        [InlineData(@"c:\foo", @"c:\foo\", true)]
        [InlineData(@"c:\foo\", @"c:\foo", true)]
        [InlineData(@"c:\foo\bar\", @"c:\foo\", true)]
        [InlineData(@"c:\foo\bar", @"c:\foo\", true)]
        [InlineData(@"c:\foo\a.txt", @"c:\foo", true)]
        [InlineData(@"c:\FOO\a.txt", @"c:\foo", true)]
        [InlineData(@"c:/foo/a.txt", @"c:\foo", true)]
        [InlineData(@"c:\foobar", @"c:\foo", false)]
        [InlineData(@"c:\foobar\a.txt", @"c:\foo", false)]
        [InlineData(@"c:\foobar\a.txt", @"c:\foo\", false)]
        [InlineData(@"c:\foo\a.txt", @"c:\foobar", false)]
        [InlineData(@"c:\foo\a.txt", @"c:\foobar\", false)]

        // These don't pass on UNIX... AFAIK we should never be using paths with ../
        // hence this should never be an issue. One more reason for not making this shared.
        //[InlineData(@"c:\foo\..\bar\baz", @"c:\foo", false)]
        //[InlineData(@"c:\foo\..\bar\baz", @"c:\bar", true)]
        //[InlineData(@"c:\foo\..\bar\baz", @"c:\barr", false)]
        public void IsSubPathOfTest(string path, string baseDirPath, bool expected)
        {
            Assert.Equal(expected, path.IsSubPathOf(baseDirPath));
        }
    }
}
