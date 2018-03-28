using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SharpCompress.Readers;

namespace RSPO
{
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
                throw new RCFileException("no file nor a stream supplied for import");
            }

            if (FileName != null && InputStream != null)
            {
                throw new RCFileException("file and a stream supplied for import");
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
                        // Console.WriteLine(String.Format("Entry:{0}", reader.Entry));
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
            actualStream.Close();
        }

        private XDocument _document = null;

        public void Import(bool onlyLoad=false)
        {
            XDocument doc = Document; // Загружает XML, еще необработанный, дерево.
            if (! onlyLoad) 
            {
                Console.WriteLine("Processing doc!");
                /*
                IEnumerable<XElement> proposals =
                    from p in doc.Elements()
                    select p;
                    */

                IEnumerable<XElement> proposals =
                    doc.Descendants(YName("offer"));

                XName internalId = YName("internal-id");
                foreach (XElement p in proposals)
                {
                    Console.WriteLine(
                        p.Attribute("internal-id").Value);
                    //Console.WriteLine(p.Name);
                }
            }
        }

        protected XName YName(string name)
        {
            return XName.Get(name, YandexNS);
        }

        protected readonly string YandexNS = @"http://webmaster.yandex.ru/schemas/feed/realty/2010-06";

    }

    // Exceptions

    public class RCException: Exception
    {
        public RCException(string message) : base(message)
        {
        }
    }

    public class RCFileException: RCException {
        public RCFileException(string message): base(message)
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
}
