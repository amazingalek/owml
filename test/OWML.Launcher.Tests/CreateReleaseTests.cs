﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OWML.Tests.Setup;
using Xunit;
using Xunit.Abstractions;

namespace OWML.Launcher.Tests
{
    public class CreateReleaseTests : OWMLTests
    {
        public CreateReleaseTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task CreateRelease()
        {
            Directory.Delete(OwmlReleasePath, true);

            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"{OwmlSolutionPath}/createrelease.bat",
                    WorkingDirectory = OwmlSolutionPath,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            });

            AssertFolderContainsFiles(OwmlReleasePath, new[]
            {
                "OWML.Launcher.exe",
                "OWML.Manifest.json",
                "OWML.DefaultConfig.json",
                "OWML.Abstractions.dll",
                "OWML.Common.dll",
                "OWML.GameFinder.dll",
                "OWML.Logging.dll",
                "OWML.ModHelper.dll",
                "OWML.ModHelper.Assets.dll",
                "OWML.ModHelper.Events.dll",
                "OWML.ModHelper.Input.dll",
                "OWML.ModHelper.Interaction.dll",
                "OWML.ModHelper.Menus.dll",
                "OWML.ModLoader.dll",
                "OWML.Patcher.dll",
                "OWML.Utils.dll",
                "0Harmony.dll",
                "dnlib.dll",
                "dnpatch.dll",
                "Gameloop.Vdf.dll",
                "Microsoft.Practices.Unity.dll",
                "NAudio-Unity.dll",
                "Newtonsoft.Json.dll",
                "System.Runtime.Serialization.dll",
                "OWML.zip"
            });

            AssertFolderContainsFiles($"{OwmlReleasePath}/Mods/OWML.EnableDebugMode", new[]
            {
                "default-config.json",
                "manifest.json",
                "OWML.EnableDebugMode.dll"
            });

            AssertFolderContainsFiles($"{OwmlReleasePath}/Mods/OWML.LoadCustomAssets", new[]
            {
                "blaster-firing.wav",
                "config.json",
                "cubebundle",
                "cubebundle.manifest",
                "default-config.json",
                "duck.obj",
                "duck.png",
                "manifest.json",
                "OWML.LoadCustomAssets.dll",
                "savefile.json",
                "spiral-mountain.mp3"
            });

            // todo log folder?
        }

        private void AssertFolderContainsFiles(string folder, string[] files)
        {
            files.ToList().ForEach(file => AssertFileExists(folder, file));
            Assert.Equal(Directory.GetFiles(folder).Length, files.Length);
        }

        private void AssertFileExists(string folder, string file) => 
            Assert.True(File.Exists($"{folder}/{file}"), $"File doesn't exist: {folder}/{file}");
    }
}
