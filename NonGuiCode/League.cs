using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux
{
    public class League
    {
        public string m_LeagueName;
        public int tier;
        public String LeagueName
        {
            get
            {
                return m_LeagueName;
            }
        }

        public List<Team> teams = new List<Team>();

        public League(string lgName)
        {
            this.m_LeagueName = lgName;
        }
    }
}
