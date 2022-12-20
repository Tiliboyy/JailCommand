using System.Linq;
using InventorySystem;
using MEC;
using PlayerRoles;
using Vector3 = UnityEngine.Vector3;
using CommandSystem;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using InventorySystem.Items.Firearms;

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
					player.ReferenceHub.inventory.UserInventory.Items.Clear();
					player.ReferenceHub.inventory.UserInventory.ReserveAmmo.Clear();
					player.ReferenceHub.inventory.SendItemsNextFrame = true;
					player.Position = e.SPosition;
					foreach (var variable in e.SItems)
					{
						player.ReferenceHub.inventory.ServerAddItem(variable);
					}

					foreach (var tuple in e.SArms)
					{
						var item = player.ReferenceHub.inventory.ServerAddItem(tuple.type);
						if(item is Firearm firearm)
							firearm.Status = new FirearmStatus(tuple.amount, tuple.flags, tuple.attachment);
					}

					foreach (var effect in e.SEffects.Where(effect => effect.Value.duration != 0 && effect.Value.intensity != 0))
					{
						player.ReferenceHub.playerEffectsController.AllEffects[effect.Key].ServerSetState(effect.Value.intensity, effect.Value.duration, false);
					}
					foreach (var variable in e.SAmmo)
						player.AddAmmo(variable.Key, variable.Value);
				});
				Players.Remove(player);
				response = "Player " + player.Nickname + " unjailed";
				return true;
			}

			var items = new List<ItemType>();
			var arms = new List<(ItemType type, byte amount, uint attachments, FirearmStatusFlags flags)>();
			foreach (var variable in player.ReferenceHub.inventory.UserInventory.Items)
			{
				if (variable.Value is Firearm firearm)
				{
					arms.Add((variable.Value.ItemTypeId, firearm.Status.Ammo, firearm.Status.Attachments, firearm.Status.Flags));
				}
				else
				{
					items.Add(variable.Value.ItemTypeId);
				}
			}
			
			var ammo = player.ReferenceHub.inventory.UserInventory.ReserveAmmo.ToDictionary(variable => variable.Key, variable => variable.Value);
			JailedPlayer jailedPlayer = new()
			{
				SRole = player.Role, 
				SPosition = player.Position, 
				SItems = items , 
				SAmmo = ammo, 
				SArms = arms, 
				SEffects = new Dictionary<int, (float duration, byte intensity)>()
			};
			for (int i = 0; i < player.ReferenceHub.playerEffectsController.AllEffects.Length; i++)
			{
				var effect = player.ReferenceHub.playerEffectsController.AllEffects[i];
				jailedPlayer.SEffects[i] = (effect.Duration, effect.Intensity);
			}
			Players.Add(player, jailedPlayer);
			player.Role = RoleTypeId.Tutorial;
			player.Position = new Vector3(39, 1014, -32);
			response = "Player " + player.Nickname + " jailed";
			return true;
		}
	}



	public struct JailedPlayer
	{
		public List<ItemType> SItems { get; set; }
		public List<(ItemType type, byte amount, uint attachment, FirearmStatusFlags flags)> SArms { get; set; }
		public Dictionary<int, (float duration, byte intensity)> SEffects { get; set; }
		public Dictionary<ItemType, ushort> SAmmo { get; set; }
		public Vector3 SPosition { get; set; }
		public RoleTypeId SRole { get; set; }
	}
}