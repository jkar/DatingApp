using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnLineUsers = new Dictionary<string, List<string>>();

        public Task UserConnected(string username, string connectionId)
        {
            lock (OnLineUsers)
            {
                if (OnLineUsers.ContainsKey(username))
                {
                    //an yparxei to key me to username perna to neo id
                    OnLineUsers[username].Add(connectionId);
                }
                else
                {
                    //an den uparxei dhmiourgei neo key me value to connectionId
                    OnLineUsers.Add(username, new List<string>{connectionId});
                }
            }

            return Task.CompletedTask;
        }

        public Task UserDissconnected(string username, string connectionId)
        {
            lock (OnLineUsers)
            {
                //an den yparxei to key-username de xreiazetia na to diagrapsei opote kleinei to Task
                if (!OnLineUsers.ContainsKey(username)) return Task.CompletedTask;

                OnLineUsers[username].Remove(connectionId);
                if (OnLineUsers[username].Count == 0)
                {
                    OnLineUsers.Remove(username);
                }
            }

            return Task.CompletedTask;
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
    }
}