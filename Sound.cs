/*
  Copyright (c) Moying-moe All rights reserved. Licensed under the MIT license.
  See LICENSE in the project root for license information.
*/

using System;
using System.Windows.Media;

namespace SuperIoTestProgram
{
    internal class Sound
    {
        #region Singleton
        private static readonly Lazy<Sound> _lazy = new Lazy<Sound>(() => new Sound());
        public static Sound Instance { get { return _lazy.Value; } }
        #endregion

        private MediaPlayer _soundStart = new MediaPlayer();
        private MediaPlayer _soundStop = new MediaPlayer();

        private Sound()
        {
            _soundStart.Open(new Uri("sound/start.mp3", UriKind.Relative));
            _soundStart.Volume = 0.2d;

            _soundStop.Open(new Uri("sound/stop.mp3", UriKind.Relative));
            _soundStop.Volume = 0.2d;
        }

        public bool ForceInitialize()
        {
            return _soundStart.HasAudio && _soundStop.HasAudio;
        }

        public void PlayStart()
        {
            _soundStart.Stop();
            _soundStart.Play();
        }

        public void PlayStop()
        {
            _soundStop.Stop();
            _soundStop.Play();
        }
    }
}
