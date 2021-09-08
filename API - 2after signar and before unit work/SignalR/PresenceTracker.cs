using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        // key - usernames => value - list of connections  
        private static readonly Dictionary<string, List<string>> OnLineUsers = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (OnLineUsers)
            {
                if (OnLineUsers.ContainsKey(username))
                {
                    //an yparxei to key me to username perna to neo id
                    OnLineUsers[username].Add(connectionId);
                }
                else
                {
                    //an den uparxei dhmiourgei neo key me value to connectionId kai dhlwnei oti einai online
                    OnLineUsers.Add(username, new List<string>{connectionId});
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDissconnected(string username, string connectionId)
        {
            bool isOffLine = false;
            lock (OnLineUsers)
            {
                //an den yparxei to key-username de xreiazetia na to diagrapsei opote kleinei to Task
                if (!OnLineUsers.ContainsKey(username)) return Task.FromResult(isOffLine);

                OnLineUsers[username].Remove(connectionId);
                if (OnLineUsers[username].Count == 0)
                {
                    OnLineUsers.Remove(username);
                    isOffLine = true;
                }
            }

            return Task.FromResult(isOffLine);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onLineUsers;
            lock (OnLineUsers)
            {
                onLineUsers = OnLineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onLineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnLineUsers)
            {
                connectionIds = OnLineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);
        }
    }
}