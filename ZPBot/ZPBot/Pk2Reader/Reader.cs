using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZPBot.Pk2Reader
{
    public class Reader : IDisposable
    {
        #region PK2Loading Algorithmus

        private string pFile;
        private Blowfish bf;
        private Dictionary<String, sFile> content;

        private bool pIsLoaded = false;

        public Reader()
        {
            bf = new Blowfish(new byte[] { 0x32, 0xCE, 0xDD, 0x7C, 0xBC, 0xA8 }, 0, 6);
            content = new Dictionary<string, sFile>();
        }
        public Reader(byte[] BlowfishKey)
        {
            bf = new Blowfish(BlowfishKey, 0, BlowfishKey.Length);
            content = new Dictionary<string, sFile>();
        }

        public Reader(string PK2Path)
        {
            pFile = PK2Path;

            bf = new Blowfish(new byte[] { 0x32, 0xCE, 0xDD, 0x7C, 0xBC, 0xA8 }, 0, 6);
            content = new Dictionary<string, sFile>();

            Load();
        }
        public Reader(string PK2Path, byte[] BlowfishKey)
        {
            pFile = PK2Path;

            bf = new Blowfish(BlowfishKey, 0, BlowfishKey.Length);
            content = new Dictionary<string, sFile>();

            Load();
        }

        public bool Load()
        {
            if (String.IsNullOrEmpty(pFile) == false && System.IO.File.Exists(pFile))
            {
                try
                {
                    FileStream fs = new FileStream(pFile, FileMode.Open, FileAccess.Read);

                    BinaryReader r = new BinaryReader(fs);

                    r.BaseStream.Position = 0;
                    content.Clear();

                    r.ReadChars(30);
                    r.ReadUInt32();
                    r.ReadUInt32();
                    r.BaseStream.Position += 218; //00-00-00..

                    //First Entry
                    byte[] buffer = r.ReadBytes(128);
                    bf.DecryptRev(buffer, 0, buffer, 0, 131);

                    using (BinaryReader rr = new BinaryReader(new MemoryStream(buffer)))
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

                    pIsLoaded = true;
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                pIsLoaded = false;
                return false;
            }
        }
        public bool Load(string PK2Path)
        {
            pFile = PK2Path;
            return Load();
        }

        public bool IsLoaded()
        {
            return pIsLoaded;
        }

        private string level = "\\";
        private bool ProcessFile(long startpos, ref BinaryReader r, ref byte[] buffer)
        {
            sPK2Entry entry = new sPK2Entry();
            r.BaseStream.Position = startpos;

            buffer = r.ReadBytes(128);
            if (buffer.GetUpperBound(0) != 0x80 - 1)
            {
                return false;
            }
            bf.DecryptRev(buffer, 0, buffer, 0, 131);

            using (BinaryReader rr = new BinaryReader(new MemoryStream(buffer)))
            {
                entry.Type = rr.ReadByte();
                for (int i = 1; i < 81; i++)
                {
                    byte b = rr.ReadByte();
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
                entry.size = rr.ReadUInt32();
                entry.nextChain = rr.ReadUInt32();
            }

            switch (entry.Type)
            {
                case 1: //Directory
                    if (entry.Name != "." && entry.Name != "..")
                    {
                        //Some name fixes
                        level = level + entry.Name + "\\";
                        level = level.ToLower();

                        //Store Directory EPosition
                        long currentpos = r.BaseStream.Position;

                        //Go to first Directory entry
                        r.BaseStream.Position = entry.PosLow;

                        //Read subfolders & files
                        while (ProcessFile(r.BaseStream.Position, ref r, ref buffer)) { }

                        r.BaseStream.Position = currentpos; //Back to Directory

                        //Restore Directorys root path
                        level = level.Remove(Len(level) - Len(entry.Name) - 1, Len(entry.Name) + 1);

                    }
                    break;

                case 2: //File
                    if (entry.size != 0 && String.IsNullOrEmpty(entry.Name) == false)
                    {
                        sFile file = new sFile();
                        file.Pos = entry.PosLow;
                        file.size = entry.size;
                        if (content.ContainsKey(level + entry.Name))
                        {
                            Console.WriteLine("Warning: File: " + level + entry.Name + " already exist.");
                            //return false;
                        }
                        else
                        {
                            content.Add(level + entry.Name.ToLower(), file);
                        }
                    }
                    break;

                default:
                    return false;
            }

            if (entry.nextChain > 0)
            {
                r.BaseStream.Position = entry.nextChain;
            }
            if (r.BaseStream.Position == entry.PosLow)
            {
                return false;
            }
            return true;
        }
        private int Len(string expression)
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

        public Dictionary<string, sFile> GetContent()
        {
            return content;
        }
        public int GetContentCount()
        {
            return content.Count;
        }

        public struct sFile
        {
            public uint Pos;
            public uint size;
        }
        private struct sPK2Entry
        {
            public byte Type;
            public string Name;
            public uint PosLow;
            public uint PosHigh;
            public uint size;
            public uint nextChain;
        }

        public void Unload()
        {
            pIsLoaded = false;
            content.Clear();
        }

        #endregion

        //Custom functions to get Files,Images etc...

        public byte[] GetFile(sFile file)
        {
            byte[] buffer;

            using (FileStream fs = new FileStream(pFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader r = new BinaryReader(fs))
                {
                    r.BaseStream.Position = file.Pos;
                    buffer = r.ReadBytes(Convert.ToInt32(file.size));
                }
            }
            return buffer;
        }

        public byte[] GetFile(string file)
        {
            byte[] buffer;

            file = file.ToLower();

            if (content.ContainsKey(file) == false)
            {
                return null;
            }

            using (FileStream fs = new FileStream(pFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader r = new BinaryReader(fs))
                {
                    r.BaseStream.Position = content[file].Pos;
                    buffer = r.ReadBytes(Convert.ToInt32(content[file].size));
                }
            }
            return buffer;
        }

        public void PrintContent(string filepath)
        {
            StreamWriter sw = new StreamWriter(filepath, false);
            foreach (string entry in content.Keys)
            {
                sw.WriteLine(entry);
            }
            sw.Close();
        }
        public void PrintContent()
        {
            FileInfo fi = new FileInfo(pFile);
            PrintContent(Environment.CurrentDirectory + "\\" + fi.Name.Replace(".pk2", ".txt"));
            fi = null;
        }

        #region IDisposable Member

        public void Dispose()
        {

            content.Clear();
            content = null;
            bf = null;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
