using System;
using System.Text;
using System.IO;

namespace RICADO.Ini
{
    public enum enIniWriteState : int
    {
        Start,
        BeforeFirstSection,
        Section,
        Closed
    };

    public class IniWriter : IDisposable
    {
        #region Private Locals

        private int m_indentation = 0;
        private bool m_useValueQuotes = false;
        private enIniWriteState m_writeState = enIniWriteState.Start;
        private char m_commentDelimiter = ';';
        private char m_assignDelimiter = '=';
        private TextWriter m_textWriter = null;
        private string m_eol = "\r\n";
        private StringBuilder m_indentationBuffer = new StringBuilder();
        private Stream m_baseStream = null;
        private bool m_disposed = false;

        #endregion


        #region Public Properties

        public int Indentation
        {
            get
            {
                return m_indentation;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Negative values are illegal");
                }

                m_indentation = value;

                m_indentationBuffer.Remove(0, m_indentationBuffer.Length);

                for (int i = 0; i < value; i++)
                {
                    m_indentationBuffer.Append(' ');
                }
            }
        }

        public bool UseValueQuotes
        {
            get
            {
                return m_useValueQuotes;
            }
            set
            {
                m_useValueQuotes = value;
            }
        }

        public enIniWriteState WriteState
        {
            get
            {
                return m_writeState;
            }
        }

        public char CommentDelimiter
        {
            get
            {
                return m_commentDelimiter;
            }
            set
            {
                m_commentDelimiter = value;
            }
        }

        public char AssignDelimiter
        {
            get
            {
                return m_assignDelimiter;
            }
            set
            {
                m_assignDelimiter = value;
            }
        }

        public Stream BaseStream
        {
            get
            {
                return m_baseStream;
            }
        }

        #endregion


        #region Constructor

        public IniWriter(string path)
        {
            m_textWriter = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None));

            StreamWriter streamWriter = m_textWriter as StreamWriter;

            if (streamWriter != null)
            {
                m_baseStream = streamWriter.BaseStream;
            }
        }

        #endregion


        #region Destructor

        ~IniWriter()
        {
            Dispose(false);
        }

        #endregion


        #region Public Methods

        public void Close()
        {
            m_textWriter.Close();
            m_writeState = enIniWriteState.Closed;
        }

        public void Flush()
        {
            m_textWriter.Flush();
        }

        public override string ToString()
        {
            return m_textWriter.ToString();
        }

        public void WriteSection(string section)
        {
            validateState();

            m_writeState = enIniWriteState.Section;

            writeLine("[" + section + "]");
        }

        public void WriteSection(string section, string comment)
        {
            validateState();

            m_writeState = enIniWriteState.Section;

            writeLine("[" + section + "]" + addComment(comment));
        }

        public void WriteKey(string key, string value)
        {
            validateStateKey();

            writeLine(key + " " + m_assignDelimiter + " " + getKeyValue(value));
        }

        public void WriteKey(string key, string value, string comment)
        {
            validateStateKey();

            writeLine(key + " " + m_assignDelimiter + " " + getKeyValue(value) + addComment(comment));
        }

        public void WriteEmpty()
        {
            validateState();

            if (m_writeState == enIniWriteState.Start)
            {
                m_writeState = enIniWriteState.BeforeFirstSection;
            }

            writeLine("");
        }

        public void WriteEmpty(string comment)
        {
            validateState();

            if (m_writeState == enIniWriteState.Start)
            {
                m_writeState = enIniWriteState.BeforeFirstSection;
            }

            if (comment == null)
            {
                writeLine("");
            }
            else
            {
                writeLine(m_commentDelimiter + " " + comment);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion


        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed == false)
            {
                if (m_textWriter != null)
                {
                    m_textWriter.Close();
                }

                if (m_baseStream != null)
                {
                    m_baseStream.Close();
                }

                m_disposed = true;

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion


        #region Private Methods

        private string getKeyValue(string text)
        {
            string result;

            if (m_useValueQuotes)
            {
                result = massageValue('"' + text + '"');
            }
            else
            {
                result = massageValue(text);
            }

            return result;
        }

        private void validateStateKey()
        {
            validateState();

            switch (m_writeState)
            {
                case enIniWriteState.BeforeFirstSection:
                case enIniWriteState.Start:
                    throw new InvalidOperationException("The WriteState is not Section");

                case enIniWriteState.Closed:
                    throw new InvalidOperationException("The writer is closed");
            }
        }

        private void validateState()
        {
            if (m_writeState == enIniWriteState.Closed)
            {
                throw new InvalidOperationException("The writer is closed");
            }
        }

        private string addComment(string text)
        {
            return (text == null) ? "" : (" " + m_commentDelimiter + " " + text);
        }

        private void write(string value)
        {
            m_textWriter.Write(m_indentationBuffer.ToString() + value);
        }

        private void writeLine(string value)
        {
            write(value + m_eol);
        }

        private string massageValue(string text)
        {
            return text.Replace("\n", "");
        }

        #endregion
    }
}
