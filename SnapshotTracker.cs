using System;
using System.Collections.Generic;

namespace AwsMeteringEstimator
{
	public static class SnapshotTracker
	{
		public static int NumberOfFileChanges(DateTime start, DateTime end)
		{
			// FileTracker.FilesModified().Count;
			// TODO: Here, the date difference will be different from the specified start and end, so we should normalise it in order to get an approximation of the actual NumberOfFileChanges over the period
		}
		
		public static int SizeOfFileChanges(DateTime start, DateTime end)
		{
			// TODO: Here, the date difference will be different from the specified start and end, so we should normalise it in order to get an approximation of the actual FileSizeChanges over the period
		}
		
		private static (string snapshot, long offsetTicks) SnapshotClosestToDate(DateTime date)
		{
			string NearestSnapshot = "";
			long SnapshotDateDifferenceTicks = long.MaxValue;
			
			IEnumerable<string> snapshots = SnapshotIOManager.LoadSnapshots();
			foreach (string snapshot in snapshots)
			{
				if(Math.Abs(SnapshotIOManager.SnapshotDate(snapshot).Ticks - date.Ticks) < SnapshotDateDifferenceTicks)
				{
					SnapshotDateDifferenceTicks = Math.Abs(SnapshotIOManager.SnapshotDate(snapshot).Ticks - date.Ticks);
					NearestSnapshot = snapshot;
				}
			}
			return (NearestSnapshot, SnapshotDateDifferenceTicks);
		}
	}
}
