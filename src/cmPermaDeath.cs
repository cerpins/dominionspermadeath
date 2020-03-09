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
            if (IsPermaDeath(save, @uid)) return;

            List<string> playerIds = Utils.GetSaveData<List<string>>(save, "cmPermaDeath");

            if (add)
            {
                playerIds.Add(@uid);
            }
            else
            {
                playerIds.Remove(@uid);
            }

            Utils.SetSaveData(save, "cmPermaDeath", playerIds);
        }

        //commands
        void CmdPermaDeath(ICoreServerAPI api, IServerPlayer p, CmdArgs args)
        {
            if (args.Length < 2)
            {
                p.SendMessage(GlobalConstants.AllChatGroups, "Incorrect arguments.", EnumChatType.AllGroups);
                return;
            }
            
            string uid = api.PlayerData.GetPlayerDataByLastKnownName(args[0]).PlayerUID;

            SetPermaDeath(
                api.WorldManager.SaveGame, 
                @uid, 
                args[1] == "true"
            );

            p.SendMessage(GlobalConstants.AllChatGroups, "Player PermaDeath toggled.", EnumChatType.AllGroups);
        }

        // events
        void OnJoin(ICoreServerAPI api, IServerPlayer p)
        {
            if (IsPermaDeath(api.WorldManager.SaveGame, @p.PlayerUID))
            {
                p.Disconnect();
                api.BroadcastMessageToAllGroups("Wails echo.", EnumChatType.Notification);
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
                (IServerPlayer p, int groupId, CmdArgs args) => CmdPermaDeath(api, p, args), 
                "root"
            );

            api.Event.PlayerJoin += (IServerPlayer p) => OnJoin(api, p);
            api.Event.PlayerDeath += (IServerPlayer p, DamageSource dmg) => OnDeath(api, p);
        }

    }
}
