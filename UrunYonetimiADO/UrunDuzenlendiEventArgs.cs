using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrunYonetimiADO.Models;

namespace UrunYonetimiADO
{
    public class UrunDuzenlendiEventArgs : EventArgs
    {
        public Urun EskiUrun { get; set; }
        public Urun YeniUrun { get; set; }
    }
}
