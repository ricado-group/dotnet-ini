using System;
using System.IO;

namespace RICADO.Ini
{
    public class IniDocument
    {
        #region Private Locals

        private IniSectionCollection m_sections = new IniSectionCollection();

        #endregion


        #region Public Properties

        public IniSectionCollection Sections
        {
            get
            {
                return m_sections;
            }
        }

        #endregion


        #region Constructors

        public IniDocument()
        {
        }

        public IniDocument(string path)
        {
            Load(path);
        }

        #endregion


        #region Public Methods

        public void Load(string path)
        {
            loadReader(new IniReader(path));
        }

        public void Save(string path)
        {
            IniWriter writer = new IniWriter(path);

            foreach (IniSection section in m_sections)
            {
                writer.WriteSection(section.Name, section.Comment);

                foreach (IniItem item in section.Items.Values)
                {
                    switch (item.Type)
                    {
                        case enItemType.Key:
                            writer.WriteKey(item.Name, item.Value, item.Comment);
                            break;
                        case enItemType.Empty:
                            writer.WriteEmpty(item.Comment);
                            break;
                    }
                }
            }

            writer.Close();
        }

        #endregion


        #region Private Methods

        private void loadReader(IniReader reader)
        {
            reader.IgnoreComments = false;
            IniSection section = null;

            try
            {
                while (reader.Read())
                {
                    switch (reader.Type)
                    {
                        case enItemType.Section:
                            if (m_sections[reader.Name] != null)
                            {
                                m_sections.Remove(reader.Name);
                            }

                            section = new IniSection(reader.Name, reader.Comment);
                            m_sections.Add(section);
                            break;

                        case enItemType.Key:
                            if (section.GetValue(reader.Name) == null)
                            {
                                section.SetValue(reader.Name, reader.Value, reader.Comment);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
            }
        }

        #endregion
    }
}
