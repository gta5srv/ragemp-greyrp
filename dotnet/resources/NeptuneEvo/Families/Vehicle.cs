using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;

namespace Golemo.Families
{
    public class Vehicle
    {
        public string FamilyCID { get; set; }
        public VehicleHash Model { get; set; }
        public Color Color { get; set; }
        public int AccessRank { get; set; }
        public int Fuel { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public List<nItem> Items = new List<nItem>();

        public Vehicle(string familycid, VehicleHash model, Color color, int accessrank, int fuel, Vector3 pos, Vector3 rot, List<nItem> items)
        {
            FamilyCID = familycid;
            Model = model;
            Color = color;
            AccessRank = accessrank;
            Fuel = fuel;
            Position = pos;
            Rotation = rot;
            Items = items;
        }
    }
}
