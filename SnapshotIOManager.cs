using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AwsMeteringEstimator
{
	public static class SnapshotIOManager
	{
		private static HashSet<string> trackedFiles { get; set; } = null;
		private const string snapshotDateFormat = "yyyyMMddHHmmss";
		private const string snapshotFileFormat = "snapshot{0}.json";
		
		public static void CreateSnapshot(string[] trackedDirectories)
		{
			foreach (string path in trackedDirectories)
			{
				CatchFileErrors(() => trackedFiles.UnionWith(Directory.EnumerateFiles(path,
					"*.*",
					SearchOption.AllDirectories)), path);
			}
		}
		
		private static bool CatchFileErrors(Action function, string path)
		{
			try
			{
				function.Invoke();
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine($"You do not have permissions for directory '{path}'");
			}
			catch (DirectoryNotFoundException)
			{
				Console.WriteLine($"Path not found: '{path}'");
			}
			catch (IOException)
			{
				Console.WriteLine($"Invalid path: '{path}'");
			}
			return false;
		}
		
		public static IEnumerable<string> LoadSnapshots()
		{
			IEnumerable<string> snapshots = new List<string>();
			foreach(string snapshotFile in 
				Directory.EnumerateFiles("./",
					string.Format(snapshotFileFormat, "*"),
					SearchOption.TopDirectoryOnly))
			{
				snapshots.Append(snapshotFile);
			}
			
			return snapshots;
		}
		
		public static HashSet<string> LoadSnapshot() => 
			LoadSnapshot(MostRecentSnapshotFile());
		
		public static HashSet<string> LoadSnapshot(string SnapshotFilename)
		{
			HashSet<string> files = null;
			CatchFileErrors(() => files =
				(HashSet<string>) JsonSerializer.Deserialize(
					File.ReadAllText(SnapshotFilename), typeof(HashSet<string>)), SnapshotFilename);
			if(files == null)
				throw new Exception("Loading snapshot failed");
			
			return files;
		}
		
		public static void SaveSnapshot()
		{
			string SnapshotFilename = string.Format(snapshotFileFormat, DateTime.Now.ToString(snapshotDateFormat));
			if(trackedFiles == null)
				throw new Exception("No snapshot to save! Try calling CreateSnapshot() first");
			string json = JsonSerializer.Serialize(trackedFiles);
			Console.WriteLine(json.Take(50)); // DEBUG
			File.WriteAllText(SnapshotFilename, json);
		}
		
		public static string MostRecentSnapshotFile()
		{
			string newestSnapshot = "";
			DateTime newestSnapshotCreationTime = DateTime.MinValue;
			
			foreach (string snapshotFile in 
				Directory.EnumerateFiles("./",
					string.Format(snapshotFileFormat, "*"),
					SearchOption.TopDirectoryOnly))
			{
				// If this file is newer:
				DateTime creationDate = File.GetCreationTime($@"./{snapshotFile}");
				if(creationDate.Ticks > newestSnapshotCreationTime.Ticks)
				{
					newestSnapshot = snapshotFile;
					newestSnapshotCreationTime = creationDate;
				}
			}
			return newestSnapshot;
		}
		
		public static DateTime SnapshotDate(string snapshotFile)
		{
			string dateString = new Regex("[^0-9]").Replace(snapshotFile, "");
			Console.WriteLine($"DateStr: {dateString}"); // DEBUG
			try
			{
				return DateTime.ParseExact(dateString, snapshotDateFormat, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				string sampleFormat = string.Format(snapshotDateFormat, snapshotDateFormat);
				throw new ArgumentException($"File must be a valid snapshot file with filename formatted as '{sampleFormat}'");
			}
		}
	}
}
