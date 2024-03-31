using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace makesetup
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid parameters");
                return;
            }

            Console.WriteLine(String.Format("Starting merge\narchive: {0}\nsetup: {1}", args[0], args[1]));
            try
            {
                using (FileStream archive = File.Open(args[0], FileMode.Open, FileAccess.Read))
                {
                    using (FileStream setup = File.Open(args[1], FileMode.Append, FileAccess.Write))
                    {
                        long insertPoint = setup.Length;
                        byte[] b = new byte[2048];
                        int bytes = 0;
                        while ((bytes = archive.Read(b, 0, b.Length)) > 0)
                        {
                            setup.Write(b, 0, bytes);
                        }
                        b[0] = (byte)((insertPoint >> 32) & 0xFF);
                        b[1] = (byte)((insertPoint >> 16) & 0xFF);
                        b[2] = (byte)((insertPoint >> 8) & 0xFF);
                        b[3] = (byte)(insertPoint & 0xFF);
                        setup.Write(b, 0, 4);
                    }
                }
                Console.WriteLine("Completed successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine("Completed with errors: " + e.Message);
            }
        }
    }
}
