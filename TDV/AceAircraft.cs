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
                private const byte MaxAceDieCount = 3;
                private ExtendedAudioBuffer taunt;
                private ExtendedAudioBuffer startChargeSound;
                private ExtendedAudioBuffer chargeSound;
                private ExtendedAudioBuffer endChargeSound;
                private OggBuffer stageVoice;
                //Two values below are measured in milliseconds
                private int rechargeTime;
                private int fireTime;
                //Two values below are measured in seconds
                private int maxRechargeTime;
                private int maxFireTime;
                private byte aceDieCount;

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
                        if (!isAI) // load sounds if this is a player (or if Ace needs sounds even as AI)
                            loadSounds(); 
                        else 
                            loadSounds(); // Ace always needs sounds for charging/taunting
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
                        if (taunt != null)
                                taunt.stop();
                        base.muteEngines();
                }

                public override void requestingTerminate()
                {
                        base.requestingTerminate();
                        if (engine != null) engine.stop();
                        if (chargeSound != null) chargeSound.stop();
                        if (endChargeSound != null) endChargeSound.stop();
                        if (startChargeSound != null) startChargeSound.stop();
                        if (taunt != null) taunt.stop();
                        if (stageVoice != null) stageVoice.stopOgg();
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
                                if (Mission.player != null && getPosition(Mission.player).distance >= 15.0)
                                {
                                        x = Mission.player.x - 3f;
                                        y = Mission.player.y + 3f;
                                        playTaunt(soundPath + "j2-"
                                         + Common.getRandom(1, 3)
                                         + ".wav");
                                }
                        } //if can fire
                        regenerate();
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

                        playTaunt(soundPath + "j" + Common.getRandom(3, 4) + ".wav");
                }

                private void specialMove()
                {
                        playTaunt(soundPath + "j1-1.wav");
                        weapon.weaponIndex = WeaponTypes.guns;
                        do
                        {
                                fireWeapon();
                        } while (weapon.increaseWeaponIndex() != WeaponTypes.guns);
                }

                private void playTaunt(String t)
                {
                        if (taunt != null && DSound.isPlaying(taunt))
                                return;
                        taunt = DSound.LoadSound(t);
                        DSound.PlaySound(taunt, true, false);
                }

                public void warnPlayer()
                {
                        if (taunt != null)
                                taunt.stop();
                        playTaunt(soundPath + "j1-2.wav");
                }

                private void regenerate()
                {
                        if (hit() && (++aceDieCount) < MaxAceDieCount)
                        { //if need to reincarnate,
                                stopCharge(false);
                                playStageVoice(aceDieCount);
                                cause = Interaction.Cause.none;
                                strengthenArmor(aceDieCount);
                        }
                }

                private void playStageVoice(byte stage)
                {
                        if (stage < 1 || stage >= MaxAceDieCount)
                                return;
                        if (stageVoice != null)
                                stageVoice.stopOgg();
                        stageVoice = DSound.loadOgg(DSound.SoundPath + "\\jj5-" + stage + ".ogg");
                        stageVoice.play();
                }

                private void strengthenArmor(byte stage)
                {
                        if (stage == 1)
                        {
                                setDamagePoints(25000);
                                maxProbability = 40;
                                maxFireTime = 60;
                                setStrafeTime(30, 10);
                        }
                        if (stage == 2)
                        {
                                setDamagePoints(30000);
                                maxProbability = 70;
                                maxFireTime = 120;
                                setStrafeTime(50, 30);
                        }
                }

                private void tickWeaponTimer()
                {
                        if (fireTime == 0)
                        {
                                rechargeTime += Common.intervalMS;
                                if (rechargeTime / 1000 >= maxRechargeTime)
                                {
                                        stopCharge(true);
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
                                        maxRechargeTime = Common.getRandom(5, 500);
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
                        if (Mission.player != null)
                                ((Aircraft)Mission.player).announceRecharging();
                }

                private void stopCharge(bool justStopSounds)
                {
                        if (rechargeTime == 0)
                                return;
                        playSound(endChargeSound, true, false);
                        chargeSound.stop();
                        startChargeSound.stop();
                        if (justStopSounds)
                        {
                                if (Mission.player != null)
                                        ((Aircraft)Mission.player).announceDoneCharging();
                        }
                        else
                        {
                                rechargeTime = 0;
                                fireTime = 1;
                        }
                }

                public override void freeResources()
                {
                        base.freeResources();
                        DSound.unloadSound(ref engine);
                        DSound.unloadSound(ref startChargeSound);
                        DSound.unloadSound(ref chargeSound);
                        DSound.unloadSound(ref endChargeSound);
                        DSound.unloadSound(ref taunt);
                        if (stageVoice != null)
                                stageVoice.stopOgg();
                }

        }
}
