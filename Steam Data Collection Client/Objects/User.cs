using System;
using System.Collections.Generic;

namespace Steam_Data_Collection_Client.Objects
{
    internal class User
    {
        #region Define Accessor Variables

        /// <summary>
        /// The 64 bit id that identifies the user
        /// </summary>
        public UInt64 SteamId { get; set; }

        /// <summary>
        /// User Name of the user
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Visibility state of the user
        /// </summary>
        public bool VisibilityState { get; set; }

        /// <summary>
        /// The custom part of the custom url, if this is equal to / then they do not have a custom url
        /// </summary>
        public String CustomUrl { get; set; }

        /// <summary>
        /// The time that the user created their account
        /// </summary>
        public DateTime MemberSince { get; set; }

        /// <summary>
        /// The laoction of the user
        /// </summary>
        public String Location { get; set; }

        /// <summary>
        /// The real name of the user
        /// </summary>
        public String RealName { get; set; }

        /// <summary>
        /// Their primary clan ID
        /// </summary>
        public UInt64 PrimaryClanId { get; set; }

        /// <summary>
        /// The last time that the user logged off their account
        /// </summary>
        public DateTime LastLogOff { get; set; }


        /// <summary>
        /// The last time that we updated the summary of the user
        /// </summary>
        public DateTime LastSummaryUpdate { get; set; }

        /// <summary>
        /// The last time that we updated the friends of the user
        /// </summary>
        public DateTime LastFriendUpdate { get; set; }

        /// <summary>
        /// The last time that we updated the groups of the suer
        /// </summary>
        public DateTime LastGroupUpdate { get; set; }

        /// <summary>
        /// The last time that we updated the games of the user
        /// </summary>
        public DateTime LastGameUpdate { get; set; }

        /// <summary>
        /// A list of the friends of the user
        /// </summary>
        public List<Friend> ListOfFriends { get; set; }

        /// <summary>
        /// A list of the groups of the user
        /// </summary>
        public List<Group> ListOfGroups { get; set; }

        /// <summary>
        /// A list of the games that the user owns
        /// </summary>
        public List<GameHistory> ListOfGames { get; set; }

        #endregion

        /// <summary>
        /// Defines an empty user
        /// </summary>
        public User()
        {
            ListOfFriends = new List<Friend>();
            ListOfGroups = new List<Group>();
            ListOfGames = new List<GameHistory>();
        }
    }
}