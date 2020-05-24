using System;
using System.IO;
using System.Text;

namespace RICADO.Ini
{
    public enum enIniReadState : int
    {
        Closed,
        EndOfFile,
        Error,
        Initial,
        Interactive
    }

    public enum enItemType : int
    {
        Section,
        Key,
        Empty
    }

    public class IniReader : IDisposable
    {
        #region Private Locals

        int lineNumber = 1;
        int column = 1;
        enItemType iniType = enItemType.Empty;
        TextReader textReader = null;
        bool ignoreComments = false;
        StringBuilder name = new StringBuilder();
        StringBuilder value = new StringBuilder();
        StringBuilder comment = new StringBuilder();
        enIniReadState readState = enIniReadState.Initial;
        bool hasComment = false;
        bool disposed = false;
        bool lineContinuation = false;
        bool acceptCommentAfterKey = true;
        bool acceptNoAssignmentOperator = false;
        bool consumeAllKeyText = false;
        char[] commentDelimiters = new char[] { ';' };
        char[] assignDelimiters = new char[] { '=' };

        #endregion


        #region Public Properties

        public string Name
        {
            get { return this.name.ToString(); }
        }

        public string Value
        {
            get { return this.value.ToString(); }
        }

        public enItemType Type
        {
            get { return iniType; }
        }

        public string Comment
        {
            get { return (hasComment) ? this.comment.ToString() : null; }
        }

        public int LineNumber
        {
            get { return lineNumber; }
        }

        public int LinePosition
        {
            get { return column; }
        }

        public bool IgnoreComments
        {
            get { return ignoreComments; }
            set { ignoreComments = value; }
        }

        public enIniReadState ReadState
        {
            get { return readState; }
        }

        public bool LineContinuation
        {
            get { return lineContinuation; }
            set { lineContinuation = value; }
        }

        public bool AcceptCommentAfterKey
        {
            get { return acceptCommentAfterKey; }
            set { acceptCommentAfterKey = value; }
        }

        public bool AcceptNoAssignmentOperator
        {
            get { return acceptNoAssignmentOperator; }
            set { acceptNoAssignmentOperator = value; }
        }

        public bool ConsumeAllKeyText
        {
            get { return consumeAllKeyText; }
            set { consumeAllKeyText = value; }
        }

        #endregion


        #region Constructor

        public IniReader(string path)
        {
            textReader = new StreamReader(path);
        }

        #endregion


        #region Public Methods

        public bool Read()
        {
            bool result = false;

            if (readState != enIniReadState.EndOfFile
                || readState != enIniReadState.Closed)
            {
                readState = enIniReadState.Interactive;
                result = ReadNext();
            }

            return result;
        }

        public bool MoveToNextSection()
        {
            bool result = false;

            while (true)
            {
                result = Read();

                if (iniType == enItemType.Section || !result)
                {
                    break;
                }
            }

            return result;
        }

        public bool MoveToNextKey()
        {
            bool result = false;

            while (true)
            {
                result = Read();

                if (iniType == enItemType.Section)
                {
                    result = false;
                    break;
                }
                if (iniType == enItemType.Key || !result)
                {
                    break;
                }
            }

            return result;
        }

        public void Close()
        {
            Reset();
            readState = enIniReadState.Closed;

            if (textReader != null)
            {
                textReader.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public char[] GetCommentDelimiters()
        {
            char[] result = new char[commentDelimiters.Length];
            Array.Copy(commentDelimiters, 0, result, 0, commentDelimiters.Length);

            return result;
        }

        public void SetCommentDelimiters(char[] delimiters)
        {
            if (delimiters.Length < 1)
            {
                throw new ArgumentException("Must supply at least one delimiter");
            }

            commentDelimiters = delimiters;
        }

        public char[] GetAssignDelimiters()
        {
            char[] result = new char[assignDelimiters.Length];
            Array.Copy(assignDelimiters, 0, result, 0, assignDelimiters.Length);

            return result;
        }

        public void SetAssignDelimiters(char[] delimiters)
        {
            if (delimiters.Length < 1)
            {
                throw new ArgumentException("Must supply at least one delimiter");
            }

            assignDelimiters = delimiters;
        }

        #endregion


        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                textReader.Close();
                disposed = true;

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion


        #region Private Methods

        ~IniReader()
        {
            Dispose(false);
        }

        private void Reset()
        {
            this.name.Remove(0, this.name.Length);
            this.value.Remove(0, this.value.Length);
            this.comment.Remove(0, this.comment.Length);
            iniType = enItemType.Empty;
            hasComment = false;
        }

        private bool ReadNext()
        {
            bool result = true;
            int ch = PeekChar();
            Reset();

            if (IsComment(ch))
            {
                iniType = enItemType.Empty;
                ReadChar(); // consume comment character
                ReadComment();

                return result;
            }

            switch (ch)
            {
                case ' ':
                case '\t':
                case '\r':
                    SkipWhitespace();
                    ReadNext();
                    break;
                case '\n':
                    ReadChar();
                    break;
                case '[':
                    ReadSection();
                    break;
                case -1:
                    readState = enIniReadState.EndOfFile;
                    result = false;
                    break;
                default:
                    ReadKey();
                    break;
            }

            return result;
        }

        private void ReadComment()
        {
            int ch = -1;
            SkipWhitespace();
            hasComment = true;

            do
            {
                ch = ReadChar();
                this.comment.Append((char)ch);
            } while (!EndOfLine(ch));

            RemoveTrailingWhitespace(this.comment);
        }

        private void RemoveTrailingWhitespace(StringBuilder builder)
        {
            string temp = builder.ToString();

            builder.Remove(0, builder.Length);
            builder.Append(temp.TrimEnd(null));
        }

        private void ReadKey()
        {
            int ch = -1;
            iniType = enItemType.Key;

            while (true)
            {
                ch = PeekChar();

                if (IsAssign(ch))
                {
                    ReadChar();
                    break;
                }

                if (EndOfLine(ch))
                {
                    if (acceptNoAssignmentOperator)
                    {
                        break;
                    }
                    throw new IniException(this,
                        String.Format("Expected assignment operator ({0})",
                                        assignDelimiters[0]));
                }

                this.name.Append((char)ReadChar());
            }

            ReadKeyValue();
            SearchForComment();
            RemoveTrailingWhitespace(this.name);
        }

        private void ReadKeyValue()
        {
            int ch = -1;
            bool foundQuote = false;
            int characters = 0;
            SkipWhitespace();

            while (true)
            {
                ch = PeekChar();

                if (!IsWhitespace(ch))
                {
                    characters++;
                }

                if (!this.ConsumeAllKeyText && ch == '"')
                {
                    ReadChar();

                    if (!foundQuote && characters == 1)
                    {
                        foundQuote = true;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (foundQuote && EndOfLine(ch))
                {
                    throw new IniException(this, "Expected closing quote (\")");
                }

                // Handle line continuation
                if (lineContinuation && ch == '\\')
                {
                    StringBuilder buffer = new StringBuilder();
                    buffer.Append((char)ReadChar()); // append '\'

                    while (PeekChar() != '\n' && IsWhitespace(PeekChar()))
                    {
                        if (PeekChar() != '\r')
                        {
                            buffer.Append((char)ReadChar());
                        }
                        else
                        {
                            ReadChar(); // consume '\r'
                        }
                    }

                    if (PeekChar() == '\n')
                    {
                        // continue reading key value on next line
                        ReadChar();
                        continue;
                    }
                    else
                    {
                        // Replace consumed characters
                        this.value.Append(buffer.ToString());
                    }
                }

                if (!this.ConsumeAllKeyText)
                {
                    // If accepting comments then don't consume as key value
                    if (acceptCommentAfterKey && IsComment(ch) && !foundQuote)
                    {
                        break;
                    }
                }

                // Always break at end of line
                if (EndOfLine(ch))
                {
                    break;
                }

                this.value.Append((char)ReadChar());
            }

            if (!foundQuote)
            {
                RemoveTrailingWhitespace(this.value);
            }
        }

        private void ReadSection()
        {
            int ch = -1;
            iniType = enItemType.Section;
            ch = ReadChar(); // consume "["

            while (true)
            {
                ch = PeekChar();
                if (ch == ']')
                {
                    break;
                }
                if (EndOfLine(ch))
                {
                    throw new IniException(this, "Expected section end (])");
                }

                this.name.Append((char)ReadChar());
            }

            ConsumeToEnd(); // all after '[' is garbage			
            RemoveTrailingWhitespace(this.name);
        }

        private void SearchForComment()
        {
            int ch = ReadChar();

            while (!EndOfLine(ch))
            {
                if (IsComment(ch))
                {
                    if (ignoreComments)
                    {
                        ConsumeToEnd();
                    }
                    else
                    {
                        ReadComment();
                    }
                    break;
                }
                ch = ReadChar();
            }
        }

        private void ConsumeToEnd()
        {
            int ch = -1;

            do
            {
                ch = ReadChar();
            } while (!EndOfLine(ch));
        }

        private int ReadChar()
        {
            int result = textReader.Read();

            if (result == '\n')
            {
                lineNumber++;
                column = 1;
            }
            else
            {
                column++;
            }

            return result;
        }

        private int PeekChar()
        {
            return textReader.Peek();
        }

        private bool IsComment(int ch)
        {
            return HasCharacter(commentDelimiters, ch);
        }

        private bool IsAssign(int ch)
        {
            return HasCharacter(assignDelimiters, ch);
        }

        private bool HasCharacter(char[] characters, int ch)
        {
            bool result = false;

            for (int i = 0; i < characters.Length; i++)
            {
                if (ch == characters[i])
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool IsWhitespace(int ch)
        {
            return ch == 0x20 || ch == 0x9 || ch == 0xD || ch == 0xA;
        }

        private void SkipWhitespace()
        {
            while (IsWhitespace(PeekChar()))
            {
                if (EndOfLine(PeekChar()))
                {
                    break;
                }

                ReadChar();
            }
        }

        private bool EndOfLine(int ch)
        {
            return (ch == '\n' || ch == -1);
        }

        #endregion
    }
}
