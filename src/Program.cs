using System;
using System.IO;

namespace ZipFile
{
    class Program
    {
        private CSharp.ZipLib.Zip.ZipUtil m_ziplib = null;
        private CSharp.ZipLib.Zip.ZipUtil ZipLib
        {
            get
            {
                if (m_ziplib == null)
                    m_ziplib = new CSharp.ZipLib.Zip.ZipUtil();

                return m_ziplib;
            }
        }

        public void Zip(string p_srcfile, string p_password)
        {
            if (File.Exists(p_srcfile) == false)
            {
                Console.WriteLine("file not found: {0}", p_srcfile);
                return;
            }

            using (var _source = new MemoryStream(File.ReadAllBytes(p_srcfile)))
            {
                byte[] _obuffer = ZipLib.ZipStreams
                (
                    new Stream[] { _source },
                    new string[] { Path.GetFileName(p_srcfile) },
                    p_password
                );

                string _zipfile = Path.ChangeExtension(p_srcfile, "zip");
                if (File.Exists(_zipfile) == true)
                {
                    Console.Write("\nalready exist file: {0}\nenter key to overwrite(o)....", p_srcfile);
                    ConsoleKeyInfo _key = Console.ReadKey();
                    if (_key.Key != ConsoleKey.O)
                    {
                        Console.WriteLine("\nfailure zip file....");
                        return;
                    }
                }

                File.WriteAllBytes(_zipfile, _obuffer);
                Console.WriteLine("\nsuccess zip file....");
            }
        }

        static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                Program _p = new Program();
                _p.Zip(args[0], args[1]);
            }
            else
            {
                Console.WriteLine("usage: {0} filename password", System.AppDomain.CurrentDomain.FriendlyName);
            }
        }
    }
}