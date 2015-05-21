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
        public int AppId { get; set; }

        /// <summary>
        /// The number of time in the last 2 weeks
        /// </summary>
        public int Last2Weeks { get; set; }

        /// <summary>
        /// Time spent in total
        /// </summary>
        public int OnRecord { get; set; }

        /// <summary>
        /// The name of the game
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}