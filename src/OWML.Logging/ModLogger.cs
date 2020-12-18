﻿using System;
using System.IO;
using System.Linq;
using OWML.Common;

namespace OWML.Logging
{
	public class ModLogger : IModLogger
	{
		private static IOwmlConfig _config;
		private readonly IModManifest _manifest;
		private static string _logFileName;

		public ModLogger(IOwmlConfig config, IModManifest manifest)
		{
			if (_config == null)
			{
				_config = config;
			}
			_manifest = manifest;

			var startTime = DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");
			_logFileName = $"{config.LogsPath}/OWML.Log.{startTime}.txt";

			if (!Directory.Exists(config.LogsPath))
			{
				Directory.CreateDirectory(config.LogsPath);
			}
		}

		public void Log(string s) => 
			LogInternal($"[{_manifest.Name}]: {s}");

		public void Log(params object[] objects) => 
			Log(string.Join(" ", objects.Select(o => o.ToString()).ToArray()));

		private static void LogInternal(string message) => 
			File.AppendAllText(_logFileName, $"{DateTime.Now}: {message}{Environment.NewLine}");
	}
}
