namespace JailCommand
{
	using PluginAPI.Core;
	using PluginAPI.Core.Attributes;
	using PluginAPI.Events;

	public class Plugin
	{
		[PluginEntryPoint(
			"JailCommand",
			"1.0.0", 
			"Plugin to Jail players.", 
			"Tiliboyy")]
		private void LoadPlugin()
		{

		}

		[PluginConfig]
		public Config PluginConfig;
	}
}
