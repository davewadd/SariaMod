
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
	public class ZZBeamHold : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;

		// The vanilla Last Prism is an animated item with 5 frames of animation. We copy that here.
		private const int NumAnimationFrames = 5;

		// This controls how many individual beams are fired by the Prism.
		public const int NumBeams = 1;

		// This value controls how many frames it takes for the Prism to reach "max charge". 60 frames = 1 second.
		public const float MaxCharge = 180f;

		// This value controls how many frames it takes for the beams to begin dealing damage. Before then they can't hit anything.
		public const float DamageStart = 30f;

		// This value controls how sluggish the Prism turns while being used. Vanilla Last Prism is 0.08f.
		// Higher values make the Prism turn faster.
		private const float AimResponsiveness = 0.08f;

		// This value controls how frequently the Prism emits sound once it's firing.
		private const int SoundInterval = 20;

		// These values place caps on the mana consumption rate of the Prism.
		// When first used, the Prism consumes mana once every MaxManaConsumptionDelay frames.
		// Every time mana is consumed, the pace becomes one frame faster, meaning mana consumption smoothly increases.
		// When capped out, the Prism consumes mana once every MinManaConsumptionDelay frames.
		private const float MaxManaConsumptionDelay = 15f;
		private const float MinManaConsumptionDelay = 5f;

		// This property encloses the internal AI variable Projectile.ai[0]. It makes the code easier to read.
		private float FrameCounter {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		// This property encloses the internal AI variable Projectile.ai[1].
		private float NextManaFrame {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		// This property encloses the internal AI variable Projectile.localAI[0].
		// localAI is not automatically synced over the network, but that does not cause any problems in this case.
		private float ManaConsumptionRate {
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = NumAnimationFrames;
			ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 100000;
			// Signals to Terraria that this Projectile requires a unique identifier beyond its index in the Projectile array.
			// This prevents the issue with the vanilla Last Prism where the beams are invisible in multiplayer.
			ProjectileID.Sets.NeedsUUID[Projectile.type] = true;

			
		}
		


		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Vector2 rrp = Projectile.Center;
			int owner = player.whoAmI;
			int GiantMoth = ModContent.ProjectileType<LocatorTurret>();
			for (int i = 0; i < 1000; i++)
			{
				
				
					if (Main.projectile[i].active && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner)))
					{

						{
							rrp = Main.projectile[i].Center;
						}

					}

				
			}
			Projectile.alpha = 0;

			// Update the frame counter.
			Projectile.alpha = 0;
			FrameCounter += 1f;

			// Update Projectile visuals and sound.


			// Update the Prism's position in the world and relevant variables of the player holding it.
			

			// Update the Prism's behavior: project beams on frame 1, consume mana, and despawn if out of mana.
			if (Projectile.owner == Main.myPlayer) {
				// Slightly re-aim the Prism every frame so that it gradually sweeps to point towards the mouse.
				UpdateAim(rrp, .0000000000001f);

				// player.CheckMana returns true if the mana cost can be paid. Since the second argument is true, the mana is actually consumed.
				// If mana shouldn't consumed this frame, the || operator short-circuits its evaluation player.CheckMana never executes.
				

				// The Prism immediately stops functioning if the player is Cursed (player.noItems) or "Crowd Controlled", e.g. the Frozen debuff.
				// player.channel indicates whether the player is still holding down the mouse button to use the item.
				bool stillInUse = false;
				if (player.ownedProjectileCounts[ModContent.ProjectileType<LocatorTurret>()] >= 1f)
				{
					stillInUse = true;
				}
				if (player.ownedProjectileCounts[ModContent.ProjectileType<LocatorTurret>()] <= 0f)
				{
					Projectile.Kill();
				}
				// Spawn in the Prism's lasers on the first frame if the player is capable of using the item.
				if (stillInUse && FrameCounter == 1f) {
					FireBeams();
				}
				else if (!stillInUse)
				{
					Projectile.Kill();
				}
				// If the Prism cannot continue to be used, then destroy it immediately.

			}

			// This ensures that the Prism never times out while in use.
			Projectile.timeLeft = 8;
		}

		

		

		

		

		

		private void UpdateAim(Vector2 source, float speed) {
			// Get the player's current aiming direction as a normalized vector.
			Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}

			// Change a portion of the Prism's current velocity so that it points to the mouse. This gives smooth movement over time.
			aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, AimResponsiveness));
			aim *= speed;

			if (aim != Projectile.velocity) {
				Projectile.netUpdate = true;
			}
			Projectile.velocity = aim;
		}

		private void FireBeams() {
			// If for some reason the beam velocity can't be correctly normalized, set it to a default value.
			Vector2 beamVelocity = Vector2.Normalize(Projectile.velocity);
			if (beamVelocity.HasNaNs()) {
				beamVelocity = -Vector2.UnitY;
			}

			// This UUID will be the same between all players in multiplayer, ensuring that the beams are properly anchored on the Prism on everyone's screen.
			int uuid = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);

			int damage = Projectile.damage;
			float knockback = Projectile.knockBack;
			for (int b = 0; b < 1; ++b) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, beamVelocity, ModContent.ProjectileType<ZZBeam>(), damage, knockback, Projectile.owner, b, uuid);
			}

			// After creating the beams, mark the Prism as having an important network event. This will make Terraria sync its data to other players ASAP.
			Projectile.netUpdate = true;
		}

		// Because the Prism is a holdout Projectile and stays glued to its user, it needs custom drawcode.
		
	}
}