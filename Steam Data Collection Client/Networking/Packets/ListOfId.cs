using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Steam_Data_Collection_Client.Networking.Packets
{
    public class ListOfId : PacketStructure
    {
        /// <summary>
        /// The List data
        /// </summary>
        private List<UInt64> _list;

        /// <summary>
        /// Create a new List with a List
        /// </summary>
        /// <param name="tmpList">The list to be stored in the byte array</param>
        /// <param name="machineId">The machine id of the machine that created the packet</param>
        /// <param name="packetType">The type of packet that this is</param>
        public ListOfId(List<UInt64> tmpList, ushort machineId, ushort packetType)
        {
            List = tmpList;
            ListToByteArray(tmpList);
            WriteUShort(packetType, 4);
            WriteUShort(machineId, 6);
        }

        /// <summary>
        /// Create a new List with byte array already
        /// </summary>
        /// <param name="bytes">The byte array to be made with</param>
        public ListOfId(Byte[] bytes)
            : base(bytes)
        {
            var tempBytes = new Byte[bytes.Length - 8];
            Array.Copy(bytes, 8, tempBytes, 0, tempBytes.Length);
            ByteArrayToList(tempBytes);
        }

        /// <summary>
        /// Create a empty List packet
        /// </summary>
        public ListOfId()
        {
            _list = new List<UInt64>();
        }

        /// <summary>
        /// The List
        /// </summary>
        public List<UInt64> List
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
        private void ListToByteArray(List<UInt64> tmpList)
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
            WriteUInt((UInt32) Buffer.Length, 0);
            Array.Copy(binaryDataResult, 0, Buffer, 8, binaryDataResult.Length);
        }

        /// <summary>
        /// Turns the byte array into a List
        /// </summary>
        /// <param name="arrayBytes">The byte array to be turned into a list</param>
        private void ByteArrayToList(Byte[] arrayBytes)
        {
            Buffer = arrayBytes;
            List<UInt64> list;
            // Deserializing into list using a memory stream to binaryformmatter deserialiser to datatable
            using (var stream = new MemoryStream(arrayBytes))
            {
                var bformatter = new BinaryFormatter();
                list = (List<UInt64>) bformatter.Deserialize(stream);
            }
            // Adding list into list object    
            _list = list;
        }
    }
}