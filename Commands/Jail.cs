using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using InventorySystem;
using MEC;
using PlayerRoles;
using Vector3 = UnityEngine.Vector3;

namespace JailCommand.Commands
{
	using CommandSystem;
	using PluginAPI.Core;
	using PluginAPI.Core.Interfaces;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Returns the list of players
	/// - CommandExpansion Command x
	/// </summary>
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public class Jail : ICommand
	{
		public static Dictionary<Player,JailedPlayer> players = new();
		public string Command { get; } = "Jail";

		public string[] Aliases { get; } = new string[] { "JailPlayer" };

		public string Description { get; } = "Jails a Player";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (arguments.Count != 1)
			{
				response = "Usage: Jail <Username>";
				return true;
			}
			

			Player player = null;
			foreach (var plyser in ReferenceHub.AllHubs.ToList())
			{
				var ply = Player.Get(plyser);
				if(ply.IsServer) continue;
				if (int.TryParse(arguments.At(0), out var id))
				{
					if(ply.ReferenceHub.PlayerId != id) continue;
					player = Player.Get(ply.ReferenceHub);
					break;

				}
			}
			if (player == null)
			{
				response = "Player not found";
				return true;
			}
			Log.Info(player.Nickname);


			if (players.ContainsKey(player))
			{
				players.TryGetValue(player, out var e);
				player.SetRole(e.SRole);
				Timing.CallDelayed(0.2f, () =>
				{
					player.Position = e.SPosition;
					foreach (var variable in e.SItems)
					{
						player.ReferenceHub.inventory.ServerAddItem(variable);
					}
					foreach (var variable in e.SAmmo)
					{
						player.AddAmmo(variable.Key, variable.Value);

					}
					

				});
				players.Remove(player);
				response = "Player " + player.Nickname + " unjailed";
				return true;


			}

			var items = new List<ItemType>();
			foreach (var variable in player.ReferenceHub.inventory.UserInventory.Items)
			{
				items.Add(variable.Value.ItemTypeId);
			}

			var ammo = new Dictionary<ItemType, ushort>();

			foreach (var variable in player.ReferenceHub.inventory.UserInventory.ReserveAmmo)
			{
				ammo.Add(variable.Key, variable.Value);
			}
			
			JailedPlayer plyer = new() { SRole = player.Role, SPosition = player.Position, SItems = items , SAmmo = ammo};

			players.Add(player, plyer);
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