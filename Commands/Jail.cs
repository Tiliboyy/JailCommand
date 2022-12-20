using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using InventorySystem;
using MEC;
using PlayerRoles;
using Vector3 = UnityEngine.Vector3;
using CommandSystem;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace JailCommand.Commands
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public class Jail : ICommand
	{
		public static Dictionary<Player,JailedPlayer> Players = new();
		public string Command { get; } = "Jail";
		public string[] Aliases { get; } = new string[] { "JailPlayer" };
		public string Description { get; } = "Jails a Player";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (arguments.Count != 1)
			{
				response = "Usage: Jail <id>";
				return true;
			}
			Player player = null;
			foreach (var ply in ReferenceHub.AllHubs.ToList().Select(plyser => Player.Get(plyser)).Where(ply => !ply.IsServer))
			{
				if (!int.TryParse(arguments.At(0), out var id))
				{
					continue;
				}

				if(ply.ReferenceHub.PlayerId != id) continue;
				player = Player.Get(ply.ReferenceHub);
				break;
			}
			if (player == null)
			{
				response = "Player not found";
				return true;
			}

			if (Players.ContainsKey(player))
			{
				Players.TryGetValue(player, out var e);
				player.SetRole(e.SRole);
				Timing.CallDelayed(0.2f, () =>
				{
					player.Position = e.SPosition;
					foreach (var variable in e.SItems)
						player.ReferenceHub.inventory.ServerAddItem(variable);
					foreach (var variable in e.SAmmo)
						player.AddAmmo(variable.Key, variable.Value);
				});
				Players.Remove(player);
				response = "Player " + player.Nickname + " unjailed";
				return true;
			}
			var items = player.ReferenceHub.inventory.UserInventory.Items.Select(variable => variable.Value.ItemTypeId).ToList();
			var ammo = player.ReferenceHub.inventory.UserInventory.ReserveAmmo.ToDictionary(variable => variable.Key, variable => variable.Value);
			JailedPlayer plyer = new() { SRole = player.Role, SPosition = player.Position, SItems = items , SAmmo = ammo};
			Players.Add(player, plyer);
			player.Role = RoleTypeId.Tutorial;
			player.Position = new Vector3(39, 1014, -32);
			response = "Player " + player.Nickname + " jailed";
			return true;
		}
	}



	public struct JailedPlayer
	{
		public List<ItemType> SItems { get; set; }
		
		public Dictionary<ItemType, ushort> SAmmo { get; set; }
		public Vector3 SPosition { get; set; }
		public RoleTypeId SRole { get; set; }
		

	}
}