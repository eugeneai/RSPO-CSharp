using System;
using System.IO;
using System.Xml.Linq;
using SharpCompress.Common;
using SharpCompress.Readers;


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

    public class RCOperationException: RCException
    {
        public RCOperationException(string message) : base(message)
        {
        }
    }

    public class ImportFromAtlcomru
    {
        public string FileName { get; set; } = null;
        public Stream InputStream { get; set; } = null;
        public XDocument Document {
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

            if (FileName != null && InputStream != null)
            {
                throw new RCNoFileSuppliedException("file and a stream supplied for import");
            }

            if (FileName != null)
            {
                InputStream=File.Open(FileName, FileMode.Open);
            }

            // Check wether InputStream is an archive

            Stream actualStream = null;
            try
            {
                IReader reader = ReaderFactory.Open(InputStream);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Console.WriteLine(String.Format("Entry:{0}", reader.Entry));
                        actualStream=reader.OpenEntryStream();
                        break;
                    }
                }
                if (actualStream == null)
                {
                    throw new RCFormatException("cannot find a file in archive");
                }
            }
            catch (InvalidOperationException)
            {
                actualStream=InputStream;
                if (actualStream.CanSeek)
                {
                    actualStream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    throw new RCOperationException("cannot seek file to reset reading");
                }
            }

            _document = XDocument.Load(actualStream);
        }

        private XDocument _document = null;

        public void Import()
        {
            XDocument doc = Document; // Загружает XML, еще необработанный, дерево.
            Console.WriteLine("Processing doc!");
        }
    }
}
