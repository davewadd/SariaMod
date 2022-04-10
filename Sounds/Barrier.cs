using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Sounds
{
	public class Barrier: ModSound
	{
		public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type)
		{
			soundInstance = base.sound.CreateInstance();
			soundInstance.Volume = volume * 1f;
			soundInstance.Pan = pan;
			Main.PlaySoundInstance(soundInstance);
			return soundInstance;
		}
	}
}
