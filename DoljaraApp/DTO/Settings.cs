using System.Collections.Generic;

namespace _DoljaraApp
{
    class Settings
    {
        public int reloadSec { get; set; }

        public string APIserverURL { get; set; }

        public string WSserverURL { get; set; }

        public List<Idol> allIdols { get; set; }
    }
}
