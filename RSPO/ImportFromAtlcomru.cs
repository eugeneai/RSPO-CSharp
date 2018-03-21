using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace RSPO
{
    public class RCException: Exception
    {
        public RCException(string message) : base(message)
        {
        }
    }

    public class RCNoFileSuppliedException: RCException {
        public RCNoFileSuppliedException(string message): base(message)
        {
        }
    }

    public class RCFormatException: RCException
    {
        public RCFormatException(string message) : base(message)
        {
        }
    }

    public class ImportFromAtlcomru
    {
        public string FileName { get; set; } = null;
        public XDocument document {
            get
            {
                if (_document == null)
                {
                    LoadDocument(); // Must set _document to nonNull value or raise an Exception.
                }
                return _document;
            }
        }

        private void LoadDocument()
        {
            if (FileName == null)
            {
                throw new RCNoFileSuppliedException("no file supplied for import");
            }
            Stream stream = null;
            if (Path.GetExtension(FileName).ToUpper() == ".ZIP")
            {
                try
                {
                    using (ZipArchive archive = ZipFile.Open(FileName, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (Path.GetExtension(entry.FullName).ToUpper() == ".XML")
                            {
                                stream = entry.Open();
                                break;
                            }
                        }
                        if (stream == null)
                        {
                            throw new RCFormatException("could not find XML inside archive");
                        }
                    }
                } catch (InvalidDataException) {
                    // Doing nothing as this in not ZIP archive.
                }
                if (stream == null) {
                    // Open file directly
                    stream = File.Open(FileName, FileMode.Open);
                }
            }
            XDocument doc = XDocument.Load(stream);
        }

        private XDocument _document = null;

        public void Import()
        {
            XDocument doc = document; // Загружает XML, еще не обработанный, дерево.
            Console.WriteLine("Processing doc!");
        }
    }

}
