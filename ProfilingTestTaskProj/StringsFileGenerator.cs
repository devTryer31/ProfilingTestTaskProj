using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProfilingTestTaskProj
{
	public sealed class StringsFileGenerator
	{
		private static readonly Random s_Rand_gen = new();

		private const int _Max_string_len = 200;
		private const int _Min_string_len = 20;

		public void Generate(int linesCount)
		{
			string fileName = $"sFile-{linesCount}.txt";

			using var writer = new StreamWriter(fileName);

			foreach (string s in GenerateRandomStrings(linesCount))
				writer.WriteLine(s_Rand_gen.Next(10_000) + ". " + s);


		}

		private IEnumerable<string> GenerateRandomStrings(int count)
		{
			SortedDictionary<int, string> dct = new(); //Pre-generated stings with their places idxs.

			for (int i = 0; i < count; i++) {
				if (dct.ContainsKey(i)) {
					yield return dct[i];
					continue;
				}

				string s = GenerateRandString();

				int tmp = s_Rand_gen.Next(0, 5);
				while (tmp-- != 0) {
					int randIdx = s_Rand_gen.Next(i, count);
					if (!dct.ContainsKey(randIdx))
						dct.Add(randIdx, s);
				}

				yield return s;
			}
		}

		private string GenerateRandString()
		{
			var gen = s_Rand_gen;
			return new string(
				Enumerable.Range(_Min_string_len, gen.Next(_Max_string_len))
					.Select(_ => (char)(gen.Next() % 2 == 0 ? gen.Next('a', 'z') : gen.Next('A', 'Z')))
					.ToArray()
			);
		}

	}
}