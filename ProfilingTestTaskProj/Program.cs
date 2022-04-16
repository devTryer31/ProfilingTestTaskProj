using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProfilingTestTaskProj
{
	class Program
	{
		public class Line : IComparable<Line>
		{
			public Line(string str)
			{
				int dotPos = str.IndexOf('.');

				Num = int.Parse(str[..dotPos]);
				Word = str[(dotPos + 2)..];
			}

			public string Word { get; set; }

			public int Num { get; set; }

			public string GetStringView() => $"{Num}. {Word}";

			public int CompareTo(Line other)
			{
				if(ReferenceEquals(this, other))
					return 0;
				if(ReferenceEquals(null, other))
					return 1;

				var wordComparison = string.Compare(Word, other.Word, StringComparison.Ordinal);
				if(wordComparison != 0)
					return wordComparison;

				return Num.CompareTo(other.Num);
			}
		}

		public class FileLineState
		{
			public StreamReader Reader { get; set; }

			public Line Line { get; set; }
		}

		public sealed class StringsFileSorter
		{
			private readonly string _fileName;

			private const double _Max_partial_file_size_gb = 1.5d;//TODO: Max_partial_file_size

			public StringsFileSorter(string fileName) => _fileName = fileName;

			public string Sort()
			{
				var files = SplitFile(_fileName, 1_000);
				SortParts(files);
				return MergeSort(files);
			}

			private string[] SplitFile(string fileName, int partLinesCount)
			{
				List<string> list = new();

				using var reader = new StreamReader(fileName);
				int partId = 0;
				while(!reader.EndOfStream)
				{
					string filePartName = $"p-{partId++}.txt";
					list.Add(filePartName);

					using var writer = new StreamWriter(filePartName);
					for(int i = 0; i < partLinesCount; ++i)
					{
						if(reader.EndOfStream)
							break;

						writer.WriteLine(reader.ReadLine());
					}
				}

				return list.ToArray();
			}

			private void SortParts(string[] fileNames)
			{
				foreach(var f in fileNames)
				{
					var res = File.ReadAllLines(f)
						.Select(l => new Line(l))
						.OrderBy(l => l)
						.Select(l => l.GetStringView());
					File.WriteAllLines(f, res);
				}
			}

			private string MergeSort(string[] filesNames)
			{
				string resultFileName = "result.txt";

				StreamReader[] readers = filesNames.Select(f => new StreamReader(f)).ToArray();

				try
				{
					var firstLines = readers
						.Select(r =>
							new FileLineState {
								Line = new Line(r.ReadLine()),
								Reader = r
							}).ToList();

					using var writer = new StreamWriter(resultFileName);

					while(firstLines.Count > 0)
					{
						var topLine = firstLines
							.OrderBy(l => l.Line).First();

						writer.WriteLine(topLine.Line.GetStringView());

						if (topLine.Reader.EndOfStream)
						{
							firstLines.Remove(topLine);
							continue;
						}

						topLine.Line = new Line(topLine.Reader.ReadLine());
					}
				}
				finally
				{
					foreach(StreamReader sr in readers)
						sr.Dispose();
				}

				return resultFileName;
			}

		}


		static void Main(string[] args)
		{
			var file = new StringsFileGenerator().Generate(10_000);
			new StringsFileSorter(file).Sort();

			Console.WriteLine("done");
		}
	}
}
