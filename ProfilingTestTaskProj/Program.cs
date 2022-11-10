using System;
using System.Diagnostics;
using System.IO;

namespace ProfilingTestTaskProj
{
	class Program
	{
		static void Main(string[] args)
		{
			var sw = Stopwatch.StartNew();

			var file = new StringsFileGenerator().Generate(600_000);
			new StringsFileSorter(file).Sort(53_000);

			sw.Stop();

			Console.WriteLine("done: {0}", sw.Elapsed);

			using var fw = File.AppendText("time_dump.log");
			fw.WriteLine($"{DateTime.Now :hh:mm:ss} -> {sw.Elapsed}");
		}
	}
}
