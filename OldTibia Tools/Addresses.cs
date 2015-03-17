using System;
using System.Collections.Generic;
using System.Text;

namespace TibianicTools
{
    internal static class Addresses
    {
        internal static class Enums
        {
            internal enum IncomingPacketTypes : byte
            {
                // GameServer
                SelfAppear = 0x0A,
                GMAction = 0x0B,
                ErrorMessage = 0x14,
                FyiMessage = 0x15,
                WaitingList = 0x16,
                Ping = 0x1E,
                Death = 0x28,
                CanReportBugs = 0x32,
                MapDescription = 0x64,
                MoveNorth = 0x65,
                MoveEast = 0x66,
                MoveSouth = 0x67,
                MoveWest = 0x68,
                TileUpdate = 0x69,
                TileAddThing = 0x6A,
                TileTransformThing = 0x6B,
                TileRemoveThing = 0x6C,
                CreatureMove = 0x6D,
                ContainerOpen = 0x6E,
                ContainerClose = 0x6F,
                ContainerAddItem = 0x70,
                ContainerUpdateItem = 0x71,
                ContainerRemoveItem = 0x72,
                InventorySetSlot = 0x78,
                InventoryResetSlot = 0x79,
                ShopWindowOpen = 0x7A,
                ShopSaleGoldCount = 0x7B,
                ShopWindowClose = 0x7C,
                SafeTradeRequestAck = 0x7D,
                SafeTradeRequestNoAck = 0x7E,
                SafeTradeClose = 0x7F,
                WorldLight = 0x82,
                MagicEffect = 0x83,
                AnimatedText = 0x84,
                Projectile = 0x85,
                CreatureSquare = 0x86,
                CreatureHealth = 0x8C,
                CreatureLight = 0x8D,
                CreatureOutfit = 0x8E,
                CreatureSpeed = 0x8F,
                CreatureSkull = 0x90,
                CreatureShield = 0x91,
                ItemTextWindow = 0x96,
                HouseTextWindow = 0x97,
                PlayerStatus = 0xA0,
                PlayerSkillsUpdate = 0xA1,
                PlayerFlags = 0xA2,
                CancelTarget = 0xA3,
                CreatureSpeech = 0xAA,
                ChannelList = 0xAB,
                ChannelOpen = 0xAC,
                ChannelOpenPrivate = 0xAD,
                RuleViolationOpen = 0xAE,
                RuleViolationRemove = 0xAF,
                RuleViolationCancel = 0xB0,
                RuleViolationLock = 0xB1,
                PrivateChannelCreate = 0xB2,
                ChannelClosePrivate = 0xB3,
                TextMessage = 0xB4,
                PlayerWalkCancel = 0xB5,
                FloorChangeUp = 0xBE,
                FloorChangeDown = 0xBF,
                OutfitWindow = 0xC8,
                VipState = 0xD2,
                VipLogin = 0xD3,
                VipLogout = 0xD4,
                QuestList = 0xF0,
                QuestPartList = 0xF1,
                ShowTutorial = 0xDC,
                AddMapMarker = 0xDD,
            }
        }

        internal static class Player
        {
            internal static int Status = 0x00499FA8;
            internal static int PlayerID = 0x0049A018;
            internal static int Exp = 0x0049A00C;
            internal static int Level = Exp - 4;
            internal static int LevelPercent = Level - 8;
            internal static int PosX = 0x004ABF48;
            internal static int PosY = PosX - 4;
            internal static int PosZ = PosY - 4;
            internal static int Cap = 0x00499FF0;
            internal static int HP = PlayerID - 4;
            internal static int MaxHP = Exp + 4;
            internal static int Mana = Exp - 20;
            internal static int MaxMana = Exp - 24;
            internal static int MagicLevel = 0x0049A004;
            internal static int Fist = 0x00499FC8;
            internal static int Club = 0x00499FCC;
            internal static int Sword = 0x00499FD0;
            internal static int Axe = 0x00499FD4;
            internal static int Distance = 0x00499FD8;
            internal static int Shielding = 0x00499FDC;
            internal static int Fishing = 0x00499FE0;
            internal static int FishingPercent = 0x00499FC4;
            internal static int ShieldingPercent = 0x00499FC0;
            internal static int DistancePercent = 0x00499FBC;
            internal static int AxePercent = 0x00499FB8;
            internal static int SwordPercent = 0x00499FB4;
            internal static int ClubPercent = 0x00499FB0;
            internal static int FistPercent = 0x00499FAC;
            internal static int MagicLevelPercent = 0x00499FFC;
            internal static int Soul = 0;

            internal static int EqRightHand = 0x004A2340;
            internal static int EqLeftHand = 0x004A234C;
            internal static int EqNeck = 0x004A231C;
            internal static int EqHead = 0x004A2310;
            internal static int EqLegs = 0x004A2358;
            internal static int EqFeet = 0x004A2364;
            internal static int EqRing = 0x004A2370;
            internal static int EqAmmo = 0x004A237C;
        }

        internal static class Client
        {
            internal static int DialogPointer = 0x004ABF04;
            internal static int DialogDistanceTitle = 0x50;
            internal static int DialogTitle = 0x00D3B9D8;
            internal static int DialogOpen = 0,
                                FPSPointer = 0,
                                FPSCurrentLimitOffset = 4,
                                FPSCurrentFPSOffset = 12,
                                GameWindowPointer = 0,
                                GameWindowOffset1 = 0x2C,
                                GameWindowOffset2 = 0x30;
            internal static int LoginServerStart = 0x005E8908;
            internal static int LoginServerDistancePort = 100;
            internal static int LoginServerStep = 112;
            internal static int Connection = 0x005EFA24;
            internal static int ShowFPS = 0x00498E50;
            internal static int StatusBarText = 0x005EFD98;
            internal static int StatusBarTime = 0x005EFD94; //byte, seconds * 10
            internal static int XTEAKey = 0x00719D78,//7.72
                                RSAKey = 0x0055B620;
        }

        internal static class Container
        {
            internal static int Begin = 0,
                                End = 0,
                                Step = 0,
                                MaxContainers = 16,
                                ItemStep = 12;
            internal class Distances
            {
                internal static int isOpen = 0,
                                    ID = 4,
                                    Name = 16,
                                    Slots = 48,
                                    AmountOfItems = 56,
                                    ItemID = 60,
                                    ItemCount = 64;
            }
        }

        internal static class Battlelist
        {
            internal static int Begin = 0;
            internal static int End = 0; //Begin + Step * 250 = End
            internal static int Step = 156; //hex=9B, is not really an address, but a distance between battlelist entrants

            internal class Distances
            {
                internal static int ID = 0, // is actually -4, use minus when calculating
                                    Name = 4,
                                    PosX = 36,
                                    PosY = 40,
                                    PosZ = 44,
                                    Walking = 76,
                                    Direction = 80, // 0=up, 1=right, 2=down, 3=left
                                    OutfitType = 96,
                                    OutfitHead = 100,
                                    OutfitBody = 104,
                                    OutfitLegs = 108,
                                    OutfitFeet = 112,
                                    Light = 116,
                                    LightColor = 120,
                                    BlackSquare = 128,
                                    HPPercent = 132,
                                    WalkSpeed = 136,
                                    IsVisible = 140,
                                    Skull = 144, // 0-4 (none, yellow, green, white, red)
                                    Party = 148;
            }
        }

        internal static class Charlist
        {
            internal static int Pointer = 0x005EF9F4; /*30 bytes - Character name, null terminated string
                                                            30 bytes - Server name, null terminated string
                                                            4 bytes - Binary IP, the IP address in hex
                                                            16 bytes - IP string, null terminated string of the IP address
                                                            2 bytes - Port number
                                                            2 bytes - Padding*/
            internal static int Step = 0x54;
            internal static int DistanceCharacter = 0;
            internal static int DistanceServerName = 30; //string
            internal static int DistanceServerIP = 64; //string
            internal static int DistanceServerPort = 80;
            internal static int NumberOfCharacters = 0x005EF9F8; // Pointer + 4;

            /// <summary>
            /// Only changes when attempting to connect
            /// </summary>
            internal static int SelectedIndex = 0; //Pointer - 4;
        }

        /// <summary>
        /// No use for these addresses yet
        /// </summary>
        internal class VIP
        {
            internal static int Begin = 0x00498E70,
                                Step = 0x2C, //200 maximum entries
                                End = Begin + Step * 200;
            internal class Distances
            {
                internal static int ID = 0,
                                    Name = 4,
                                    Status = 0x22, // 0 = offline, 1 = online
                                    Icon = 0x28;
            }
        }

        internal static bool SetAddresses(string TibiaVersion)
        {
            switch (TibiaVersion)
            {
                case "7.4":
                    TibianicTools.Client.TibiaVersion = 740;
                    #region 7.4 addresses
                    Player.Status = 0x00499FA8;
                    Player.FistPercent = Player.Status + 4;
                    Player.ClubPercent = Player.Status + 8;
                    Player.SwordPercent = Player.Status + 12;
                    Player.AxePercent = Player.Status + 16;
                    Player.DistancePercent = Player.Status + 20;
                    Player.ShieldingPercent = Player.Status + 24;
                    Player.FishingPercent = Player.Status + 28;
                    Player.Fist = Player.Status + 32;
                    Player.Club = Player.Status + 36;
                    Player.Sword = Player.Status + 40;
                    Player.Axe = Player.Status + 44;
                    Player.Distance = Player.Status + 48;
                    Player.Shielding = Player.Status + 52;
                    Player.Fishing = Player.Status + 56;
                    Player.Cap = Player.Status + 72;
                    Player.MaxMana = Player.Status + 76;
                    Player.Mana = Player.Status + 80;
                    Player.MagicLevelPercent = Player.Status + 84;
                    Player.LevelPercent = Player.Status + 88;
                    Player.MagicLevel = Player.Status + 92;
                    Player.Level = Player.Status + 96;
                    Player.Exp = Player.Status + 100;
                    Player.MaxHP = Player.Status + 104;
                    Player.HP = Player.Status + 108;
                    Player.PlayerID = Player.Status + 112;
                    Player.PosZ = 0x004A4C88;
                    Player.PosY = Player.PosZ + 4;
                    Player.PosX = Player.PosZ + 8;

                    Container.Begin = 0x004A2388;
                    Container.Step = 492;
                    Container.End = Container.Begin + Container.Step * Container.MaxContainers;
                    Player.EqAmmo = Container.Begin - 12;
                    Player.EqRing = Container.Begin - 24;
                    Player.EqFeet = Container.Begin - 36;
                    Player.EqLegs = Container.Begin - 48;
                    Player.EqLeftHand = Container.Begin - 60;
                    Player.EqRightHand = Container.Begin - 72;
                    Player.EqNeck = Container.Begin - 100;
                    Player.EqHead = Container.Begin - 112;

                    Battlelist.Begin = 0x0049A07C - 4;
                    Battlelist.Step = 156;
                    Battlelist.End = Battlelist.Begin + Battlelist.Step * 250;

                    Charlist.NumberOfCharacters = 0x005EF9F8;
                    Charlist.Pointer = 0x005EF9F4;
                    Charlist.SelectedIndex = 0x005EF9F0;

                    VIP.Begin = 0x00498E70;
                    VIP.Step = 0x2C;

                    Client.FPSPointer = 0x004571F8;
                    Client.FPSCurrentLimitOffset = 20;
                    Client.FPSCurrentFPSOffset = 12;
                    Client.DialogPointer = 0x004A4C4C;
                    Client.DialogTitle = 0x50;
                    Client.DialogOpen = 0x005EFA84;
                    Client.LoginServerStart = 0x005E8898;
                    Client.Connection = 0x005EFA24;
                    Client.ShowFPS = 0x00498E50;
                    Client.StatusBarText = 0x005EFD98;
                    Client.StatusBarTime = Client.StatusBarText - 4;
                    Client.GameWindowPointer = 0x004A4C50;
                    Client.GameWindowOffset1 = 0x2C;
                    Client.GameWindowOffset2 = 0x30;
                    #endregion
                    return true;
                case "7.6":
                    TibianicTools.Client.TibiaVersion = 760;
                    #region 7.6 addresses
                    Player.Status = 0x004A11D0;
                    Player.FistPercent = Player.Status + 4;
                    Player.ClubPercent = Player.Status + 8;
                    Player.SwordPercent = Player.Status + 12;
                    Player.AxePercent = Player.Status + 16;
                    Player.DistancePercent = Player.Status + 20;
                    Player.ShieldingPercent = Player.Status + 24;
                    Player.FishingPercent = Player.Status + 28;
                    Player.Fist = Player.Status + 32;
                    Player.Club = Player.Status + 36;
                    Player.Sword = Player.Status + 40;
                    Player.Axe = Player.Status + 44;
                    Player.Distance = Player.Status + 48;
                    Player.Shielding = Player.Status + 52;
                    Player.Fishing = Player.Status + 56;
                    Player.Cap = Player.Status + 72;
                    Player.Soul = Player.Status + 76;
                    Player.MaxMana = Player.Status + 80;
                    Player.Mana = Player.Status + 84;
                    Player.MagicLevelPercent = Player.Status + 88;
                    Player.LevelPercent = Player.Status + 92;
                    Player.MagicLevel = Player.Status + 96;
                    Player.Level = Player.Status + 100;
                    Player.Exp = Player.Status + 104;
                    Player.MaxHP = Player.Status + 108;
                    Player.HP = Player.Status + 112;
                    Player.PlayerID = Player.Status + 116;
                    Player.PosX = 0x004ABF48;
                    Player.PosY = Player.PosX - 4;
                    Player.PosZ = Player.PosX - 8;

                    Container.Begin = 0x004A9630;
                    Player.EqAmmo = Container.Begin - 12;
                    Player.EqRing = Container.Begin - 24;
                    Player.EqFeet = Container.Begin - 36;
                    Player.EqLegs = Container.Begin - 48;
                    Player.EqLeftHand = Container.Begin - 60;
                    Player.EqRightHand = Container.Begin - 72;
                    Player.EqNeck = Container.Begin - 100;
                    Player.EqHead = Container.Begin - 112;

                    Battlelist.Begin = 0x004A12AC - 4;
                    Battlelist.Step = 156;
                    Battlelist.End = Battlelist.Begin + Battlelist.Step * 250;

                    Charlist.NumberOfCharacters = 0x005F6CB8;
                    Charlist.Pointer = 0x005F6CB4;
                    Charlist.SelectedIndex = Charlist.Pointer - 4;

                    Client.FPSPointer = 0x00459828;
                    Client.FPSCurrentLimitOffset = 20;
                    Client.FPSCurrentFPSOffset = 12;
                    Client.DialogPointer = 0x004ABF04;
                    Client.DialogDistanceTitle = 0x50;
                    Client.DialogOpen = 0x005F6D44; // closed=0, open=10
                    Client.LoginServerStart = 0x005EFB50;
                    Client.LoginServerStep = 112;
                    Client.Connection = 0x005F6CE4;
                    Client.StatusBarText = 0x005F7058;
                    Client.StatusBarTime = Client.StatusBarText - 4;
                    #endregion
                    return true;
                case "7.72":
                    TibianicTools.Client.TibiaVersion = 772;
                    #region 7.72 addresses
                    Player.Status = 0x005C67D8;
                    Player.FistPercent = Player.Status + 4;
                    Player.ClubPercent = Player.Status + 8;
                    Player.SwordPercent = Player.Status + 12;
                    Player.AxePercent = Player.Status + 16;
                    Player.DistancePercent = Player.Status + 20;
                    Player.ShieldingPercent = Player.Status + 24;
                    Player.FishingPercent = Player.Status + 28;
                    Player.Fist = Player.Status + 32;
                    Player.Club = Player.Status + 36;
                    Player.Sword = Player.Status + 40;
                    Player.Axe = Player.Status + 44;
                    Player.Distance = Player.Status + 48;
                    Player.Shielding = Player.Status + 52;
                    Player.Fishing = Player.Status + 56;
                    Player.Cap = Player.Status + 72;
                    Player.Soul = Player.Status + 76;
                    Player.MaxMana = Player.Status + 80;
                    Player.Mana = Player.Status + 84;
                    Player.MagicLevelPercent = Player.Status + 88;
                    Player.LevelPercent = Player.Status + 92;
                    Player.MagicLevel = Player.Status + 96;
                    Player.Level = Player.Status + 100;
                    Player.Exp = Player.Status + 104;
                    Player.MaxHP = Player.Status + 108;
                    Player.HP = Player.Status + 112;
                    Player.PlayerID = Player.Status + 116;
                    Player.PosZ = 0x005D16E8;
                    Player.PosY = Player.PosZ + 4;
                    Player.PosX = Player.PosZ + 8;

                    Battlelist.Begin = 0x005C68B0;
                    Battlelist.Step = 156;
                    Battlelist.End = Battlelist.Begin + Battlelist.Step * 250;

                    Charlist.Pointer = 0x0071C54C;
                    Charlist.NumberOfCharacters = 0x0071C550;
                    Charlist.SelectedIndex = 0x0071C548;

                    Client.StatusBarText = 0x0071DBE0;
                    Client.StatusBarTime = Client.StatusBarText - 4;
                    Client.Connection = 0x0071C588;
                    Client.DialogPointer = 0x005D16AC;
                    Client.DialogDistanceTitle = 0x50;
                    Client.DialogOpen = 0x0071C5E8;
                    Client.LoginServerStart = 0x007152F8;
                    Client.LoginServerStep = 112;
                    Client.XTEAKey = 0x00719D78;
                    Client.RSAKey = 0x0055B620;
                    Client.GameWindowPointer = 0x005D16B0;
                    Client.GameWindowOffset1 = 0x2C;
                    Client.GameWindowOffset2 = 0x30;
                    #endregion
                    return true;
                default:
                    return false;
            }
        }
    }
}
