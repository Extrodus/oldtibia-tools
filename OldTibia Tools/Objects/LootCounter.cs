using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace TibianicTools.Objects
{
    class LootCounter
    {
        internal LootCounter()
        {
            Stopwatch = new Stopwatch();
            ItemPrices = new Dictionary<ushort, ushort>();
        }

        #region get-set properties
        private Stopwatch Stopwatch { get; set; }
        /// <summary>
        /// Key = item id, value = NPC price.
        /// </summary>
        private Dictionary<ushort, ushort> ItemPrices { get; set; }
        /// <summary>
        /// Key = item id, value = # of items (item count * item stacks)
        /// </summary>
        private Dictionary<ushort, ushort> StartingItems { get; set; }

        internal TimeSpan Elapsed
        {
            get { return Stopwatch.Elapsed; }
        }
        #endregion

        #region methods
        /// <summary>
        /// Connects to a binary file on specified URI. Note that this is done synchronously.
        /// </summary>
        /// <param name="uri">A URI to connect to.</param>
        /// <returns>True if success, false if failure.</returns>
        internal bool ConnectToDatabase(Uri uri)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] data = webClient.DownloadData(uri);
                using (BinaryReader breader = new BinaryReader(new MemoryStream(data)))
                {
                    /* file structure:
                     * 2 bytes items count
                     * loop items
                     *    2 bytes item id
                     *    2 bytes npc worth
                     */
                    ushort count = breader.ReadUInt16();
                    ItemPrices = new Dictionary<ushort, ushort>();
                    for (ushort i = 0; i < count; i++)
                    {
                        ItemPrices.Add(breader.ReadUInt16(), breader.ReadUInt16());
                    }
                }
                return true;
            }
            catch { }
            return false;
        }

        #endregion
    }
}
