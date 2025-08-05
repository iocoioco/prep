using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre_Processor
{
    internal class wr
    {
        public static void create_empty_temp_file()
        {
            string path = @"C:\BJS\temp.txt";

            if (File.Exists(path))
                File.Delete(path);

            if (!File.Exists(path))
                File.Create(path).Dispose();
        }

        public static void w(double[] a)
        {

                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                string str = "";
                foreach (var t in a)
                {
                    str += "\t" + t.ToString();
                }

                sw.WriteLine("{0}", str);
                sw.Close();
         
        }


        public static void w(List<List<double>> double_list)
        {
      
                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                string str = "";
                foreach (var t in double_list)
                {
                    str += t[0] + "/" + t[1] + "\t";
                }

                sw.WriteLine("{0}", str);
                sw.Close();
     
        }


        public static void w(string t)
        {
  
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.WriteLine("{0}", t);
                sw.Close();
  
        }


        public static void w(string[] t)
        {
  
                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                // sw.WriteLine(time_now);
                for (int i = 0; i < t.Length; i++)
                {
                    sw.Write(t[i]);
                    sw.Write("\t");
                    //if(i < t.Length - 1)
                    //    sw.Write("\t");
                    //else
                    //    sw.Write("\n");
                }
                //sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Write("\n");
                sw.Close();

        }


        public static void w(List<string> GL_title, List<List<string>> GL)
        {

                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                string str = "";
                for (int i = 0; i < GL.Count; i++)
                {
                    str += GL_title[i];
                    foreach (var u in GL[i])
                    {
                        str += "\t" + u + "\n";
                    }
                    str += "\n";
                }

                sw.WriteLine("{0}", str);
                sw.Close();

        }

        public static void w(int[,] x, int start_line, int end_line)
        {

                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                for (int i = start_line; i <= end_line; i++)
                {
                    if (i < 0)
                    {
                        break;
                    }
                    for (int j = 0; j < 12; j++)
                    {
                        sw.Write("{0, 10}", x[i, j]);
                    }
                    sw.Write("\n");
                }
                sw.Close();

        }

        public static void wt(string t)
        {

                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);


                sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Close();

        }

        public static void wt(string[] t)
        {

                string time_now = DateTime.Now.ToString("hh:mm:ss.ffff");
                string path = @"C:\BJS\temp.txt";
                StreamWriter sw = File.AppendText(path);

                sw.WriteLine(time_now);
                for (int i = 0; i < t.Length; i++)
                {
                    sw.WriteLine(t[i]);
                }
                sw.WriteLine("{0}\t{1}", t, time_now);
                sw.Close();

        }
    }
}
