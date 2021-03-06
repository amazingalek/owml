﻿using OWML.ModHelper;
using OWML.Tests.Setup;
using Xunit;
using Xunit.Abstractions;

namespace OWML.Utils.Tests
{
	public class TypeExtensionTests : OWMLTests
	{
		public TypeExtensionTests(ITestOutputHelper outputHelper)
			: base(outputHelper)
		{
		}

		[Fact]
		public void GetValue()
		{
			var config = new ModConfig
			{
				Enabled = true
			};

			var value = config.GetValue<bool>("Enabled");

			Assert.True(value);
		}

		[Fact]
		public void SetValue()
		{
			var config = new ModConfig
			{
				Enabled = false
			};

			config.SetValue("Enabled", true);

			Assert.True(config.Enabled);
		}
	}
}
