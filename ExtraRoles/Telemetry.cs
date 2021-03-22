using HarmonyLib;
using InnerNet;
using System;
using System.Net.Http;

namespace ExtraRolesMod
{

    //This is a class that sends a ping to my public api so people can see a player counter. Go to http://computable.us:5001/api/playercount to view the people currently playing.
    //No sensitive information is logged, viewed, or used in any way.
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
    class GameUpdate
    {
        static readonly HttpClient client = new HttpClient();
        static DateTime? lastGuid = null;
        static Guid clientGuid = Guid.NewGuid();

        static void Postfix()
        {
            lastGuid ??= DateTime.UtcNow.AddSeconds(-20);

            if (lastGuid.Value.AddSeconds(20).Ticks >= DateTime.UtcNow.Ticks)
                return;

            client.PostAsync("http://computable.us:5001/api/ping?guid=" + clientGuid, null);
            lastGuid = DateTime.UtcNow;
        }
    }
}
