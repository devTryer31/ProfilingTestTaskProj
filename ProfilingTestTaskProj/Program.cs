using System;

namespace ProfilingTestTaskProj
{
	class Program
	{
		static void Main(string[] args)
		{
			new StringsFileGenerator().Generate(100);

			Console.WriteLine("done");
		}
	}
}
