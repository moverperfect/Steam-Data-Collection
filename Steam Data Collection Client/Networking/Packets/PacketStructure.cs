using System;

namespace Steam_Data_Collection_Client.Networking.Packets
{
    /// <summary>
    /// Standard Packet structure - data in bytes 0-7
    /// 0-3 - Packet Length
    /// 4-5 - Packet Type
    /// 6-7 - Machine ID
    /// 
    /// This is the standard packet structure 
    /// </summary>
    public abstract class PacketStructure
    {
        /// <summary>
        /// Contains the byte array that represents the packet data
        /// </summary>
        protected byte[] Buffer;

        /// <summary>
        /// Creates the packet of length such and machine id and user id of the source machine that created the packet
        /// </summary>
        /// <param name="length">The length of the byte array</param>
        /// <param name="machineId">The machine id of the machine that created the packet</param>
        protected PacketStructure(UInt32 length, ushort machineId)
        {
            Buffer = new byte[length];
            WriteUInt(length, 0);
            WriteUShort(machineId, 6);
        }

        /// <summary>
        /// Creates the packet out of the bare byte array
        /// </summary>
        /// <param name="packet">The base byte array that will be used as the data</param>
        protected PacketStructure(byte[] packet)
        {
            Buffer = packet;
        }

        /// <summary>
        /// Unused emtpy constructor
        /// </summary>
        protected PacketStructure()
        {
        }

        /// <summary>
        /// Returns the byte array that holds the data
        /// </summary>
        public byte[] Data
        {
            get { return Buffer; }
        }

        /// <summary>
        /// Returns the machine id property of the packet
        /// </summary>
        public ushort MachineId
        {
            get { return ReadUShort(6); }
        }

        /// <summary>
        /// Returns the type of the packet
        /// </summary>
        internal ushort PacketType
        {
            get { return ReadUShort(4); }
        }

        /// <summary>
        /// Writes a number to the byte array at an offset start point
        /// </summary>
        /// <param name="value">The number to be written</param>
        /// <param name="offset">The start point to write the number at</param>
        protected void WriteUShort(ushort value, int offset)
        {
            var tempBuffer = BitConverter.GetBytes(value);
            Array.Copy(tempBuffer, 0, Buffer, offset, 2);
        }

        /// <summary>
        /// Reads a 16-bit 'number' from a particular start point
        /// </summary>
        /// <param name="offset">The start point to read from</param>
        /// <returns>The number that is at that point</returns>
        private ushort ReadUShort(int offset)
        {
            return BitConverter.ToUInt16(Buffer, offset);
        }

        /// <summary>
        /// Writes a number to the byte array at an offest start point
        /// </summary>
        /// <param name="value">The number to be written</param>
        /// <param name="offset">The start point to write the number at</param>
        protected void WriteUInt(UInt32 value, int offset)
        {
            var tempBuffer = BitConverter.GetBytes(value);
            Array.Copy(tempBuffer, 0, Buffer, offset, 4);
        }
    }
}