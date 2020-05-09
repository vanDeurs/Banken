using System;

namespace Server
{
    sealed class XmlDocumentException : Exception
    {
        public XmlDocumentException() { }
        public XmlDocumentException(string s) : base(s) { }
    }
}
