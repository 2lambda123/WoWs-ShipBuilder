﻿using System;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;
using WoWsShipBuilder.Core.Builds;

namespace WoWsShipBuilder.Core.Test.BuildTests;

[TestFixture]
public class BuildDeserialization
{
    [Test]
    public void CreateBuildFromString_EmptyBuild_Success()
    {
        const string buildString = @"bJBLTsMwEIbv4nWRbNoF6a6xKqCIENV9qELImipWahE/ZDsCCXEyFhyJKzBOyI7lfP/MfKP5+fr+IGWvu6YCo8iSJBXT1TkDMiPiov29bdQ7BvVGlJTdIK0gaWcRbcCDRfDomr5TkSyfsWu/KhiTJaNytd1JsXviD9iC/C7zExhITrJisRipyFT0bqzWxXycXVe3f7MvM7L3bYBmEGDFwSfQ2V/zI6Us3/mqu26KnY29gfN0ED9RWkgeIF54UG91UEb3JuswQdNWtcr+k8yl8C4lFSaOu4VuLUyi4WsHFeLwjOvPXwAAAP//AwA=";
        const string buildName = "test-build";
        var logger = new Mock<ILogger>();

        var build = Build.CreateBuildFromString(buildString, logger.Object);

        logger.Verify(l => l.Info(It.IsAny<string>()), Times.Never());
        build.BuildName.Should().Be(buildName);
        build.BuildVersion.Should().Be(Build.CurrentBuildVersion);
        build.Modules.Should().NotBeEmpty();
        build.Upgrades.Should().BeEmpty();
        build.Captain.Should().NotBeNullOrWhiteSpace();
        build.Skills.Should().BeEmpty();
        build.Consumables.Should().NotBeEmpty();
        build.Signals.Should().BeEmpty();
        build.Hash.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void CreateBuildFromString_EmptyBuildOldVersion_Upgraded()
    {
        const string buildString = "bJDPSgMxEIffJecKCe3B7a0bilrpujStUqSEKRu2obtJyB8UxCfz4CP5Ck523ZvH+X4z8w3z8/X9Qcqku6aCXpEliSrEm3MGZEbERbsH06h3DOqNKCm7RVpB1NYg2oADg2Brm9SpQJav2HVYFYzJklG52u2l2D/xR2xBfp/5EXqIVrJisRipyFQkO1brYj7Orqu7v9nTjBxc66EZBFhxcBF09tf8hVKW77zqrptia0Lq4TwdxI+UFpJ7CBfu1VvtVa9Tn3WYoGmnWmX+SeZSOBuj8hPH3UK3BibR8LVn5cPwDPb5CwAA//8DAA==";
        const string buildName = "test-build";
        var logger = new Mock<ILogger>();

        var build = Build.CreateBuildFromString(buildString, logger.Object);

        logger.Verify(l => l.Info("Upgrading build {} from build version {} to current version.", buildName, 1), Times.Once());
        build.BuildName.Should().Be(buildName);
        build.BuildVersion.Should().Be(Build.CurrentBuildVersion);
        build.Modules.Should().NotBeEmpty();
        build.Upgrades.Should().BeEmpty();
        build.Captain.Should().NotBeNullOrWhiteSpace();
        build.Skills.Should().BeEmpty();
        build.Consumables.Should().NotBeEmpty();
        build.Signals.Should().BeEmpty();
        build.Hash.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void CreateBuildFromString_TwoBuildsSameString_SameHash()
    {
        const string buildString = @"bJBLTsMwEIbv4nWRbNoF6a6xKqCIENV9qELImipWahE/ZDsCCXEyFhyJKzBOyI7lfP/MfKP5+fr+IGWvu6YCo8iSJBXT1TkDMiPiov29bdQ7BvVGlJTdIK0gaWcRbcCDRfDomr5TkSyfsWu/KhiTJaNytd1JsXviD9iC/C7zExhITrJisRipyFT0bqzWxXycXVe3f7MvM7L3bYBmEGDFwSfQ2V/zI6Us3/mqu26KnY29gfN0ED9RWkgeIF54UG91UEb3JuswQdNWtcr+k8yl8C4lFSaOu4VuLUyi4WsHFeLwjOvPXwAAAP//AwA=";
        const string buildName = "test-build";

        var build1 = Build.CreateBuildFromString(buildString);
        var build2 = Build.CreateBuildFromString(buildString);

        build1.BuildName.Should().Be(buildName);
        build2.BuildName.Should().Be(buildName);

        build1.Hash.Should().Be(build2.Hash);
    }

    [Test]
    public void CreateBuildFromString_TwoBuildsDifferentString_SameHash()
    {
        const string buildString1 = @"bJBLTsMwEIbv4nWRbNoF6a6xKqCIENV9qELImipWahE/ZDsCCXEyFhyJKzBOyI7lfP/MfKP5+fr+IGWvu6YCo8iSJBXT1TkDMiPiov29bdQ7BvVGlJTdIK0gaWcRbcCDRfDomr5TkSyfsWu/KhiTJaNytd1JsXviD9iC/C7zExhITrJisRipyFT0bqzWxXycXVe3f7MvM7L3bYBmEGDFwSfQ2V/zI6Us3/mqu26KnY29gfN0ED9RWkgeIF54UG91UEb3JuswQdNWtcr+k8yl8C4lFSaOu4VuLUyi4WsHFeLwjOvPXwAAAP//AwA=";
        const string buildString2 = @"XJDBSsQwFEX/JesR0tpNZ1czozMjdkrTKiISMvTZBts0JC0K4pe58JP8BV9aCzKbwDv35t3L+/n6/iBXo2qrVHZA1mQAN1ycPCArwhtl9rqCdxSyA2f0MkKaykH1GtFBGqkR3PXV2IIj6yd0lUkcBoIFVCR5IXhxZLdoQb7zvNimN6UI4iiaYRHTeDIXxzzDJ5sx914+9vO0jYN5I37+2/i8IqWpraymWJyYNINUvlXGHigNfPtX1baL3Gs3dvK01GSP1Adb6Rpm4S2z0Kmx83GoYFqi7AZeQDvYKGfO5FBcq7oZwJ5xKnKoQf9fiNFc1VouPaZT34N10wXDz18AAAD//wMA";
        const string buildName = "test-build";

        var build1 = Build.CreateBuildFromString(buildString1);
        var build2 = Build.CreateBuildFromString(buildString2);

        build1.BuildName.Should().Be(buildName);
        build2.BuildName.Should().Be(buildName);

        build1.Hash.Should().NotBe(build2.Hash);
    }

    [Test]
    public void CreateBuildFromString_InvalidString_WrongEncoding()
    {
        const string buildString = @"bJBLTsMwEIbv4nWRbNoF6a6xKqCIENV9qELImipWahE/ZDsCCXEyFhyJKzBOyI7lfP/MfKP5+fr+IGWvu6YCo8iSJBXT1TkDMiPiov29bdQ7BvVGlJTdIK0gaWcRbcCDRfDomr5TkSyfsWu/KhiTJaNytd1JsXviD9iC/C7zExhITrJisRipyFT0bqzWxXycXVe3f7MvM7L3bYBmEGDFwSfQ2V/zI6Us3/mqu26KnY29gfN0ED9RWkgeIF54UG91UEb3JuswQdNWtcr+k8yl8C4lFSaOu4VuLUyi4WsHFeLwjOvPXwAAAP";

        Func<Build> action = () => Build.CreateBuildFromString(buildString);

        action.Should().ThrowExactly<FormatException>().WithInnerException<FormatException>();
    }

    [Test]
    public void CreateBuildFromString_InvalidString_NoValidJson()
    {
        const string input = "er97v6GRsYmpmbmFpQEAAAD//wMA";

        Func<Build> action = () => Build.CreateBuildFromString(input);

        action.Should().ThrowExactly<FormatException>().WithInnerException<JsonSerializationException>();
    }

    [Test]
    public void CreateBuildFromString_InvalidString_EmptyObject()
    {
        const string input = "er97f3UtAAAA//8DAA==";

        Func<Build> action = () => Build.CreateBuildFromString(input);

        action.Should().ThrowExactly<FormatException>().WithInnerException<JsonSerializationException>();
    }
}
