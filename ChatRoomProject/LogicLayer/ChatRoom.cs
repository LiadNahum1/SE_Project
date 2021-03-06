﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoomProject.DataAccess;

using System.Timers;
namespace ChatRoomProject.LogicLayer
{
  /*This is the main class of the Logic Layer*/
    public class ChatRoom : IChatRoom
    {
        //fields
        private MessageHandler message_handler;
        private UserHandler user_handler;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ChatRoom.cs");
        private List<IUser> users;
        private List<IMessage> messages;
        private IUser currentUser;

        //useful error messages
        const string INVALID_NICKNAME = "Invalid nickname. \nYou insert a nickname that is already used in your group";
        const string EMPTY_INPUT = "Please insert data";
        const string INVALID_LOGIN = "Must register first";
        const string INVALID_GROUPID = "Group Id must be an integer";
        const string WRONG_PASSWORD = "Wrong password";
        const string ILLEGAL_LENGTH_MESSAGE = "Illegal length message. Must be under 100 characters";

        //constructor
        public ChatRoom()
        {
            this.message_handler = new MessageHandler();
            this.user_handler = new UserHandler();
            this.users = new List<IUser>(); //users list
            this.messages = new List<IMessage>(); //messages list
            this.currentUser = null; //user that is connected now
        }

        
        public void Start()
        {
            log.Info("The system starts now");
            try
            {
                messages = message_handler.RetrieveMessages(true);
            }
            catch(Exception e)
            {
                log.Error(e.ToString());
            }
        }
        //This is done every two seconds by reading from the timer.
        //This returns an updated list of messages that are organized according to
        //the data that the operation receives: sort type, sort order and filter 
        //if the user is interested.
        public List<IMessage> MessageManager(bool ascending, string filter, string sort, string groupId, string nickName, bool ispressed)
        {
            try
            {
                List<IMessage> updateList = new List<IMessage>();
                if (ispressed)
                {
                    this.messages.Clear();
                    if (filter != null) // if the user chose to filter the messages 
                    {
                        if (filter.Equals("filterByUser"))
                        {
                            message_handler.AddNicknameFilter(nickName);
                            message_handler.AddGroupFilter(Int32.Parse(groupId));
                        }
                        else // FilterByGroupID
                            message_handler.AddGroupFilter(Int32.Parse(groupId));
                        updateList = message_handler.RetrieveMessages(ispressed); // filter list
                        this.messages = updateList;
                    }

                    else // no filter so update the list of messages
                    {
                        message_handler.ClearFilters(); // delete all the filters
                        this.messages = (message_handler.RetrieveMessages(ispressed)); // update the new messages from the data base
                        updateList = this.messages;
                    }
                }
                else // not the first time of the same sort/filter
                {
                    updateList = this.messages;
                    if (filter != null) // if the user chose to filter the messages 
                    {
                        if (filter.Equals("filterByUser"))
                        {
                            message_handler.AddNicknameFilter(nickName);
                            message_handler.AddGroupFilter(Int32.Parse(groupId));
                        }
                        else // FilterByGroupID
                        {
                            message_handler.AddGroupFilter(Int32.Parse(groupId));
                        }

                        updateList.AddRange(message_handler.RetrieveMessages(ispressed)); // filter list
                        this.messages = updateList;
                    }

                    else // no filter so update the list of messages
                    {
                        updateList.AddRange(message_handler.RetrieveMessages(ispressed));// concat the new messages from the data base
                        this.messages = updateList;
                    }
                }

                if (sort.Equals("SortByNickName"))
                    updateList = SortByNickname(updateList, ascending);
                if (sort.Equals("SortByIdNicknameTimestamp"))
                    updateList = SortByIdNicknameTimestamp(updateList, ascending);
                if (sort.Equals("SortByTimestamp"))
                    updateList = SortTimestamp(updateList, ascending);

                updateList = LegalSizeOfMessagesList(updateList); // if there are more than 200 messag
                return updateList;
            }
            catch(Exception e)
            {
                log.Error(e.ToString());
                return null; 
            }
        }

   
        /*The method registrates a new user . The method first checks if nickname and groupId input are legal.
         * If there are, the method creats new User instance and adds him to data base 
         * If nickname is already been used by the same groupId, the function throws an exception
         */
        public void Registration(string groupId, string nickname, string password)
        {
            if (CheckIfInputIsEmpty(groupId) || CheckIfInputIsEmpty(nickname) || CheckIfInputIsEmpty(password))
            {
                throw new Exception(EMPTY_INPUT);
            }
            if (!IsValidGroupID(groupId))
            {
                throw new Exception(INVALID_GROUPID);
            }
            if (!IsValidNickname(groupId, nickname))
            {
                throw new Exception(INVALID_NICKNAME);
            }
            else
            {
                IUser user = new User(int.Parse(groupId), nickname, password);
                this.users.Add(user);
                try
                {
                    user_handler.InsertNewUser(user);
                }
                catch(Exception e)
                {
                    log.Error(e.ToString());
                }
            }
        }
        /*returns a hash string of password*/
        public string HashedPassword(string password)
        {
            return hashing.GetHashString(password);
        }
        public List<IUser> getUsers()
        {
            return this.users;
        }
        public IUser getCurrentUser()
        {
            return this.currentUser;
        }
        /*Check if nickname is already been used in the same group. If it is, returns false because the nickname is invalid. 
         * Nickname can only be used by one member of the same group
         */
        private bool IsValidNickname(string groupId, string nickname)
        {
            try
            {
                return this.user_handler.IsValidNickname(groupId, nickname);
            }
            catch(Exception e)
            {
                log.Error(e.ToString());
                return false;
            }
        }

        /*Check if password is legal. Need to be consisted only from letters and numbers and
         *  4 <= length <= 16"*/
        public bool IsValidPassword(string password)
        {
            if (password.Length < 4 | password.Length > 16)
                return false;
            password = password.ToLower();
            for (int i = 0; i < password.Length; i = i + 1)
            {
                if (!((password[i] >= '0' & password[i] <= '9') | (password[i] >= 'a' & password[i] <= 'z')))
                    return false;
            }
            return true;
        }


        /*Check if groupID is an integer*/
        private bool IsValidGroupID(string groupId)
        {
            try
            {
                int group = int.Parse(groupId);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        //return true if input is empty 
        private bool CheckIfInputIsEmpty(string str)
        {
            if (str == null | str == "")
            {
                return true;
            }
            return false;
        }
        public bool CanEdit(IMessage lastMessage)
        {
            if (lastMessage != null)
                return (this.currentUser.GroupID().Equals(lastMessage.GroupID) & this.currentUser.Nickname().Equals(lastMessage.UserName));
            else
            {
                return false;
            }
        }
        public void EditMessage(string newMessage, IMessage lastMessage)
        {
            if (!Message.CheckValidity(newMessage))
            {
                if (newMessage == "")//empty message
                {
                    log.Error("The user wrote an empty message");
                    throw new Exception(EMPTY_INPUT);
                }
                else
                {
                    log.Error("The user wrote an illegal message");
                    throw new Exception(ILLEGAL_LENGTH_MESSAGE);
                }
            }
            else
            {

                messages.Remove(lastMessage);
                try
                {
                    message_handler.EditByGuid(newMessage, lastMessage.Id.ToString());
                }
                catch(Exception e)
                {
                    log.Error(e.ToString());
                }
            }

        }
        //Check if the user is registered. If he is, returns true. Otherwise, returns false.
        public bool Login(string groupId, string nickname, string password)
        {
            try
            {
                if (CheckIfInputIsEmpty(groupId) || CheckIfInputIsEmpty(nickname) || CheckIfInputIsEmpty(password))
                {
                    throw new Exception(EMPTY_INPUT);
                }
                else if (this.user_handler.IsValidNickname(groupId, nickname))
                {
                    throw new Exception(INVALID_LOGIN); //user wasnt found
                }
                else if (!this.user_handler.IsValidPassword(groupId, nickname, password))
                {
                    throw new Exception(WRONG_PASSWORD);
                }
                else
                {
                    this.currentUser = this.user_handler.RetrieveUser(int.Parse(groupId), nickname, password);
                    return true;
                }
            }
            catch(Exception e)
            {
                log.Error(e.ToString());
                return false; 
            }
        }

        //Logout the current user
        public void Logout()
        {
            log.Info("User: " + this.currentUser + " logout");
            this.currentUser = null;
        }

        /*Keep the size of the message list no more than 200*/
        public List<IMessage> LegalSizeOfMessagesList(List<IMessage> list)
        {
            if (list.Count > 200)
            {
                int gap = list.Count - 200;
                for (int i = 0; i < gap; i = i + 1)
                {
                    list.RemoveAt(0);
                }
            }
            return list;
        }

        //Send messages. If empty or more than 100 characters throw an exception.
        //Otherwise, send it and save into data base
        public void Send(string messageContent)
        {
            if (!Message.CheckValidity(messageContent))
            {
                if (messageContent == "")//empty message
                {
                    log.Error("The user wrote an empty message");
                    throw new Exception(EMPTY_INPUT);
                }
                else
                {
                    log.Error("The user wrote an illegal message");
                    throw new Exception(ILLEGAL_LENGTH_MESSAGE);
                }
            }
            else
            {
                try
                {
                    message_handler.InsertNewMessage(new Message(this.currentUser.Nickname(), this.currentUser.GroupID(), messageContent));
                    log.Info("message was sent");
                }
                catch(Exception e)
                {
                    log.Error(e.ToString());
                }

            }
        }

        //input: list of Imessages and true- if ascending, false-descending
        //output:list of Imessages sorted by timestamp according the ascending boolean variable
        public static List<IMessage> SortTimestamp(List<IMessage> updatelist, Boolean ascending)
        {
            if (ascending)
            {
                return updatelist.OrderBy(o => o.Date).ToList();
            }
            else//descending
            {
                return updatelist.OrderByDescending(o => o.Date).ToList();
            }
        }
        //input: list of Imessages and true- if ascending, false-descending
        //output:list of Imessages sorted by NickName according the ascending boolean variable
        public static List<IMessage> SortByNickname(List<IMessage> updatelist, Boolean ascending)
        {
            if (ascending)
            {
                updatelist = updatelist.OrderBy(o => o.UserName).ToList();
                return updatelist;
            }
            else //descending
            {
                updatelist = updatelist.OrderByDescending(o => o.UserName).ToList();
                return updatelist;
            }
        }
        //input: list of Imessages and true- if ascending, false-descending
        //output:list of Imessages sorted by id->nickname->TimeStamp according the ascending boolean variable
        public static List<IMessage> SortByIdNicknameTimestamp(List<IMessage> updatelist, Boolean ascending)
        {
            if (ascending)
            {
                return updatelist.OrderBy(x => x.GroupID).ThenBy(x => x.UserName).ThenBy(x => x.Date).ToList();
            }
            else
            { // descending
                return updatelist.OrderByDescending(x => x.GroupID).ThenByDescending(x => x.UserName).ThenByDescending(x => x.Date).ToList();
            }
        }

    }
}