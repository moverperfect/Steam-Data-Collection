using System;

namespace Steam_Data_Collection_Client.Objects
{
    [Serializable]
    public class Friend
    {
        #region Define Accessor Variables

        /// <summary>
        /// The steam id of the friend
        /// </summary>
        public UInt64 SteamId { get; internal set; }

        /// <summary>
        /// The timestamp of the time that the users became friends
        /// </summary>
        public DateTime TimeStamp { get; internal set; }

        #endregion

        /// <summary>
        /// Empty friend constructor
        /// </summary>
        internal Friend()
        {
        }
    }
}