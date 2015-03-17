using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools.Objects
{
    class Item
    {
        internal Item() { }
        internal Item(int address, ushort id, ushort count, byte containerNumber, byte slot)
        {
            Address = address;
            ID = id;
            Count = count;
            ContainerNumber = containerNumber;
            Slot = slot;
        }

        internal int Address { get; set; }
        internal ushort ID { get; set; }
        internal ushort Count { get; set; }
        internal byte ContainerNumber { get; set; }
        internal byte Slot { get; set; }
        internal Container Parent
        {
            get
            {
                if (ContainerNumber >= 0x40) { return Container.GetContainer((byte)(ContainerNumber - 0x40)); }
                return null;
            }
        }
    }
}
