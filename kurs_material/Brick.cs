using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_material
{
    public class Brick : MainBrick
    {
        private List<int> ListAmountInM2 = new List<int>();

        public Brick(string name, string type, double inpack)
        {
            Name = name;
            Type = type;
            InPack = inpack;
            countAmountInM2(Type);
        }

        public int countAmountInM2(string type)
        {
            if (Type == "Одинарный")
            {

                ListAmountInM2.AddRange(new int[4] { 102, 153, 204, 255 });
                switch (type)
                {
                    case "В 1 кирпич":
                        return 102;
                    case "В 1.5 кирпича":
                        return 153;
                    case "В 2 кирпича":
                        return 204;
                    case "В 2.5 кирпича":
                        return 255;
                }
            }
            if (Type == "Полуторный")
            {
                ListAmountInM2.AddRange(new int[4] { 78, 117, 156, 195 });
                switch (type)
                {
                    case "В 1 кирпич":
                        return 78;
                    case "В 1.5 кирпича":
                        return 117;
                    case "В 2 кирпича":
                        return 156;
                    case "В 2.5 кирпича":
                        return 195;
                }

            }
            if (Type == "Двойной")
            {
                switch (type)
                {
                    case "В 1 кирпич": 
                        return 52;
                    case "В 1.5 кирпича":
                        return 78;
                    case "В 2 кирпича":
                        return 104;
                    case "В 2.5 кирпича":
                        return 130;
                }
            }
            return 0;
        }
    }
}
