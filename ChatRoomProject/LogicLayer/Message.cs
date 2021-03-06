﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoomProject.LogicLayer
{
     
    public class Message : IMessage
    {
        //fields
        private Guid id;
        private string nickname;
        private int groupId;
        private DateTime date;
        private string messageContent;
        const int MAX_LENGTH = 100;

        /* constructor 
          * gets strings: id, nickname, groupId, date and message content and a boolean value isRestored which indicates whether the current
          * message details are restored from dataBase or the message is a new one. 
          * If the message is restored there is no need to save it in persistent layer.
          * If it isn't, we will save its details in files. 
          */
    public Message(string nickname, int groupId, string messageContent)
        {
            this.id = Guid.NewGuid();
            this.nickname = nickname;
            this.groupId = groupId;
            this.date = DateTime.Now.ToUniversalTime();
            this.messageContent = messageContent;
        }

        public Message(Guid id,string nickname, int groupId, DateTime date, string messageContent)
        {
            this.id = id;
            this.nickname = nickname;
            this.groupId = groupId;
            this.date = date;
            this.messageContent = messageContent;
        }

        //implements IMessage


        public Guid Id { get { return this.id; } set { this.id = value; } }
        public string UserName { get { return this.nickname; } set { this.nickname = value; } }
        public DateTime Date { get { return this.date; } set { this.date = value; } }
        public string MessageContent { get { return this.messageContent; } set { this.messageContent = value; } }
        int IMessage.GroupID { get { return this.groupId; } set { this.groupId = value; }}

        //Save message's details in the system files 
        public void Save()
        {
            //MessageHandler.SaveToFile(this.id, this.nickname, this.groupId, this.date, this.messageContent);
        }

        /*Static function checks the validity of a message content. 
          If the string is empty or above 150 characters, returns false.
        */
        public static bool CheckValidity(string content)
        {
            if (content.Length == 0 | content.Length > MAX_LENGTH)
                return false;
            else
             return true;
        }

        public override string ToString()
        {
            return "Group:" + this.groupId + ",Nickname:" + this.nickname  +  " (" + TimeZoneInfo.ConvertTime(this.date, TimeZoneInfo.Utc, TimeZoneInfo.Local) + "): "+ this.messageContent;
        }
    }
}
