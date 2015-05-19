using System;

namespace Steam_Data_Collection_Client.Objects
{
    [Serializable]
    public abstract class Friend
    {
        #region Define Accessor Variables

        /// <summary>
        /// The steam id of the friend
        /// </summary>
        public UInt64 SteamId { get; set; }

        /// <summary>
        /// The timestamp of the time that the users became friends
        /// </summary>
        public DateTime TimeStamp { get; set; }

        #endregion

        /// <summary>
        /// The main constuctor to create a friend
        /// </summary>
        /// <param name="steamid">The steam id of the friend</param>
        /// <param name="timeStamp">The timestamp that they became friends</param>
        public Friend(UInt64 steamid, DateTime timeStamp)
        {
            SteamId = steamid;
            TimeStamp = timeStamp;
        }
    }
}