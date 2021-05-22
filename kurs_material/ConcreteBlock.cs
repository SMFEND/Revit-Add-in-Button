using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_material
{
    class ConcreteBlock
    {
        private string _Type;
        public string Name { get; set; }
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
                if (Type == "Для несущих")
                    Volume = 0.014; // 390 * 190 * 190
                else
                    Volume = 0.088; // 390 * 120 * 188; 
            }
        }
        public double Volume;
    }
}
