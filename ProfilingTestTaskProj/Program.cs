using System;
using System.Diagnostics;

namespace ProfilingTestTaskProj
{
	class Program
	{
		static void Main(string[] args)
		{
			var sw = Stopwatch.StartNew();

			var file = new StringsFileGenerator().Generate(150_050);
			new StringsFileSorter(file).Sort(1000);

			sw.Stop();

			Console.WriteLine("done: {0}", sw.Elapsed);
		}
	}
}
