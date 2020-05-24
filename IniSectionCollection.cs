using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Ini
{
    public class IniSectionCollection : List<IniSection>
    {
        #region Public Properties

        public IniSection this[string section]
        {
            get
            {
                if (this.Count(sec => sec.Name == section) > 0)
                {
                    return this.First(sec => sec.Name == section);
                }

                return null;
            }
        }

        #endregion


        #region Public Methods

        public void Remove(string section)
        {
            if (this.Count(sec => sec.Name == section) > 0)
            {
                this.Remove(this.First(sec => sec.Name == section));
            }
        }

        public bool Contains(string section)
        {
            if (this.Count(sec => sec.Name == section) > 0)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
