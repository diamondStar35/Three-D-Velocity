/* This source is provided under the GNU AGPLv3  license. You are free to modify and distribute this source and any containing work (such as sound files) provided that:
* - You make available complete source code of modifications, even if the modifications are part of a larger project, and make the modified work available under the same license (GNU AGPLv3).
* - You include all copyright and license notices on the modified source.
* - You state which parts of this source were changed in your work
* Note that containing works (such as SharpDX) may be available under a different license.
* Copyright (C) Munawar Bijani
*/
using System;
using System.IO;
using BPCSharedComponent.ExtendedAudio;
using BPCSharedComponent.VectorCalculation;

namespace TDV
{
	public class AceAircraft : Aircraft
	{
		private ExtendedAudioBuffer startChargeSound;
		private ExtendedAudioBuffer chargeSound;
		private ExtendedAudioBuffer endChargeSound;
		//Two values below are measured in milliseconds
		private int rechargeTime;
		private int fireTime;
		//Two values below are measured in seconds
		private int maxRechargeTime;
		private int maxFireTime;

		public AceAircraft(float x, float y)
			: base(0, 1000, "Ace", true, new Track(Options.currentTrack))
		{
			showInList = true;
			this.x = x;
			this.y = y;
			maxProbability = 30;
			weapon = new Weapons(this,
			 WeaponTypes.guns,
			 WeaponTypes.missile,
			 WeaponTypes.laserCannonSystem,
			 WeaponTypes.cruiseMissile,
			 WeaponTypes.explosiveMissile);
			weapon.setInfiniteAmmunition();
			startAtHeight(1000f);
			setDamagePoints(10000);
			setStrafeTime(20, 10);
			liftSpeed = 200;
			//set the two values below to initial defaults so he starts firing
			//when the match starts
			fireTime = 1;
			maxFireTime = 30;
		}

		public AceAircraft()
			: this(0f, 0f)
		{ }


		protected override void loadSounds()
		{
			base.loadSounds();
			engine = loadSound(soundPath + "e9.wav");
			explodeSound = loadSound(soundPath + "d1.wav");
			startChargeSound = loadSound(soundPath + "cs.wav");
			chargeSound = loadSound(soundPath + "cl.wav");
			endChargeSound = loadSound(soundPath + "ce.wav");
		}

		protected override void muteEngines()
		{
			engine.stop();
			chargeSound.stop();
			endChargeSound.stop();
			startChargeSound.stop();
			base.muteEngines();
		}


		public override void move()
		{
			if (readyToTerminate())
			{
				isProjectorStopped = true;
				return;
			}

			tickWeaponTimer();
			if (rechargeTime != 0)
				playSound(chargeSound, false, true); //move the sound in 3d space
			if (fireTime != 0)
			{
				if (Common.getRandom(1, 300) == 150)
					teleport();
				if (Common.getRandom(1, 300) == 5)
					specialMove();
			} //if can fire
			base.move();
		}

		private void teleport()
		{
			if (Mission.player == null) return;
			
			switch (Common.getRandom(1, 4))
			{
				case 1:
					x = Mission.player.x - 2;
					y = Mission.player.y - 2;
					direction = getPosition(Mission.player).degrees;
					break;
				case 2:
					//end up in front of player
					x = Mission.player.x;
					y = Mission.player.y + 2;
					direction = Mission.player.direction;
					break;
				case 3:
					x = Mission.player.x + 2;
					y = Mission.player.y - 2;
					break;
				case 4:
					x = Mission.player.x + 20;
					y = Mission.player.y + 10;
					break;
			} //switch
		}

		private void specialMove()
		{
			weapon.weaponIndex = WeaponTypes.guns;
			do
			{
				fireWeapon();
			} while (weapon.increaseWeaponIndex() != WeaponTypes.guns);
		}

		private void tickWeaponTimer()
		{
			if (fireTime == 0)
			{
				rechargeTime += Common.intervalMS;
				if (rechargeTime / 1000 >= maxRechargeTime)
				{
					stopCharge();
					rechargeTime = 0;
					fireTime = 1;
					maxFireTime = Common.getRandom(10, 30);
				}
			}
			else
			{ //if fire time counting
				fireTime += Common.intervalMS;
				if (fireTime / 1000 >= maxFireTime)
				{
					fireTime = 0;
					rechargeTime = 1;
					startCharge();
					maxRechargeTime = Common.getRandom(5, 50);
				} //if firetime exceeded
			} //if firetime counting
		}

		protected override string fireWeapon()
		{
			if (fireTime == 0)
				return null;
			return base.fireWeapon();
		}

		private void startCharge()
		{
			playSound(startChargeSound, true, false);
			playSound(chargeSound, true, true);
		}

		private void stopCharge()
		{
			if (rechargeTime == 0)
				return;
			playSound(endChargeSound, true, false);
			chargeSound.stop();
			startChargeSound.stop();
			rechargeTime = 0;
			fireTime = 1;
		}

		public override void freeResources()
		{
			base.freeResources();
			DSound.unloadSound(ref engine);
			DSound.unloadSound(ref startChargeSound);
			DSound.unloadSound(ref chargeSound);
			DSound.unloadSound(ref endChargeSound);
		}

	}
}
