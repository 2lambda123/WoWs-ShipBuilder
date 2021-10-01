﻿using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SplashScreenViewModel : ViewModelBase
    {
        private const int TaskNumber = 4;

        private double progress;

        private AwsClient awsClient;

        private IFileSystem fileSystem;

        public SplashScreenViewModel()
            : this(AwsClient.Instance, new FileSystem())
        {
        }

        public SplashScreenViewModel(AwsClient awsClient, IFileSystem fileSystem)
        {
            this.awsClient = awsClient;
            this.fileSystem = fileSystem;
        }

        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        private string downloadInfo = "placeholder";

        public string DownloadInfo
        {
            get => downloadInfo;
            set => this.RaiseAndSetIfChanged(ref downloadInfo, value);
        }

        public async Task VersionCheck()
        {
            IProgress<(int, string)> progressTracker = new Progress<(int state, string title)>(value =>
            {
                // ReSharper disable once PossibleLossOfFraction
                Progress = value.state * 100 / TaskNumber;
                DownloadInfo = value.title;
            });

            await JsonVersionCheck(progressTracker);
            await DownloadImages(progressTracker);
            progressTracker.Report((TaskNumber, "done"));
        }

        private async Task JsonVersionCheck(IProgress<(int, string)> progressTracker)
        {
            progressTracker.Report((1, "jsonData"));
            await awsClient.CheckFileVersion(ServerType.Live);
        }

        private async Task DownloadImages(IProgress<(int, string)> progressTracker)
        {
            string imageBasePath = fileSystem.Path.Combine(AppDataHelper.Instance.AppDataDirectory, "Images");
            var shipImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Ships"));
            if (!shipImageDirectory.Exists || !shipImageDirectory.GetFiles().Any())
            {
                progressTracker.Report((2, "shipImages"));
                await awsClient.DownloadAllImages(ImageType.Ship);
            }

            var camoImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Camos"));
            if (!camoImageDirectory.Exists || !camoImageDirectory.GetFiles().Any())
            {
                progressTracker.Report((3, "camoImages"));
                await awsClient.DownloadAllImages(ImageType.Camo);
            }
        }
    }
}