using Game;

namespace ILTG
{
	public class ILTGLoader : ModLoader
	{
		public override void __ModInitialize()
		{
		    ModsManager.RegisterHook("OnProjectLoaded", this);
            //ModsManager.RegisterHook("SkyDrawExtra", this);
		}
		
	}
}
