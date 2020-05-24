using System;
using System.Collections.Generic;

namespace RICADO.Ini
{
    public class IniSection
    {
        #region Private Locals

        private Dictionary<string, IniItem> m_itemsList = new Dictionary<string, IniItem>();
        private string m_name = "";
        private string m_comment = null;

        #endregion


        #region Public Properties

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
        }

        public int ItemCount
        {
            get
            {
                return m_itemsList.Count;
            }
        }

        public Dictionary<string, IniItem> Items
        {
            get
            {
                return m_itemsList;
            }
        }

        #endregion


        #region Constructors

        public IniSection(string name, string comment)
        {
            m_name = name;
            m_comment = comment;
        }

        public IniSection(string name)
            : this(name, null)
        {
        }

        #endregion


        #region Public Methods

        public string GetValue(string key)
        {
            if (m_itemsList.ContainsKey(key))
            {
                return m_itemsList[key].Value;
            }
            else
            {
                return null;
            }
        }

        public string[] GetKeys()
        {
            List<string> keys = new List<string>();

            foreach (string key in m_itemsList.Keys)
            {
                keys.Add(key);
            }

            return keys.ToArray();
        }

        public bool ContainsKey(string key)
        {
            return m_itemsList.ContainsKey(key);
        }

        public void SetValue(string key, string value, string comment)
        {
            if (m_itemsList.ContainsKey(key))
            {
                m_itemsList[key].Value = value;
                m_itemsList[key].Comment = comment;
            }
            else
            {
                IniItem item = new IniItem(key, value, enItemType.Key, comment);
                m_itemsList.Add(key, item);
            }
        }

        public void SetValue(string key, string value)
        {
            SetValue(key, value, null);
        }

        public void RemoveValue(string key)
        {
            if (m_itemsList.ContainsKey(key))
            {
                m_itemsList.Remove(key);
            }
        }

        public Dictionary<string, IniItem>.Enumerator GetEnumerator()
        {
            return m_itemsList.GetEnumerator();
        }

        #endregion
    }
}
