using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    internal class LogWriter : StreamWriter
    {
        internal TextWriter ConsoleOut;

        public LogWriter(Stream stream, TextWriter cout) : base(stream)
        {
            ConsoleOut = cout;
        }

        public LogWriter(string path) : base(path)
        {
        }

        public LogWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public LogWriter(string path, bool append) : base(path, append)
        {
        }

        public LogWriter(Stream stream, Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize)
        {
        }

        public LogWriter(string path, bool append, Encoding encoding) : base(path, append, encoding)
        {
        }

        public LogWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen) : base(stream, encoding, bufferSize, leaveOpen)
        {
        }

        public LogWriter(string path, bool append, Encoding encoding, int bufferSize) : base(path, append, encoding, bufferSize)
        {
        }


        public override void Close()
        {
            base.Close();
            ConsoleOut.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ConsoleOut.Dispose();
        }

        public override void Flush()
        {
            base.Flush();
            ConsoleOut.Flush();
        }

        public override void Write(char value)
        {
            base.Write(value);
            ConsoleOut.Write(value);
        }
        public override void Write(char[] buffer)
        {
            base.Write(buffer);
            ConsoleOut.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            ConsoleOut.Write(buffer, index, (int)count);
        }
        public override void Write(string value)
        {
            base.Write(value);
            ConsoleOut.Write(value);
        }
        public override Task WriteAsync(char value)
        {
            ConsoleOut.WriteAsync(value);
            return base.WriteAsync(value);
        }

        public override Task WriteAsync(string value)
        {
            ConsoleOut.WriteAsync(value);
            return base.WriteAsync(value);
        }
               
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            ConsoleOut.WriteAsync(buffer, index, count);
            return base.WriteAsync(buffer, index, count);
        }

        public override Task WriteLineAsync()
        {
            ConsoleOut.WriteLineAsync();
            return base.WriteLineAsync();
        }

        public override Task WriteLineAsync(char value)
        {
            ConsoleOut.WriteLineAsync(value);
            return base.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(string value)
        {
            ConsoleOut.WriteLineAsync(value);
            return base.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            ConsoleOut.WriteLineAsync((char[])buffer, index, count);
            return base.WriteLineAsync((char[])buffer, index, count);
        }

        public override Task FlushAsync()
        {
            ConsoleOut.FlushAsync();
            return base.FlushAsync();
        }

    }
}
