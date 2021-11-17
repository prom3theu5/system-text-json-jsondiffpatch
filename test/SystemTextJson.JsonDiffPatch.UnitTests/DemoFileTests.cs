using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace SystemTextJson.JsonDiffPatch.UnitTests
{
    public class DemoFileTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DemoFileTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            // Initialize the patcher type
            JsonDiffPatcher.Diff((JsonNode) null, null);
        }

        [Fact]
        public void Diff_DemoJson()
        {
            // Compare the two JSON objects from https://benjamine.github.io/jsondiffpatch/demo/index.html
            var result = File.ReadAllText(@"Examples\demo_result.json");

            var sw = Stopwatch.StartNew();
            var diff = JsonDiffPatcher.DiffFile(
                @"Examples\demo_left.json",
                @"Examples\demo_right.json",
                new JsonDiffOptions
                {
                    TextDiffMinLength = 60,
                    // https://github.com/benjamine/jsondiffpatch/blob/a8cde4c666a8a25d09d8f216c7f19397f2e1b569/docs/demo/demo.js#L163
                    ArrayObjectItemKeyFinder = (n, i) =>
                    {
                        if (n is JsonObject obj
                            && obj.TryGetPropertyValue("name", out var value))
                        {
                            return value?.GetValue<string>() ?? "";
                        }

                        return null;
                    }
                });
            sw.Stop();

            var time = sw.ElapsedMilliseconds == 0
                ? $"{sw.ElapsedTicks} ticks"
                : $"{sw.Elapsed.TotalMilliseconds}ms";
            _testOutputHelper.WriteLine($"Diff completed in {time}");

            Assert.NotNull(diff);

            var diffJson = JsonSerializer.Serialize(diff, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var resultDiff = JsonDiffPatcher.Diff(diffJson, result);

            Assert.Null(resultDiff);
        }

        [Fact]
        public void Diff_LargeObjects()
        {
            var diff = JsonDiffPatcher.DiffFile(
                @"Examples\large_left.json",
                @"Examples\large_right.json");

            Assert.NotNull(diff);

            var diffJson = JsonSerializer.Serialize(diff, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Assert.NotNull(diffJson);
        }
    }
}
