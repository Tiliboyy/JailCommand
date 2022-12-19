namespace JailCommand
{
	using PluginAPI.Core;
	using PluginAPI.Core.Attributes;
	using PluginAPI.Events;

	public class MainClass
	{
		[PluginEntryPoint("JailCommand", "1.0.0", "Plugin to Jail players.", "Tiliboyy")]
		void LoadPlugin()
		{
			Log.Info("Loading JailCommand...");
			EventManager.RegisterEvents(this);
			EventManager.RegisterEvents<EventHandlers>(this);
		}

		[PluginConfig]
		public Config PluginConfig;
	}
}
