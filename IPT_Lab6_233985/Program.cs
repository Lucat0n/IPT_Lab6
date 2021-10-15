using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPT_Lab6_233985
{
    class Program
    {

        /*private class Suffix
        {
            private int index;
            private string suff;
            //private ReadOnlySpan<char> test;
            
            public int Index { get => index; set => index = value; }
            public string Suff { get => suff; set => suff = value; }

            public override string ToString()
            {
                return index.ToString() + " " + suff;
            }
        }*/

        private class SuffixArray
        {
            private int[] indexes;
            private String text;

            public SuffixArray(string text)
            {
                this.text = text;
                indexes = Enumerable.Range(0, text.Length).ToArray();
                //indexes = indexes.OrderBy(x => text.AsSpan(x).ToString()).ToArray();
                Array.Sort(indexes, CompareElements);
            }

            public int CompareElements(int i1, int i2)
            {
                return text.AsSpan(i1).SequenceCompareTo(text.AsSpan(i2));
            }

            public (int, string) this[int i]
            {
                get { return (indexes[i], text.AsSpan(indexes[i]).ToString()); }
            }

            public (int, string) this[(int, int) tuple]
            {
                get { return (indexes[tuple.Item1], text.AsSpan(indexes[tuple.Item1], Math.Min(tuple.Item2, PeekLength(tuple.Item1))).ToString()); }
            }

            public int PeekLength(int index)
            {
                return text.Length - indexes[index];
            }

            public void PrintAllContents()
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    Console.WriteLine(i + "\t" + this[i].ToString());
                }
            }

            public void Search(string pattern, bool logging = false)
            {
                int n = text.Length;

                int l = 0;
                int r = n - 1;
                while (l <= r)
                {
                    int mid = l + (r - l) / 2;
                    int diff = String.Compare(pattern, this[(mid, pattern.Length)].Item2);

                    if (diff == 0)
                    {
                        int old_mid = mid;
                        if (mid > 0)
                        {
                            while (String.Compare(pattern, this[(mid, pattern.Length)].Item2) == 0)
                            {
                                if(logging)
                                    Console.WriteLine("Pattern found at index " + indexes[mid]);
                                mid--;
                            }
                        }
                        mid = old_mid+1;
                        if (mid < n - 1)
                        {
                            while (String.Compare(pattern, this[(mid, pattern.Length)].Item2) == 0)
                            {
                                if (logging)
                                    Console.WriteLine("Pattern found at index " + indexes[mid]);
                                mid++;
                            }
                        }
                        return;
                    }
                    if (diff < 0)
                        r = mid - 1;
                    else
                        l = mid + 1;

                }
                if (logging)
                    Console.WriteLine("Pattern not found");
            }

        }

        static void Main(string[] args)
        {
            /*if (args.Length != 1)
                Console.WriteLine("Invalid input args count.");
            else if (!File.Exists(args[0]))
                Console.WriteLine("Given file does not exist.");
            else
            {*/
            RunBenchmarks(args[0]);
            Console.WriteLine("Done");
            //}
            Console.ReadKey();
        }

        static void RunBenchmarks(string path)
        {
            string[] size_ext = new string[] { ".1MB", ".2MB", ".3MB", ".4MB", ".5MB", ".10MB" };
            string[] m = new string[] { "8", "16", "32", "64" };
            Stopwatch stopwatch = new Stopwatch();
            StreamWriter streamWriter = new StreamWriter("Output_SA.csv");
            streamWriter.WriteLine("m/mode;file_size;time(ms)");
            foreach (string ext in size_ext)
            {
                String text = File.ReadAllText(System.IO.Path.ChangeExtension(path, ext));
                stopwatch.Start();
                SuffixArray suffixArray = new SuffixArray(text);
                stopwatch.Stop();
                Console.WriteLine("Total array building time(" + ext + " file): " + stopwatch.ElapsedMilliseconds + "ms");
                streamWriter.WriteLine("build;" + ext.Substring(1) + ";" + stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
                //string updated_path = Path.ChangeExtension(path, ext);
                foreach (string m_size in m)
                {
                    String patt = File.ReadAllText(System.IO.Path.ChangeExtension(path, null) + "_patt_" + ext.Substring(1) + "_" + m_size);
                    int pattern_length = Int32.Parse(m_size);
                    int pattern_count = patt.Length / pattern_length;
                    String[] patterns = new string[pattern_count];
                    for(int i=0; i<pattern_count; i++)
                        patterns[i] = patt.Substring(i * pattern_length, pattern_length);
                    stopwatch.Start();
                    for (int i = 0; i < 50000; i++)
                    {
                        foreach (string pattern in patterns)
                            suffixArray.Search(pattern);
                    }
                    stopwatch.Stop();
                    streamWriter.WriteLine(m_size + ";" + ext.Substring(1) + ";" + (((double)stopwatch.ElapsedTicks/(double)Stopwatch.Frequency)*1000)/ 50000);
                    stopwatch.Reset();
                }
            }
            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}
