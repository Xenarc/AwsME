using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AwsMeteringEstimator
{
	public static class FileTracker
	{
		private static long FileSize(string path)
		{
			return (new FileInfo(path)).Length;
		}
		
		public static DateTime DateModified(string path)
		{
			return (new FileInfo(path)).LastWriteTime;
		}
		
		public static HashSet<string> FilesModified(IEnumerable<string> original, IEnumerable<string> current)
		{
			IEnumerable<(string before, string after)> files = original.Zip(current);
			return (HashSet<string>)files
				.Where(f => 
					FileSize(f.before) != FileSize(f.after) || 
					Math.Abs(DateModified(f.before).Second - DateModified(f.before).Second) < 5)
				.Select(f => f.after);
		}
		
		public static long TotalFileSize(IEnumerable<string> files)
		{
			long total = 0;
			foreach (string file in files)
			{
				total += FileSize(file);
			}
			return total;
		}
	}
}
