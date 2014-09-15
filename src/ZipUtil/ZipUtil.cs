using System;
using System.Collections;
using System.IO;

namespace Oraion.CSharp.ZipLib.Zip
{
    public class ZipUtil
    {
        private ArrayList GenerateFileList(string p_directory)
        {
            ArrayList _files = new ArrayList();

            bool _isEmpty = true;
            foreach (string _file in Directory.GetFiles(p_directory))                   // add each file in directory
            {
                _files.Add(_file);
                _isEmpty = false;
            }

            if (_isEmpty == true)
            {
                if (Directory.GetDirectories(p_directory).Length == 0)                  // if directory is completely empty, add it
                {
                    _files.Add(p_directory + @"/");
                }
            }

            foreach (string _dirs in Directory.GetDirectories(p_directory))             // recursive
            {
                foreach (object _object in this.GenerateFileList(_dirs))
                {
                    _files.Add(_object);
                }
            }

            return _files;                                                              // return file list
        }

        public void ZipFiles(string p_infolder, string p_zipfile, string p_password)
        {
            ArrayList _files = this.GenerateFileList(p_infolder);                       // generate file list

            int _trimLength = (Directory.GetParent(p_infolder)).ToString().Length;      // find number of chars to remove - from orginal file path
            _trimLength += 1;                                                           //remove '\'

            FileStream _istream;
            byte[] _ibuffer;

            string _outpath = Path.Combine(p_infolder, p_zipfile);
            ZipOutputStream _ozipStream = new ZipOutputStream(File.Create(_outpath));   // create zip stream
            {
                if (String.IsNullOrEmpty(p_password) == false)
                    _ozipStream.Password = p_password;

                _ozipStream.UseZip64 = UseZip64.Off;
                _ozipStream.SetLevel(9);                                                // maximum compression
            }

            foreach (string _file in _files)                                            // for each file, generate a zipentry
            {
                ZipEntry _ozipEntry = new ZipEntry(_file.Remove(0, _trimLength));
                _ozipStream.PutNextEntry(_ozipEntry);

                if (_file.EndsWith(@"/") ==  false)                                     // if a file ends with '/' its a directory
                {
                    _istream = File.OpenRead(_file);
                    _ibuffer = new byte[_istream.Length];

                    _istream.Read(_ibuffer, 0, _ibuffer.Length);
                    _ozipStream.Write(_ibuffer, 0, _ibuffer.Length);
                }
            }

            _ozipStream.Finish();
            _ozipStream.Close();
        }

        public byte[] ZipStreams(Stream[] p_istreams, string[] p_filenames, string p_password)
        {
            byte[] _result = null;

            MemoryStream _ostream = new MemoryStream();

            ZipOutputStream _ozipStream = new ZipOutputStream(_ostream);                 // create zip stream
            {
                if (String.IsNullOrEmpty(p_password) == false)
                    _ozipStream.Password = p_password;

                _ozipStream.UseZip64 = UseZip64.Off;
                _ozipStream.SetLevel(9);                                                // maximum compression
            }

            for (int i = 0; i < p_istreams.Length; i++)
            {
                ZipEntry _ozipEntry = new ZipEntry(p_filenames[i]);
                _ozipStream.PutNextEntry(_ozipEntry);

                Stream _istream = p_istreams[i];
                byte[] _ibuffer = new byte[_istream.Length];

                _istream.Read(_ibuffer, 0, _ibuffer.Length);
                _ozipStream.Write(_ibuffer, 0, _ibuffer.Length);
            }

            _ozipStream.Finish();
            
            _ostream.Position = 0;
            _result = new byte[_ostream.Length];
            Array.Copy(_ostream.ToArray(), _result, _result.Length);

            _ozipStream.Close();

            return _result;
        }

        public void UnZipFiles(string p_zipfile, string p_outfolder, string p_password, bool p_deleteZip)
        {
            ZipInputStream _izipStream = new ZipInputStream(File.OpenRead(p_zipfile));
            if (String.IsNullOrEmpty(p_password) == false)
                _izipStream.Password = p_password;

            ZipEntry _izipEntry;
            while ((_izipEntry = _izipStream.GetNextEntry()) != null)
            {
                string _directory = p_outfolder;
                if (Directory.Exists(_directory) == false)
                    Directory.CreateDirectory(_directory);

                string _file = Path.GetFileName(_izipEntry.Name);
                if (String.IsNullOrEmpty(_file) == true)
                    continue;

                if (_izipEntry.Name.IndexOf(".ini") >= 0)
                    continue;

                string _fullPath = Path.Combine(_directory, _izipEntry.Name).Replace("\\ ", "\\");

                string _fullDirectory = Path.GetDirectoryName(_fullPath);
                if (Directory.Exists(_fullDirectory) == false)
                    Directory.CreateDirectory(_fullDirectory);

                FileStream _ostream = File.Create(_fullPath);

                int _size = 2048;
                byte[] _obuffer = new byte[_size];

                while (true)
                {
                    _size = _izipStream.Read(_obuffer, 0, _obuffer.Length);
                    if (_size <= 0)
                        break;

                    _ostream.Write(_obuffer, 0, _size);
                }

                _ostream.Close();
            }

            _izipStream.Close();
            
            if (p_deleteZip == true)
                File.Delete(p_zipfile);
        }
    }
}