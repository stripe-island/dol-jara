using System.Collections.Generic;

namespace _DoljaraApp
{
    class Audition
    {
        public Producer eastP { get; set; }

        public Producer westP { get; set; }

        public Producer southP { get; set; }

        public Producer northP { get; set; }

        public List<Idol> applicants { get; set; }
    }
}
