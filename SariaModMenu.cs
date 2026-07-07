using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
namespace SariaMod
{
	public class ExampleModMenu : ModMenu
	{
		public override Asset<Texture2D> Logo => base.Logo;
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Title");
		public override string DisplayName => "Saria ModMenu";
	}
}
