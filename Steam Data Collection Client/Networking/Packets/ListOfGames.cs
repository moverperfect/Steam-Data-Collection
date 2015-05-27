using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Steam_Data_Collection_Client.Objects;

namespace Steam_Data_Collection_Client.Networking.Packets
{
    public class ListOfGames : PacketStructure
    {
        /// <summary>
        /// The list data
        /// </summary>
        private List<GameHistory> _list;

        /// <summary>
        /// Creates a new list with a list
        /// </summary>
        /// <param name="tmpList">The list to be stored</param>
        /// <param name="machineId">The machine id of the creation machine</param>
        /// <param name="packetType">The type of packet this is</param>
        public ListOfGames(List<GameHistory> tmpList, ushort machineId, ushort packetType)
        {
            List = tmpList;
            ListToByteArray(tmpList);
            WriteUShort(packetType, 4);
            WriteUShort(machineId, 6);
        }

        /// <summary>
        /// Creates a new list with a byte array already
        /// </summary>
        /// <param name="bytes">The byte array to be created with</param>
        public ListOfGames(Byte[] bytes)
            : base(bytes)
        {
            var tempBytes = new byte[bytes.Length - 8];
            Array.Copy(bytes, 8, tempBytes, 0, tempBytes.Length);
            ByteArrayToList(tempBytes);
        }

        /// <summary>
        /// Create a empty list packet
        /// </summary>
        public ListOfGames()
        {
            _list = new List<GameHistory>();
        }

        /// <summary>
        /// The List
        /// </summary>
        public List<GameHistory> List
        {
            get { return _list; }
            private set
            {
                _list = value;
                ListToByteArray(value);
            }
        }

        /// <summary>
        /// Turns a List into a byte array
        /// </summary>
        /// <param name="tmpList"></param>
        private void ListToByteArray(List<GameHistory> tmpList)
        {
            byte[] binaryDataResult;
            // Serializing the List using a binaryformmatter to memorystream to byte array
            using (var memStream = new MemoryStream())
            {
                var brFormatter = new BinaryFormatter();
                brFormatter.Serialize(memStream, tmpList);
                binaryDataResult = memStream.ToArray();
            }
            // Add the length metadata to the byte array
            Buffer = new Byte[binaryDataResult.Length + 8];
            WriteUInt((UInt32)Buffer.Length, 0);
            Array.Copy(binaryDataResult, 0, Buffer, 8, binaryDataResult.Length);
        }

        /// <summary>
        /// Turns the byte array into a List
        /// </summary>
        /// <param name="arrayBytes">The byte array to be turned into a list</param>
        private void ByteArrayToList(Byte[] arrayBytes)
        {
            Buffer = arrayBytes;
            List<GameHistory> list;
            // Deserializing into list using a memory stream to binaryformmatter deserialiser to datatable
            using (var stream = new MemoryStream(arrayBytes))
            {
                var bformatter = new BinaryFormatter();
                list = (List<GameHistory>)bformatter.Deserialize(stream);
            }
            // Adding list into list object    
            _list = list;
        }
    }
}
