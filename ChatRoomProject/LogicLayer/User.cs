﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoomProject.DataAccess;

namespace ChatRoomProject.LogicLayer
{
    public class User : IUser
    {
        //fields
        private int groupID;
        private string nickname;
        private string password;


        /*constructor
         * gets groupId, nickname and boolean value isRestored which indicates whether the current user details are restored from dataBase or
         * the user is a new one. If the user is restored there is no need to save him in persistent layer because he is already saved.
         * If he isn't, we will save his details in Data Base. 
         */
        public User(int groupID, string nickname,string password)
        {
            this.groupID = groupID;
            this.nickname = nickname;
            this.password = password;
        }

        public string Password()
        {
            return this.password;
        }
        public int GroupID()
        {
            return this.groupID;
        }
        public string Nickname()
        {
            return this.nickname;
        }

        public override string ToString()
        {
            return GroupID() + " "+ Nickname();
        }

      
    }
}
