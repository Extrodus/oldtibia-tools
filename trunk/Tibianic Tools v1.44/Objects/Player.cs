using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TibianicTools
{
    class Player
    {
        internal Player()
        {
            SetBattlelistAddress();
        }

        internal int BattlelistAddress { get; set; }

        internal void SetBattlelistAddress()
        {
            if (Connected)
            {
                uint playerID = ID;
                for (int i = Addresses.Battlelist.Begin; i < Addresses.Battlelist.End; i += Addresses.Battlelist.Step)
                {
                    if (Memory.ReadUInt(i + Addresses.Battlelist.Distances.ID) == playerID) { BattlelistAddress = i; break; }
                }
            }
        }

        internal bool Connected
        {
            get { return Memory.ReadByte(Addresses.Client.Connection) == 8; }
        }

        internal uint ID
        {
            get { return Memory.ReadUInt(Addresses.Player.PlayerID); }
            set { Memory.WriteUInt32(Addresses.Player.PlayerID, value); }
        }

        internal uint Experience
        {
            get { return Memory.ReadUInt(Addresses.Player.Exp); }
            set { Memory.WriteUInt32(Addresses.Player.Exp, value); }
        }

        internal uint Level
        {
            get { return Memory.ReadUInt(Addresses.Player.Level); }
            set { Memory.WriteUInt32(Addresses.Player.Level, value); }
        }

        internal int LevelPercent
        {
            get { return Memory.ReadInt(Addresses.Player.LevelPercent); }
            set { Memory.WriteInt32(Addresses.Player.LevelPercent, value); }
        }

        internal ushort Health
        {
            get { return Memory.ReadUShort(Addresses.Player.HP); }
            set { Memory.WriteUShort(Addresses.Player.HP, value); }
        }

        internal ushort HealthMax
        {
            get { return Memory.ReadUShort(Addresses.Player.MaxHP); }
            set { Memory.WriteUShort(Addresses.Player.MaxHP, value); }
        }

        internal byte HealthPercent
        {
            get { return (byte)((double)Health / HealthMax * 100); }
        }

        internal ushort Mana
        {
            get { return Memory.ReadUShort(Addresses.Player.Mana); }
            set { Memory.WriteUShort(Addresses.Player.Mana, value); }
        }

        internal ushort ManaMax
        {
            get { return Memory.ReadUShort(Addresses.Player.MaxMana); }
            set { Memory.WriteUShort(Addresses.Player.MaxMana, value); }
        }

        internal double ManaPercent
        {
            get { return Math.Round(((double)Mana / (double)ManaMax) * 100); }
        }

        internal uint Soul
        {
            get { return Memory.ReadUInt(Addresses.Player.Soul); }
            set { Memory.WriteUInt32(Addresses.Player.Soul, value); }
        }

        internal uint Cap
        {
            get { return Memory.ReadUInt(Addresses.Player.Cap); }
            set { Memory.WriteUInt32(Addresses.Player.Cap, value); }
        }

        /*internal uint Stamina
        {
            get { return Memory.ReadUInt(Addresses.Player.Stamina); }
            set { Memory.WriteUInt32(Addresses.Player.Stamina, value); }
        }*/

        internal ushort X
        {
            get { return Memory.ReadUShort(Addresses.Player.PosX); }
            set { Memory.WriteUShort(Addresses.Player.PosX, value); }
        }

        internal ushort Y
        {
            get { return Memory.ReadUShort(Addresses.Player.PosY); }
            set { Memory.WriteUShort(Addresses.Player.PosY, value); }
        }

        internal byte Z
        {
            get { return Memory.ReadByte(Addresses.Player.PosZ); }
            set { Memory.WriteByte(Addresses.Player.PosZ, value); }
        }

        internal Structs.Pos Pos
        {
            get { return new Structs.Pos(X, Y, Z); }
        }

        internal byte Direction
        {
            get { return Memory.ReadByte(BattlelistAddress + Addresses.Battlelist.Distances.Direction); }
            set { Memory.WriteByte(BattlelistAddress + Addresses.Battlelist.Distances.Direction, value); }
        }

        internal string Name
        {
            get { return Memory.ReadString(BattlelistAddress + Addresses.Battlelist.Distances.Name); }
            set { Memory.WriteString(BattlelistAddress + Addresses.Battlelist.Distances.Name, value); }
        }

        internal uint WalkSpeed
        {
            get { return Memory.ReadUInt(BattlelistAddress + Addresses.Battlelist.Distances.WalkSpeed); }
            set { Memory.WriteUInt32(BattlelistAddress + Addresses.Battlelist.Distances.WalkSpeed, value); }
        }

        /*internal Objects.Item GetItemInSlot(Enums.EquipmentSlots Slot)
        {
            switch (Slot)
            {
                case Enums.EquipmentSlots.Ammo:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqAmmo), Memory.ReadUShort(Addresses.Player.EqAmmo + 4));
                case Enums.EquipmentSlots.Body:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqTorso), Memory.ReadUShort(Addresses.Player.EqTorso + 4));
                case Enums.EquipmentSlots.Container:
                    return new Item();
                case Enums.EquipmentSlots.Feet:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqFeet), Memory.ReadUShort(Addresses.Player.EqFeet + 4));
                case Enums.EquipmentSlots.Head:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqHead), Memory.ReadUShort(Addresses.Player.EqHead + 4));
                case Enums.EquipmentSlots.LeftHand:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqLeftHand), Memory.ReadUShort(Addresses.Player.EqLeftHand + 4));
                case Enums.EquipmentSlots.Neck:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqNeck), Memory.ReadUShort(Addresses.Player.EqNeck + 4));
                case Enums.EquipmentSlots.RightHand:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqRightHand), Memory.ReadUShort(Addresses.Player.EqRightHand + 4));
                case Enums.EquipmentSlots.Ring:
                    return new Item(Memory.ReadUShort(Addresses.Player.EqRing), Memory.ReadUShort(Addresses.Player.EqRing + 4));
                default:
                    return new Item(0, 0);
            }
        }*/
    }
}
