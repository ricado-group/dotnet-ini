using System;

namespace RICADO.Ini
{
    public class IniItem
    {
        #region Private Locals

        private enItemType m_type = enItemType.Empty;
        private string m_name = "";
        private string m_value = "";
        private string m_comment = null;

        #endregion


        #region Public Properties

        public enItemType Type
        {
            get
            {
                return m_type;
            }
        }

        public string Value
        {
            get
            {
                return m_value;
            }
            internal set
            {
                m_value = value;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Comment
        {
            get
            {
                return m_comment;
            }
            internal set
            {
                m_comment = value;
            }
        }

        #endregion


        #region Constructor

        internal protected IniItem(string name, string value, enItemType type, string comment)
        {
            m_name = name;
            m_value = value;
            m_type = type;
            m_comment = comment;
        }

        #endregion
    }
}
