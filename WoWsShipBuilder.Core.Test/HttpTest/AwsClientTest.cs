﻿using System;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;

namespace WoWsShipBuilder.Core.Test.HttpTest
{
    [TestFixture]
    public class AwsClientTest
    {
        private Mock<HttpMessageHandler> messageHandlerMock = default!;

        private MockFileSystem mockFileSystem = default!;

        private AppDataHelper appDataHelper = default!;

        [SetUp]
        public void Setup()
        {
            messageHandlerMock = new Mock<HttpMessageHandler>();
            mockFileSystem = new MockFileSystem();
            appDataHelper = new AppDataHelper(mockFileSystem);
            messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.OK));
        }

        [Test]
        public async Task DownloadFile_LocalDirectoryDoesNotExist()
        {
            var client = new AwsClient(mockFileSystem, appDataHelper, messageHandlerMock.Object);
            var filePath = @"C:\json\live\VersionInfo.json";
            var requestUri = new Uri("https://cloudfront/api/live/VersionInfo.json");
            mockFileSystem.FileExists(filePath).Should().BeFalse();

            await client.DownloadFileAsync(requestUri, filePath);

            mockFileSystem.FileExists(filePath).Should().BeTrue();
            messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == requestUri), ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task DownloadFile_LocalDirectoryDoesNotExist_OneRetry()
        {
            messageHandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new AwsClient(mockFileSystem, appDataHelper, messageHandlerMock.Object);
            var filePath = @"C:\json\live\VersionInfo.json";
            var requestUri = new Uri("https://cloudfront/api/live/VersionInfo.json");
            mockFileSystem.FileExists(filePath).Should().BeFalse();

            await client.DownloadFileAsync(requestUri, filePath);

            mockFileSystem.FileExists(filePath).Should().BeTrue();
            messageHandlerMock.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == requestUri), ItExpr.IsAny<CancellationToken>());
        }
    }
}