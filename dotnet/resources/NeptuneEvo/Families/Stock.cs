using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;

namespace Golemo.Families
{
    public class Stocks
    {
        public string FamilyCID { get; set; }
        public bool isStock { get; set; } = false;
        public int MaxAmount { get; set; }
        public int Dimension { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public List<nItem> Items = new List<nItem>();

        public Stocks(string familycid, bool isstock, int maxamount, Vector3 pos, Vector3 rot, List<nItem> items, int dimension)
        {
            FamilyCID = familycid;
            isStock = isstock;
            MaxAmount = maxamount;
            Position = pos;
            Rotation = rot;
            Items = items;
            Dimension = dimension;
        }
    }
}
