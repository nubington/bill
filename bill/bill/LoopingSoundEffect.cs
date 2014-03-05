using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace bill
{
    class LoopingSoundEffect
    {
        public static List<LoopingSoundEffect> LoopingSoundEffects = new List<LoopingSoundEffect>();

        private static bool isMuted;

        private SoundEffectInstance sound;
        private bool isStopped;
        private float volume;

        public LoopingSoundEffect(SoundEffect soundEffect, float volume)
        {
            sound = soundEffect.CreateInstance();
            sound.IsLooped = true;
            this.volume = sound.Volume = volume;
            LoopingSoundEffects.Add(this);
        }

        public void Start()
        {
            sound.Play();
            isStopped = false;
        }

        public void Stop()
        {
            isStopped = true;
            sound.Stop();
        }

        public bool IsPlaying
        {
            get
            {
                return !isStopped;
            }
        }
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = sound.Volume = value;
            }
        }

        public static bool IsMuted
        {
            get
            {
                return isMuted;
            }
            set
            {
                isMuted = value;
                if (isMuted)
                    foreach (LoopingSoundEffect s in LoopingSoundEffects)
                        s.sound.Volume = 0;
                else
                    foreach (LoopingSoundEffect s in LoopingSoundEffects)
                        s.sound.Volume = s.volume;
            }
        }
    }
}