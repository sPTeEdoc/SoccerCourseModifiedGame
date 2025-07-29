using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux
{
    class LeagueCountry
    {
        public string m_CountryName;
        public List<League> leagues;

        public String CountryName
        {
            get
            {
                return m_CountryName;
            }
            set
            {
                m_CountryName = value;
            }
        }

        public LeagueCountry(string name)
        {
            this.m_CountryName = name;
            this.leagues = new List<League>();
        }
    }
}
