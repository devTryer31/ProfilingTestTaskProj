using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProfilingTestTaskProj
{
	public sealed class StringsFileGenerator
	{
		private static readonly Random s_Rand_gen = new();

		private const int _Max_string_len = 200;
		private const int _Min_string_len = 20;
		private const int _Max_file_size_gb = 2;

		public string Generate(int linesCount)
		{
			string fileName = $"sFile-{linesCount}.txt";

			using var writer = new StreamWriter(fileName);

			try
			{
				ulong fileWeight_b = 0;
				foreach(string s in GenerateRandomStrings(linesCount))
				{
					string ts = s_Rand_gen.Next(10_000) + ". " + s;

					fileWeight_b += (ulong)Encoding.UTF8.GetByteCount(ts) + 2;//+2 for \r\n;

					if(fileWeight_b > unchecked(_Max_file_size_gb * 1024UL * 1024 * 1024))
						throw new OverflowException("File size upper bound reached");

					writer.WriteLine(ts);
				}
			}
			finally
			{
				writer.Dispose();
			}

			return fileName;
		}

		private IEnumerable<string> GenerateRandomStrings(int count)
		{
			Dictionary<int, string> dct = new(); //Pre-generated stings with their places idxs.

			for(int i = 0; i < count; i++)
			{
				if(dct.ContainsKey(i))
				{
					yield return dct[i];
					continue;
				}

				string s = GenerateRandString();

				int tmp = s_Rand_gen.Next(0, 5);
				while(tmp-- != 0)
				{
					int randIdx = s_Rand_gen.Next(i, count);
					if(!dct.ContainsKey(randIdx))
						dct.Add(randIdx, s);
				}

				yield return s;
			}
		}

		private string GenerateRandString()
		{
			var gen = s_Rand_gen;
			int len = gen.Next(_Min_string_len, _Max_string_len);
			char[] symbols = new char[len];

			for(int i = 0; i < len; i++)
			{
				int num = gen.Next('a', 'z' + 1 + ('Z' + 1 - 'A'));
				if(num > 'z')
					symbols[i] = (char)(num - ('z' + 1) + 'A');
				else
					symbols[i] = (char)num;
			}

			return new string(symbols);
		}

	}
}