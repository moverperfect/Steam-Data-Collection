using System;

namespace Steam_Data_Collection_Client.Objects
{
    [Serializable]
    public class GameHistory
    {
        #region Define Accessor Variables

        /// <summary>
        /// The ID number of the app
        /// </summary>
        public int AppId { get; internal set; }

        /// <summary>
        /// The number
        /// </summary>
        public int Last2Weeks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int OnRecord { get; set; }

        #endregion
    }
}