using System;

namespace RICADO.Ini
{
    public class IniException : SystemException
    {
        #region Private Locals

        private IniReader m_reader = null;
        private string m_message = "";

        #endregion


        #region Public Properties

        public int LinePosition
        {
            get
            {
                if (m_reader != null)
                {
                    return m_reader.LinePosition;
                }

                return 0;
            }
        }

        public int LineNumber
        {
            get
            {
                if (m_reader != null)
                {
                    return m_reader.LineNumber;
                }

                return 0;
            }
        }

        public override string Message
        {
            get
            {
                if (m_reader != null)
                {
                    return m_message + " - Line: " + LineNumber + " - Position: " + LinePosition;
                }

                return base.Message;
            }
        }

        #endregion


        #region Constructors

        public IniException()
            : base()
        {
            m_message = "Exception was Thrown";
        }

        public IniException(string message, Exception exception)
            : base(message, exception)
        {
        }

        public IniException(string message)
            : base(message)
        {
            m_message = message;
        }

        internal IniException(IniReader reader, string message)
            : this(message)
        {
            m_reader = reader;
            m_message = message;
        }

        #endregion
    }
}
