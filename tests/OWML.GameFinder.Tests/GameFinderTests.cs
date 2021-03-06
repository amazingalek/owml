﻿using System;
using System.IO;
using OWML.Common;
using OWML.Tests.Setup;
using Xunit;
using Xunit.Abstractions;

namespace OWML.GameFinder.Tests
{
	public class GameFinderTests : OWMLTests
	{
		public GameFinderTests(ITestOutputHelper outputHelper)
			: base(outputHelper)
		{
		}

		[Fact]
		public void PathFinder_FindGamePath()
		{
			var pathFinder = new PathFinder(new OwmlConfig(), Console.Object);

			var gamePath = pathFinder.FindGamePath();

			Assert.Equal(new DirectoryInfo(SteamGamePath).FullName, new DirectoryInfo(gamePath).FullName, StringComparer.InvariantCultureIgnoreCase);
		}
	}
}
