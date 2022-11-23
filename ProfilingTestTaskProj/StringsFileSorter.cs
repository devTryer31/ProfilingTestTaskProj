using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProfilingTestTaskProj
{
    public sealed class StringsFileSorter
    {
        public const string ResultFileName = "result.txt";

        private readonly struct Line : IComparable<Line>
        {
            private readonly string _line;

            private readonly int _dotPos;

            public Line(string str)
            {
                _line = str;
                _dotPos = str.IndexOf('.');

                Num = int.Parse(_line.AsSpan(0, _dotPos));
            }

            public ReadOnlySpan<char> Word => _line.AsSpan(_dotPos + 2);

            public int Num { get; }

            public string GetStringView() => _line;

            public int CompareTo(Line other)
            {
                var wordComparison = Word.CompareTo(other.Word, StringComparison.Ordinal);
                return wordComparison != 0 ? wordComparison : Num.CompareTo(other.Num);
            }
        }

        private sealed class FileLineState
        {
            public StreamReader Reader { get; set; }

            public Line Line { get; set; }
        }

        private readonly string _fileName;

        private const double _Max_partial_file_size_gb = 1.5d;//TODO: Max_partial_file_size

        public StringsFileSorter(string fileName) => _fileName = fileName;

        public string Sort(int partLinesCount)
        {
            var files = SplitFile(_fileName, partLinesCount);
            SortPartsParallel(files);
            return MergeSort(files);
        }

        //TODO: can be paralelled?
        private string[] SplitFile(string fileName, int partLinesCount)
        {
            List<string> list = new();

            using var reader = new StreamReader(fileName);
            int partId = 0;
            while (!reader.EndOfStream)
            {
                string filePartName = $"sPartFiles/p-{partId++}.txt";
                list.Add(filePartName);

                using var writer = new StreamWriter(filePartName);
                for (int i = 0; i < partLinesCount; ++i)
                {
                    if (reader.EndOfStream)
                        break;

                    writer.WriteLine(reader.ReadLine());
                }
            }

            return list.ToArray();
        }

        private void SortParts(string[] fileNames)
        {
            foreach (var f in fileNames)
            {
                var res = File.ReadAllLines(f)
                    .Select(l => new Line(l))
                    .OrderBy(l => l)
                    .Select(l => l.GetStringView());
                File.WriteAllLines(f, res);
            }
        }

        private void SortPartsParallel(string[] fileNames)
        {
            var res = Parallel.ForEach(fileNames,
                (f) =>
                {
                    var res = File.ReadAllLines(f)
                    .Select(l => new Line(l))
                    .OrderBy(l => l)
                    .Select(l => l.GetStringView());
                    File.WriteAllLines(f, res);
                });
        }

        private string MergeSort(string[] filesNames)
        {
            StreamReader[] readers = filesNames.Select(f => new StreamReader(f)).ToArray();

            try
            {
                var firstLines = readers
                    .Select(r =>
                        new FileLineState
                        {
                            Line = new Line(r.ReadLine()),
                            Reader = r
                        }).ToList();

                using var writer = new StreamWriter(ResultFileName);

                while (firstLines.Count > 0)
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
                foreach (StreamReader sr in readers)
                    sr.Dispose();
            }

            return ResultFileName;
        }

    }
}