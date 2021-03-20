using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;
using Essentials.CustomOptions;
using static Pheonix.PheonixRole;
using Reactor.Unstrip;
using UnityEngine;
using Reactor.Extensions;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using System.Linq;
using System;
using System.Net;

//change the namespace to one you will use through this entire mod.
namespace Pheonix
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class HarmonyMain : BasePlugin
    {
        //id the game can call, change senseirole at the end.
        public const string Id = "gg.reactor.pheonixrole";
        public Harmony Harmony { get; } = new Harmony(Id);
        //Settings Below
        // ability 1: respawns by himself
        public static Essentials.Options.CustomToggleOption undeadCanTalk = Essentials.Options.CustomOption.AddToggle("Undead Pheonix Speaks", false);
        // ability 2:  droplet of life
        // ability 3: flash
        public static Essentials.Options.CustomNumberOption flashDuration = Essentials.Options.CustomOption.AddNumber("Flash Duration", 5f, 1f, 10f, 1f);
        // impostor ability 1: extinguish
        public static Essentials.Options.CustomNumberOption extinguishCooldown = Essentials.Options.CustomOption.AddNumber("Extinguish Cooldown", 15f, 5f, 60f, 2.5f);
        //Allows for harmony patching below

        //adding cheep servers aswell
        public ConfigEntry<string> Name { get; set; }
        public ConfigEntry<string> Ip { get; set; }
        public ConfigEntry<ushort> Port { get; set; }
        public override void Load()
        {
            Harmony.PatchAll();
            Name = Config.Bind("Server", "Name", "CheepYT - EU");
            Ip = Config.Bind("Server", "Ipv4 or Hostname", "207.180.234.175");
            Port = Config.Bind("Server", "Port", (ushort)22023);
            var defReg = AOBNFCIHAJL.DefaultRegions.ToList();
            if (Uri.CheckHostName(Ip.Value).ToString() == "Dns")
            {
                foreach (IPAddress adress in Dns.GetHostAddresses(Ip.Value))
                {
                    if (adress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        Ip.Value = adress.ToString();
                        break;
                    }
                }
            }
            defReg.Insert(0, new OIBMKGDLGOG(Name.Value, Ip.Value, new[] { new PLFDMKKDEMI($"{Name.Value}-Master-1", Ip.Value, Port.Value) }));
            AOBNFCIHAJL.DefaultRegions = defReg.ToArray();
        }
    }
}