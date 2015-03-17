using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools.Objects
{
    class Container
    {
        private int Address;

        internal Container(int address, byte orderNumber)
        {
            this.Address = address;
            OrderNumber = orderNumber;
        }

        #region methods
        internal List<Item> Items
        {
            get
            {
                List<Item> listItems = new List<Item>();
                if (isOpen)
                {
                    for (byte i = 0; i < ItemsAmount; i++)
                    {
                        Item item = new Item();
                        item.Address = Address + Addresses.Container.ItemStep * i;
                        item.Count = Memory.ReadByte(Address + Addresses.Container.ItemStep * i + Addresses.Container.Distances.ItemCount);
                        item.ContainerNumber = (byte)(OrderNumber + 0x40);
                        item.ID = Memory.ReadUShort(Address + Addresses.Container.ItemStep * i + Addresses.Container.Distances.ItemID);
                        item.Slot = i;
                        listItems.Add(item);
                    }
                }
                return listItems;
            }
        }

        internal Item GetItem(ushort id)
        {
            if (isOpen)
            {
                foreach (Item item in Items)
                {
                    if (item.ID == id) { return item; }
                }
            }
            return null;
        }

        internal Item GetItem(ushort id, byte count)
        {
            if (isOpen)
            {
                foreach (Item item in Items)
                {
                    if (item.ID == id && item.Count == count) { return item; }
                }
            }
            return null;
        }

        internal Item GetItem(byte slot)
        {
            if (isOpen)
            {
                foreach (Item item in Items)
                {
                    if (item.Slot == slot) { return item; }
                }
            }
            return null;
        }

        internal bool isOpen
        {
            get { return Memory.ReadByte(Address + Addresses.Container.Distances.isOpen) == 1; }
        }

        internal byte OrderNumber { get; set; }

        internal uint ID
        {
            get { return Memory.ReadUInt(Address + Addresses.Container.Distances.ID); }
            set { Memory.WriteUInt32(Address + Addresses.Container.Distances.ID, value); }
        }

        internal byte ItemsAmount
        {
            get { return Memory.ReadByte(Address + Addresses.Container.Distances.AmountOfItems); }
        }

        internal byte Slots
        {
            get { return Memory.ReadByte(Address + Addresses.Container.Distances.Slots); }
        }

        internal bool isFull
        {
            get { return ItemsAmount == Slots; }
        }

        internal string Name
        {
            get { return Memory.ReadString(Address + Addresses.Container.Distances.Name); }
            set { Memory.WriteString(Address + Addresses.Container.Distances.Name, value); }
        }
        #endregion

        #region static methods
        internal static List<Container> GetContainers()
        {
            List<Container> containers = new List<Container>();
            byte j = 0;
            for (int i = Addresses.Container.Begin; i <= Addresses.Container.End; i += Addresses.Container.Step)
            {
                if (Memory.ReadByte(i + Addresses.Container.Distances.isOpen) == 1)
                {
                    containers.Add(new Container(i, j));
                }
                j++;
            }
            return containers;
        }

        internal static Container GetContainer(byte containerNumber)
        {
            int address = Addresses.Container.Begin + Addresses.Container.Step * containerNumber;
            return new Container(address, containerNumber);
        }
        #endregion
    }
}
