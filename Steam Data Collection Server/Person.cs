using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam_Data_Collection
{
    class Person
    {
        public DateTime DateCollected { get; set; }
        public Int64 ID64 { get; set; }
        public String Username { get; set; }
        public List<GameHistory> ListOfGames { get; set; }
    }
}
