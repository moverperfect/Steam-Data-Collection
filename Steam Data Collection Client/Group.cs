using System;

namespace Steam_Data_Collection_Client
{
    internal class Group
    {
        #region Define Accessor Variables

        /// <summary>
        /// The id of the group
        /// </summary>
        public Int64 GroupId { get; set; }

        /// <summary>
        /// The name of the group
        /// </summary>
        public String GroupName { get; set; }

        /// <summary>
        /// The custom url of the group
        /// </summary>
        public String GroupUrl { get; set; }

        /// <summary>
        /// The summary information of the group
        /// </summary>
        public String Summary { get; set; }

        /// <summary>
        /// The number of members in the group
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// The last time this information was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        #endregion
    }
}