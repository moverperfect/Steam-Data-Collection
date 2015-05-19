using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Steam_Data_Collection_Client.Objects;

namespace Steam_Data_Collection_Client.Networking.Packets
{
    public class ListOfUsers : PacketStructure
    {
        /// <summary>
        /// The list data
        /// </summary>
        private List<User> _list;

        /// <summary>
        /// Create a new list with a list
        /// </summary>
        /// <param name="tmpList">The list to be stored</param>
        /// <param name="machineId">The machine id of the host machine</param>
        /// <param name="userId">The user id of the user, unused</param>
        /// <param name="packetType">The type of packet that this is</param>
        public ListOfUsers(List<User> tmpList, ushort machineId, ushort userId, ushort packetType)
        {
            List = tmpList;
            ListToByteArray(tmpList);
            WriteUShort(packetType, 2);
            WriteUShort(machineId, 4);
            WriteUShort(userId, 6);
        }

        /// <summary>
        /// Create a new list with a byte array already
        /// </summary>
        /// <param name="bytes">Thw byte array to be made into a list</param>
        public ListOfUsers(Byte[] bytes)
            : base(bytes)
        {
            var tempBytes = new Byte[bytes.Length - 8];
            Array.Copy(bytes, 8, tempBytes, 0, tempBytes.Length);
            ByteArrayToList(tempBytes);
        }

        /// <summary>
        /// Create a empty List packet
        /// </summary>
        public ListOfUsers()
        {
            List = new List<User>();
        }

        /// <summary>
        /// The List
        /// </summary>
        public List<User> List
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
        public void ListToByteArray(List<User> tmpList)
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
            WriteUShort((ushort) Buffer.Length, 0);
            Array.Copy(binaryDataResult, 0, Buffer, 8, binaryDataResult.Length);
        }

        /// <summary>
        /// Turns the byte array into a List
        /// </summary>
        /// <param name="arrayBytes">The byte array to be turned into a list</param>
        public void ByteArrayToList(Byte[] arrayBytes)
        {
            Buffer = arrayBytes;
            List<User> list;
            // Deserializing into list using a memory stream to binaryformmatter deserialiser to datatable
            using (var stream = new MemoryStream(arrayBytes))
            {
                var bformatter = new BinaryFormatter();
                list = (List<User>) bformatter.Deserialize(stream);
            }
            // Adding list into list object    
            _list = list;
        }
    }
}