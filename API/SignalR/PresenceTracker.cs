using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        // key - usernames => value - list of connections (connections related with groups-(συνομιλίες))
        private static readonly Dictionary<string, List<string>> OnLineUsers = new Dictionary<string, List<string>>();

        //gyrnaei true or false , an o user einai connected kai pernaei sto dictionary to connectioniD tou user , k an den uparxei idi to key(user) to dhmiourgei
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (OnLineUsers)
            //tsekarei an sto dictionary yparxei idi to key me to username
            //an yparxxei kanei add to connectionId pou xrhsimopoieitai kai sto Connection table
            
            // ?????????????? den katalavainw giati den exei isOnline = true; kai sto if ?????????????????????? (lusi apo katw)
            //katalava oti An exei connectionId ,xeroun idi oti einai online ,opote den xreaizetai na enimerwthoun xana (exei kanei pithanon log in k apo alli suskeui i browser) 
            {
                if (OnLineUsers.ContainsKey(username))
                {
                    //an yparxei to key me to username perna to neo id
                    OnLineUsers[username].Add(connectionId);
                }
                //an den yparxei dhmiourgei k to key k to value kai dilwnei oti einai online
                else
                {
                    //an den uparxei dhmiourgei neo key me value to connectionId kai dhlwnei oti einai online
                    OnLineUsers.Add(username, new List<string>{connectionId});
                    isOnline = true;
                }
            }
            //gyrnaei true or false an einai online
            return Task.FromResult(isOnline);
        }

        //gyrnaei true or false , an o user einai dissconnected kai diagrafei to connectionId tou apo to Dictionary, an den exoun meinei alla connectionId gia ton xristi
        //diagrafei k to key - (username) apo to dictionary
        //???? kai s auti ti methodo me berdeuei to offline pou gurnaei (lusi apo katw)
        //katalava oti to kanei (enimerwnei oti einai offline tous allous me isoffline = true otan den exei kanena connectionId sto dictionary pou shmainei oti exei kanei 
        //log out apo oles tis susksues)
        public Task<bool> UserDissconnected(string username, string connectionId)
        {
            bool isOffLine = false;
            lock (OnLineUsers)
            {
                //an den yparxei to key-username de xreiazetia na to diagrapsei opote kleinei to Task
                if (!OnLineUsers.ContainsKey(username)) return Task.FromResult(isOffLine);

                //diagrafei connectionId apo to List tou username sto dictionary
                OnLineUsers[username].Remove(connectionId);
                //an den exoun meinei connectionId sto list tou username, diagrafei k to key (username)
                //kai thetei oti einai offLine gia na enimerwsei tous allous
                if (OnLineUsers[username].Count == 0)
                {
                    OnLineUsers.Remove(username);
                    isOffLine = true;
                }
            }

            return Task.FromResult(isOffLine);
        }

        //epistrfei ena array apo keys twn users pou ta vriskei apo to Dictionary
        public Task<string[]> GetOnlineUsers()
        {
            string[] onLineUsers;
            lock (OnLineUsers)
            {
                onLineUsers = OnLineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onLineUsers);
        }

        //epistrefei apo to Dictionary ta connectionId se List, pou sxetizontai me to username (parameter) to opoio einai key sto Dictionary
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