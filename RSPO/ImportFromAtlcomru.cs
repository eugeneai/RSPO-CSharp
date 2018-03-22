using System;
using System.IO;
using System.IO.Compression;
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
        public Stream InputStream { get; set; } = null;
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
            if (FileName == null && InputStream == null)
            {
                throw new RCNoFileSuppliedException("no file nor a stream supplied for import");
            }
            if (FileName != null && InputStream == null)
            {
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
                                    InputStream = entry.Open();
                                    break;
                                }
                            }
                            if (InputStream == null)
                            {
                                throw new RCFormatException("could not find XML inside archive");
                            }
                        }
                    }
                    catch (InvalidDataException e)
                    {
                        // Doing nothing as this in not ZIP archive.
                        Console.WriteLine(String.Format("Trying ZIP: '{0}'", e));
                    }
                    if (InputStream == null)
                    {
                        // Open file directly
                        InputStream = File.Open(FileName, FileMode.Open);
                    }
                }
            };
            XDocument doc = XDocument.Load(InputStream);
        }

        private XDocument _document = null;

        public void Import()
        {
            XDocument doc = document; // Загружает XML, еще не обработанный, дерево.
            Console.WriteLine("Processing doc!");
        }
    }

}
