using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

namespace cmpermadeath.src
{
    class CmPermaDeath : ModSystem
    {
        void InitSaveData(ISaveGame save)
        {
            //Make sure there is no null data, else get calls will fail.
            if (!Utils.HasSaveData(save, "cmPermaDeath"))
            {
                Utils.SetSaveData(save, "cmPermaDeath", new List<string>());
            }
        }

        bool IsPermaDeath(ISaveGame save, string uid)
        {
            List<string> playerIds = Utils.GetSaveData<List<string>>(save, "cmPermaDeath");

            return playerIds.Exists(puid => (@puid == @uid));
        }

        void SetPermaDeath(ISaveGame save, string uid, bool add = true)
        {
            List<string> playerIds = Utils.GetSaveData<List<string>>(save, "cmPermaDeath");

            if (add)
            {
                if (IsPermaDeath(save, @uid)) return;
                playerIds.Add(@uid);
            }
            else
            {
                playerIds.Remove(@uid);
            }

            Utils.SetSaveData(save, "cmPermaDeath", playerIds);
        }

        //commands
        void CmdPermaDeath(ICoreServerAPI api, CmdArgs args)
        {
            if (args.Length < 2)
            {
                api.Server.LogNotification("Incorrect arguments");
                return;
            }
            
            string uid = api.PlayerData.GetPlayerDataByLastKnownName(args[0])?.PlayerUID;

            if (@uid == null)
            {
                api.Server.LogNotification("Failed to get UID.");
                return;
            }

            //List<string> li = Utils.GetSaveData<List<string>>(api.WorldManager.SaveGame, "cmPermaDeath");
            //li.ForEach(i => api.Server.LogNotification("PermaDeath Item: " + i));

            SetPermaDeath(
                api.WorldManager.SaveGame, 
                @uid, 
                (args[1] == "true")
            );

            api.Server.LogNotification("Set PermaDeath from server. ");
        }

        // events
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
            p.Disconnect("You've died."); 

            api.BroadcastMessageToAllGroups("The life of " + p.ServerData.LastKnownPlayername.ToUpper() + " has been extinguished.", EnumChatType.Notification);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            InitSaveData(api.WorldManager.SaveGame);

            api.RegisterCommand(
                "permadeath", 
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
