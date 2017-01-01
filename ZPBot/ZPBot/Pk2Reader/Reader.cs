using System;
using System.Collections.Generic;
using System.IO;
using ZPBot.Annotations;

namespace ZPBot.Pk2Reader
{
    public class Reader : IDisposable
    {
        #region PK2Loading Algorithmus

        private string _pFile;
        private Blowfish _bf;
        private Dictionary<string, SFile> _content;

        private bool _pIsLoaded;

        public Reader()
        {
            _bf = new Blowfish(new byte[] { 0x32, 0xCE, 0xDD, 0x7C, 0xBC, 0xA8 }, 0, 6);
            _content = new Dictionary<string, SFile>();
        }
        public Reader([NotNull] byte[] blowfishKey)
        {
            _bf = new Blowfish(blowfishKey, 0, blowfishKey.Length);
            _content = new Dictionary<string, SFile>();
        }

        public Reader(string pk2Path)
        {
            _pFile = pk2Path;

            _bf = new Blowfish(new byte[] { 0x32, 0xCE, 0xDD, 0x7C, 0xBC, 0xA8 }, 0, 6);
            _content = new Dictionary<string, SFile>();

            Load();
        }
        public Reader(string pk2Path, [NotNull] byte[] blowfishKey)
        {
            _pFile = pk2Path;

            _bf = new Blowfish(blowfishKey, 0, blowfishKey.Length);
            _content = new Dictionary<string, SFile>();

            Load();
        }

        public bool Load()
        {
            if (string.IsNullOrEmpty(_pFile) == false && File.Exists(_pFile))
            {
                try
                {
                    var fs = new FileStream(_pFile, FileMode.Open, FileAccess.Read);

                    var r = new BinaryReader(fs);

                    r.BaseStream.Position = 0;
                    _content.Clear();

                    r.ReadChars(30);
                    r.ReadUInt32();
                    r.ReadUInt32();
                    r.BaseStream.Position += 218; //00-00-00..

                    //First Entry
                    var buffer = r.ReadBytes(128);
                    _bf.DecryptRev(buffer, 0, buffer, 0, 131);

                    using (var rr = new BinaryReader(new MemoryStream(buffer)))
                    {
                        rr.ReadByte(); //Type
                        rr.ReadChars(81); //Name
                        rr.ReadBytes(24);
                        rr.ReadUInt32(); //PosLow
                        rr.ReadUInt32(); //PosHigh
                        rr.ReadUInt32(); //Size
                        rr.ReadUInt32(); //NextChain                        
                    }

                    while (ProcessFile(r.BaseStream.Position, ref r, ref buffer)) { }

                    r.Close();
                    fs.Close();

                    _pIsLoaded = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Pk2Reader" + ex.Message);
                    return false;
                }
            }
            else
            {
                _pIsLoaded = false;
                return false;
            }
        }
        public bool Load(string pk2Path)
        {
            _pFile = pk2Path;
            return Load();
        }

        public bool IsLoaded()
        {
            return _pIsLoaded;
        }

        private string _level = "\\";
        private bool ProcessFile(long startpos, ref BinaryReader r, ref byte[] buffer)
        {
            var entry = new SPk2Entry();
            r.BaseStream.Position = startpos;

            buffer = r.ReadBytes(128);
            if (buffer.GetUpperBound(0) != 0x80 - 1)
            {
                return false;
            }
            _bf.DecryptRev(buffer, 0, buffer, 0, 131);

            using (var rr = new BinaryReader(new MemoryStream(buffer)))
            {
                entry.Type = rr.ReadByte();
                for (var i = 1; i < 81; i++)
                {
                    var b = rr.ReadByte();
                    if (b >= 0x20 && b <= 0x7E)
                    {
                        entry.Name += Convert.ToChar(b);
                    }
                    else
                    {
                        rr.BaseStream.Position = 82;
                        break; //Exit For
                    }
                }

                rr.BaseStream.Position += 24;
                entry.PosLow = rr.ReadUInt32();
                entry.PosHigh = rr.ReadUInt32();
                entry.Size = rr.ReadUInt32();
                entry.NextChain = rr.ReadUInt32();
            }

            switch (entry.Type)
            {
                case 1: //Directory
                    if (entry.Name != "." && entry.Name != "..")
                    {
                        //Some name fixes
                        _level = _level + entry.Name + "\\";
                        _level = _level.ToLower();

                        //Store Directory EPosition
                        var currentpos = r.BaseStream.Position;

                        //Go to first Directory entry
                        r.BaseStream.Position = entry.PosLow;

                        //Read subfolders & files
                        while (ProcessFile(r.BaseStream.Position, ref r, ref buffer)) { }

                        r.BaseStream.Position = currentpos; //Back to Directory

                        //Restore Directorys root path
                        _level = _level.Remove(Len(_level) - Len(entry.Name) - 1, Len(entry.Name) + 1);

                    }
                    break;

                case 2: //File
                    if (entry.Size != 0 && string.IsNullOrEmpty(entry.Name) == false)
                    {
                        var file = new SFile
                        {
                            Pos = entry.PosLow,
                            Size = entry.Size
                        };
                        if (_content.ContainsKey(_level + entry.Name))
                        {
                            Console.WriteLine(@"Warning: File: " + _level + entry.Name + @" already exist.");
                            //return false;
                        }
                        else
                        {
                            _content.Add(_level + entry.Name.ToLower(), file);
                        }
                    }
                    break;

                default:
                    return false;
            }

            if (entry.NextChain > 0)
            {
                r.BaseStream.Position = entry.NextChain;
            }
            if (r.BaseStream.Position == entry.PosLow)
            {
                return false;
            }
            return true;
        }
        private int Len([CanBeNull] string expression)
        {
            if (expression == null)
            {
                return 0;
            }
            else
            {
                return expression.Length;
            }
        }

        public Dictionary<string, SFile> GetContent()
        {
            return _content;
        }
        public int GetContentCount()
        {
            return _content.Count;
        }

        public struct SFile
        {
            public uint Pos;
            public uint Size;
        }
        private struct SPk2Entry
        {
            public byte Type;
            public string Name;
            public uint PosLow;
            public uint PosHigh;
            public uint Size;
            public uint NextChain;
        }

        public void Unload()
        {
            _pIsLoaded = false;
            _content.Clear();
        }

        #endregion

        //Custom functions to get Files,Images etc...

        [NotNull]
        public byte[] GetFile(SFile file)
        {
            byte[] buffer;

            using (var fs = new FileStream(_pFile, FileMode.Open, FileAccess.Read))
            {
                using (var r = new BinaryReader(fs))
                {
                    r.BaseStream.Position = file.Pos;
                    buffer = r.ReadBytes(Convert.ToInt32(file.Size));
                }
            }
            return buffer;
        }

        [CanBeNull]
        public byte[] GetFile(string file)
        {
            byte[] buffer;

            file = file.ToLower();

            if (_content.ContainsKey(file) == false)
            {
                return null;
            }

            using (var fs = new FileStream(_pFile, FileMode.Open, FileAccess.Read))
            {
                using (var r = new BinaryReader(fs))
                {
                    r.BaseStream.Position = _content[file].Pos;
                    buffer = r.ReadBytes(Convert.ToInt32(_content[file].Size));
                }
            }
            return buffer;
        }

        public void PrintContent([NotNull] string filepath)
        {
            var sw = new StreamWriter(filepath, false);
            foreach (var entry in _content.Keys)
            {
                sw.WriteLine(entry);
            }
            sw.Close();
        }

        public void PrintContent()
        {
            var fi = new FileInfo(_pFile);
            PrintContent(Environment.CurrentDirectory + "\\" + fi.Name.Replace(".pk2", ".txt"));
        }

        #region IDisposable Member

        public void Dispose()
        {

            _content.Clear();
            _content = null;
            _bf = null;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
