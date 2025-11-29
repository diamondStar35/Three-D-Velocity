using System;
using System.Collections.Generic;
using System.IO;

namespace TDVServer
{
    public class ChatManager
    {
        private Dictionary<String, ChatRoom> m_chatRooms;
        private Dictionary<String, Player> m_clientList;

        public ChatManager(Dictionary<String, Player> clientList)
        {
            m_chatRooms = new Dictionary<string, ChatRoom>();
            m_clientList = clientList;
            CreateDefaultRooms();
        }

        private void CreateDefaultRooms()
        {
            CreateChatRoom("Foo Bar");
			CreateChatRoom("For Your Kids");
			CreateChatRoom("Admins", null, "adminsrule");
        }

        public void CreateChatRoom(String friendlyName, String tag1, String tag2, String password)
		{
			String id = Server.getID(m_chatRooms);
			m_chatRooms.Add(id, new ChatRoom(id, friendlyName, tag1, tag2, password));
			if (tag2 != null)
				m_clientList[tag2].chatID = id;
			if (tag1 != null) {
				m_clientList[tag1].chatID = id;
				SendChatMessage(id, "Room created", MessageType.enterRoom, true);
			}
		}

		public void CreateChatRoom(String friendlyName)
		{
			CreateChatRoom(friendlyName, null, null, null);
		}

		public void CreateChatRoom(String tag1, String tag2)
		{
			CreateChatRoom(null, tag1, tag2, null);
		}

		public void CreateChatRoom(String friendlyName, String tag, String password)
		{
			CreateChatRoom(friendlyName, tag, null, password);
		}

        public bool JoinChatRoom(String tag, String id, String password)
		{
			ChatRoom room = null;
			if (!m_chatRooms.TryGetValue(id, out room))
				return false;
			if (room.password != null && !String.Equals(password, room.password))
				return false;
			Server.output(LoggingLevels.info, "Player " + m_clientList[tag].name + " joined chat room " + room.friendlyName);
			SendChatMessage(id, m_clientList[tag].name + " has joined the room!", MessageType.enterRoom, true);
			SendToRoom(id, CSCommon.buildCMDString(CSCommon.cmd_addMember, tag, m_clientList[tag].name));
			room.add(tag);
			m_clientList[tag].chatID = id;
			return true;
		}

		public void LeaveRoom(String tag, bool playerAlive)
		{
			ChatRoom room = null;
			Player p = Server.getPlayerByID(tag);
			if (p == null)
				return;
			if (p.chatID == null)
				return; //protect against potential multiple presses of "leave" button
			if (!m_chatRooms.TryGetValue(p.chatID, out room))
				return;
			String id = room.id;
			if (room.remove(tag)) //return true if this is the last member to be removed.
			{
				m_chatRooms.Remove(room.id);
				Server.output(LoggingLevels.info, "Chat room " + room.friendlyName + " was closed because the last member left.");
			}
			Server.output(LoggingLevels.info, "Player " + p.name + " left chat room " + room.friendlyName);
			if (playerAlive)
				CSCommon.sendData(p.client, CSCommon.buildCMDString(CSCommon.cmd_leaveChatRoom));
			p.chatID = null;
			SendChatMessage(id, p.name + " has left the room!", MessageType.leaveRoom, true);
			SendToRoom(id, CSCommon.buildCMDString(CSCommon.cmd_removeMember, tag));
		}

		public void SendToRoom(String id, MemoryStream data)
		{
			ChatRoom c;
			Player p;
			if (m_chatRooms.TryGetValue(id, out c)) {
				foreach (String s in c.getIds()) {
					if ((p = Server.getPlayerByID(s)) != null)
						CSCommon.sendData(p.client, data);
				}
			}
			data.Close();
		}

		public bool IsPassworded(String id)
		{
			ChatRoom room = null;
			if (!m_chatRooms.TryGetValue(id, out room))
				return false;
			return room.type == RoomTypes.password;
		}

        public void SendPrivateChatMessage(String senderTag, String recipientTag, String message)
        {
            Player p = Server.getPlayerByID(recipientTag);
            Player sender = Server.getPlayerByID(senderTag);
            if (p != null && sender != null)
            {
                string chatMsg = sender.name + " (private): " + message;
                CSCommon.sendData(p.client, CSCommon.buildCMDString(CSCommon.cmd_chat, (byte)MessageType.privateMessage, chatMsg));
            }
        }

		public void SendChatMessage(String sender, String message, MessageType type, bool fromServer)
		{
			ChatRoom room = null;
			String chatId = null;
			Player p = null;
			if (fromServer)
				chatId = sender;
			else {
				p = Server.getPlayerByID(sender);
                if (p == null) return; // Player might have disconnected
				chatId = p.chatID;
			}
			if (!fromServer)
				message = p.name + ": " + message;

			if (chatId == null)
            {
                // Propagate to lobby
                MemoryStream stream = CSCommon.buildCMDString(CSCommon.cmd_chat, (byte)type, message);
                foreach (Player lobbyPlayer in m_clientList.Values) {
                    if (lobbyPlayer.client != (p?.client) && lobbyPlayer.chatID == null)
                        CSCommon.sendData(lobbyPlayer.client, stream);
                }
            }
			else {
				if (!m_chatRooms.TryGetValue(chatId, out room)) {
					if (p != null)
						CSCommon.sendData(p.client, CSCommon.buildCMDString(CSCommon.cmd_leaveChatRoom));
					return;
				} //if the chat room no longer exists.
				foreach (String id in room.getIds()) {
					if (!fromServer && sender.Equals(id))
						continue; //so sender doesn't get their own message
                    Player recipient = Server.getPlayerByID(id);
					if (recipient != null)
						CSCommon.sendData(recipient.client, CSCommon.buildCMDString(CSCommon.cmd_chat, (byte)type, message));
				}
			}
			String writeMsg = "";
			if (p != null)
				writeMsg = p.name + Environment.NewLine + p.tag;
			if (!String.IsNullOrEmpty(writeMsg))
				writeMsg += Environment.NewLine;
			if (room == null)
				writeMsg += "Lobby";
			else
				writeMsg += room.friendlyName;
			writeMsg += Environment.NewLine + message;
			Server.outputChat(writeMsg);
		}

        public ICollection<ChatRoom> GetPublicRooms()
        {
            List<ChatRoom> publicRooms = new List<ChatRoom>();
            foreach (ChatRoom room in m_chatRooms.Values)
            {
                if (room.type != RoomTypes.closed)
                {
                    publicRooms.Add(room);
                }
            }
            return publicRooms;
        }

        public ICollection<ChatRoom> GetAllRooms()
        {
            return m_chatRooms.Values;
        }

        public ChatRoom GetRoom(string id)
        {
            m_chatRooms.TryGetValue(id, out ChatRoom room);
            return room;
        }
    }
}
