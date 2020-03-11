using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace dominionspermadeath.src
{
    class PermaDeath : ModSystem
    {
        void InitSaveData(ISaveGame save)
        {
            //Make sure there is no null data, else get calls will fail.
            //Namespace in case someone uses the same key
            if (!Utils.HasSaveData(save, "DominionsPermaDeath"))
            {
                Utils.SetSaveData(save, "DominionsPermaDeath", new List<string>());
            }
        }

        bool IsPermaDeath(ISaveGame save, string uid)
        {
            List<string> playerIds = Utils.GetSaveData<List<string>>(save, "DominionsPermaDeath");

            return playerIds.Exists(puid => (@puid == @uid));
        }

        void SetPermaDeath(ISaveGame save, string uid, bool add = true)
        {
            List<string> playerIds = Utils.GetSaveData<List<string>>(save, "DominionsPermaDeath");

            if (add)
            {
                if (IsPermaDeath(save, @uid)) return;
                playerIds.Add(@uid);
            }
            else
            {
                playerIds.Remove(@uid);
            }

            Utils.SetSaveData(save, "DominionsPermaDeath", playerIds);
        }

        void CmdPermaDeath(ICoreServerAPI api, CmdArgs args)
        {
            if (args.Length < 2)
            {
                api.Server.LogNotification("Incorrect arguments");
                return;
            }

            IServerPlayerData data = api.PlayerData.GetPlayerDataByLastKnownName(args[0]);

            if (data == null)
            {
                api.Server.LogNotification("Failed to get UID.");
                return;
            }

            string uid = data.PlayerUID;

            //List<string> li = Utils.GetSaveData<List<string>>(api.WorldManager.SaveGame, "cmPermaDeath");
            //li.ForEach(i => api.Server.LogNotification("PermaDeath Item: " + i));

            SetPermaDeath(
                api.WorldManager.SaveGame,
                @uid,
                (args[1] == "true")
            );

            api.Server.LogNotification("Set DominionsPermaDeath from server. ");
        }

        void OnJoin(ICoreServerAPI api, IServerPlayer p)
        {
            if (IsPermaDeath(api.WorldManager.SaveGame, @p.PlayerUID))
            {
                p.Disconnect("You've died.");
            }
        }

        void OnDeath(ICoreServerAPI api, IServerPlayer p)
        {
            SetPermaDeath(api.WorldManager.SaveGame, @p.PlayerUID);

            //Small delay to prevent interfering with server updating
            api.Event.EnqueueMainThreadTask(() => p.Disconnect("You've died"), "");

            api.BroadcastMessageToAllGroups("The life of " + p.ServerData.LastKnownPlayername.ToUpper() + " has been extinguished.", EnumChatType.Notification);
       
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            InitSaveData(api.WorldManager.SaveGame);

            api.RegisterCommand(
                "dominionspermadeath",
                "args[playername][true/false]",
                "",
                (IServerPlayer p, int groupId, CmdArgs args) => CmdPermaDeath(api, args),
                "root"
            );

            api.Event.PlayerJoin += (IServerPlayer p) => OnJoin(api, p);
            api.Event.PlayerDeath += (IServerPlayer p, DamageSource dmg) => OnDeath(api, p);

        }
    }
}
