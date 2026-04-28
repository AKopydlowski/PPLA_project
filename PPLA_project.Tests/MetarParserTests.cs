using PPLA.Project.Core.Metar;
using Xunit;

namespace PPLA_project.Tests;

public class MetarParserTests
{
    [Fact]
    public void Parse_ShouldExtractMainTokens()
    {
        var parser = new MetarParser();
        var report = parser.Parse("EPWA 281200Z 23012KT 9999 BKN020 Q1017");

        Assert.Equal("23012KT", report.Wind);
        Assert.Equal("9999", report.Visibility);
        Assert.Equal("BKN020", report.Clouds);
        Assert.Equal("Q1017", report.Qnh);
    }
}
