using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cmpermadeath.src
{
    class cmPermaDeath : ModSystem
    {
        bool IsAutokick(IServerPlayerData d)
        {
            string kickValue;
            if (!d.CustomPlayerData.TryGetValue("cmAutokick", out kickValue)) return false;

            return (kickValue == "1");
        }

        void SetAutokick(IServerPlayerData p, string flag = "1")
        {
            p.CustomPlayerData["cmAutokick"] = flag;
        }

        void CommandUnautokick(IServerPlayer p, string name, ICoreServerAPI api)
        {
            IServerPlayerData target = api.PlayerData.GetPlayerDataByLastKnownName(name);
            SetAutokick(target, "0");

            p.SendMessage(GlobalConstants.GeneralChatGroup, "The spark of life has been relit.", EnumChatType.CommandSuccess);
        }

        // events
        void OnJoin(IServerPlayer p, ICoreServerAPI api)
        {
            if (!IsAutokick(p.ServerData)) return;
            p.Disconnect();
            api.BroadcastMessageToAllGroups(
                "Wails echo.",
                EnumChatType.Notification
            );
        }

        void OnDeath(IServerPlayer p, ICoreServerAPI api)
        {
            api.Server.LogEvent("Setting player as autokick.");
            SetAutokick(p.ServerData);
            p.Disconnect("Death was imminent");

            api.BroadcastMessageToAllGroups(
                "The life of " + p.ServerData.LastKnownPlayername.ToUpper() + " has been extinguished.", 
                EnumChatType.Notification
            );
            //send out soundeffect globally
        }


        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            api.RegisterCommand("unautokick", "remove autokick from player by name", "", (IServerPlayer p, int groupId, CmdArgs args) =>
            CommandUnautokick(p, args[0], api), "root");

            api.Event.PlayerJoin += (IServerPlayer p) => OnJoin(p, api);
            api.Event.PlayerDeath += (IServerPlayer p, DamageSource dmg) => OnDeath(p, api);
        }

    }
}
