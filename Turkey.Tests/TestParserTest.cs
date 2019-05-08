using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace Turkey.Tests
{
    public class TestParserTests
    {

        [Fact]
        public void DisabledTestShouldBeSkipped()
        {
            TestParser parser = new TestParser();
            SystemUnderTest system = new SystemUnderTest();
            TestDescriptor test = new TestDescriptor()
            {
                Enabled = false,
            };

            var shouldRun = parser.ShouldRunTest(system, test);

            Assert.False(shouldRun);
        }

        [Theory]
        [InlineData("1.0.1", false)]
        [InlineData("1.1", false)]
        [InlineData("1.1.1", false)]
        [InlineData("2.0", false)]
        [InlineData("2.0.9", false)]
        [InlineData("2.1", true)]
        [InlineData("2.1.0", true)]
        [InlineData("2.1.1", true)]
        [InlineData("2.2", true)]
        [InlineData("2.3", true)]
        [InlineData("3.0", true)]
        public void TestShouldBeRunForSameOrHigherVersions(string version, bool expectedToRun)
        {
            TestParser parser = new TestParser();
            SystemUnderTest system = new SystemUnderTest()
            {
                RuntimeVersion = Version.Parse(version),
                CurrentPlatformIds = new List<string>(),
            };
            TestDescriptor test = new TestDescriptor()
            {
                Enabled = true,
                VersionSpecific = false,
                Version = "2.1",
            };

            var shouldRun = parser.ShouldRunTest(system, test);

            Assert.Equal(expectedToRun, shouldRun);
        }

        [Theory]
        [InlineData("1.0.1", false)]
        [InlineData("1.1", false)]
        [InlineData("1.1.1", false)]
        [InlineData("2.0", false)]
        [InlineData("2.0.9", false)]
        [InlineData("2.1", true)]
        [InlineData("2.1.0", true)]
        [InlineData("2.1.1", true)]
        [InlineData("2.1.99", true)]
        [InlineData("2.2", false)]
        [InlineData("2.3", false)]
        [InlineData("3.0", false)]
        public void VersionSpecificTestShouldBeRunForSameMajorMinorVersion(string version, bool expectedToRun)
        {
            TestParser parser = new TestParser();
            SystemUnderTest system = new SystemUnderTest()
            {
                RuntimeVersion = Version.Parse(version),
                CurrentPlatformIds = new List<string>(),
            };
            TestDescriptor test = new TestDescriptor()
            {
                Enabled = true,
                VersionSpecific = true,
                Version = "2.1",
            };

            var shouldRun = parser.ShouldRunTest(system, test);

            Assert.Equal(expectedToRun, shouldRun);
        }

        [Theory]
        [InlineData("1.0.1", false)]
        [InlineData("1.1", false)]
        [InlineData("1.1.1", false)]
        [InlineData("2.0", true)]
        [InlineData("2.0.9", true)]
        [InlineData("2.1", true)]
        [InlineData("2.1.0", true)]
        [InlineData("2.1.1", true)]
        [InlineData("2.1.99", true)]
        [InlineData("2.2", true)]
        [InlineData("2.2.99", true)]
        [InlineData("2.3", true)]
        [InlineData("2.9", true)]
        [InlineData("3.0", false)]
        [InlineData("3.2", false)]
        public void VersionSpecificTestWithWildcardShouldBeRunForSameMajorVersion(string version, bool expectedToRun)
        {
            TestParser parser = new TestParser();
            SystemUnderTest system = new SystemUnderTest()
            {
                RuntimeVersion = Version.Parse(version),
                CurrentPlatformIds = new List<string>(),
            };
            TestDescriptor test = new TestDescriptor()
            {
                Enabled = true,
                VersionSpecific = true,
                Version = "2.x",
            };

            var shouldRun = parser.ShouldRunTest(system, test);

            Assert.Equal(expectedToRun, shouldRun);
        }

        [Theory]
        [InlineData(new string[] { "linux" }, new string[] { }, true)]
        [InlineData(new string[] { "linux" }, new string[] { "fedora" }, true)]
        [InlineData(new string[] { "fedora" }, new string[] { }, true)]
        [InlineData(new string[] { "fedora99" }, new string[] { }, true)]
        [InlineData(new string[] { "fedora99" }, new string[] { "fedora10" }, true)]
        [InlineData(new string[] { "fedora" }, new string[] { "fedora" }, false)]
        [InlineData(new string[] { "fedora", "fedora99" }, new string[] { "fedora" }, false)]
        public void TestShouldNotRunOnBlacklistedPlatforms(string[] currentPlatforms, string[] platformBlacklist, bool expectedToRun)
        {
            TestParser parser = new TestParser();
            SystemUnderTest system = new SystemUnderTest()
            {
                RuntimeVersion = Version.Parse("2.1"),
                CurrentPlatformIds = currentPlatforms.ToList(),
            };
            TestDescriptor test = new TestDescriptor()
            {
                Enabled = true,
                Version = "2.1",
                PlatformBlacklist = platformBlacklist.ToList(),
            };

            var shouldRun = parser.ShouldRunTest(system, test);

            Assert.Equal(expectedToRun, shouldRun);
        }
    }
}
