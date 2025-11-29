using System;
using System.Collections.Generic;

namespace TDVServer
{
    public class GameManager
    {
        private Dictionary<String, Game> m_gameList;
        private Dictionary<String, Player> m_clientList;
        private Action<bool> m_setModifiedClientList;

        public GameManager(Dictionary<String, Player> clientList, Action<bool> setModifiedClientList)
        {
            m_gameList = new Dictionary<string, Game>();
            m_clientList = clientList;
            m_setModifiedClientList = setModifiedClientList;
            CreateFFA();
        }

        private void CreateFFA()
        {
            CreateNewGame(null, Game.GameType.freeForAll);
        }

        public Game CreateNewGame(String tag, Game.GameType type)
        {
            if (tag != null)
                Server.output(LoggingLevels.info, "Player " + Server.getPlayerByID(tag).name + " is creating a new " + type.ToString() + " game.");
            else
                Server.output(LoggingLevels.info, "Creating FFA game.");

            String id = Server.getID(m_gameList);
            Game g = new Game(id, type);
            g.gameFinished += GameFinishedEvent;
            m_gameList.Add(id, g);
            if (tag != null) {
                g.add(m_clientList[tag]);
                m_clientList.Remove(tag);
                m_setModifiedClientList(true);
            }
            Server.output(LoggingLevels.debug, "ok");
            return g;
        }

        private void GameFinishedEvent(Game sender)
        {
            m_gameList.Remove(sender.id);
            Server.output(LoggingLevels.debug, "Game " + sender.id + " ended.");
            sender.gameFinished -= GameFinishedEvent;
        }

        public String JoinGame(String tag, String id)
        {
            Server.output(LoggingLevels.info, "Player " + Server.getPlayerByID(tag).name + " is attempting to join game " + id);
            if (!m_gameList.ContainsKey(id)) {
                Server.output(LoggingLevels.error, "ERROR: id " + id + " doesn't exist.");
                CSCommon.sendResponse(m_clientList[tag].client, false);
                return null;
            }
            String name = m_gameList[id].ToString();
            if (!m_gameList[id].isOpen(tag, m_clientList[tag].entryMode)) {
                CSCommon.sendResponse(m_clientList[tag].client, false);
                return null;
            }
            CSCommon.sendResponse(m_clientList[tag].client, true);
            m_gameList[id].add(m_clientList[tag]);
            m_clientList.Remove(tag);
            m_setModifiedClientList(true);
            Server.output(LoggingLevels.debug, "ok");
            return name;
        }

        public bool JoinFFA(String tag)
        {
            foreach (Game g in m_gameList.Values) {
                if (g.type == Game.GameType.freeForAll) {
                    g.add(m_clientList[tag]);
                    m_clientList.Remove(tag);
                    m_setModifiedClientList(true);
                    return true;
                }
            }
            return false;
        }

        public ICollection<Game> GetGames()
        {
            return m_gameList.Values;
        }
        
        public void ForceEndAllGames(bool isRebooting)
        {
            foreach (Game g in m_gameList.Values)
                g.setForceGameEnd((isRebooting) ? null : "there was a problem with the server.");
        }

        public int GameCount()
        {
            return m_gameList.Count;
        }

        public void QueueCriticalMessageInGames(string message)
        {
            foreach (Game g in m_gameList.Values)
                g.queueCriticalMessage(message);
        }
    }
}
