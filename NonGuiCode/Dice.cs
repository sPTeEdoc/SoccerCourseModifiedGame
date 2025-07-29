using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGame
{
    public static class Dice
    {
        public static int d6()
        {
            return Die.Roll(6);
        }

        public static int d100()
        {
            return Die.Roll(100);
        }

        public static int d10()
        {
            return Die.Roll(9);
        }
        public static int d3()
        {
            return Die.Roll(3);
        }

        public static int d4()
        {
            return Die.Roll(4);
        }

        public static int d20()
        {
            return Die.Roll(20);
        }

        public static int d5()
        {
            return Die.Roll(5);
        }

        public static int d121()
        {
            return Die.Roll(121);
        }

        public static int d181()
        {
            return Die.Roll(181);
        }

        public static int d50()
        {
            return Die.Roll(50);
        }

        public static int d40()
        {
            return Die.Roll(40);
        }

        public static int d2()
        {
            return Die.Roll(2);
        }
    }
}
