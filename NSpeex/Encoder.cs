using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSpeex
{
    public class Encoder
    {
        public int FrameSize
        {
            get;
        }

        public int Quality
        {
            get;
            set;
        }

        public bool IsVBR
        {
            get;
            set;
        }

        public bool IsABR
        {
            get;
            set;
        }

        public bool IsVAD
        {
            get;
            set;
        }

        public bool IsDTX
        {
            get;
            set;
        }

        private SpeexMode mode;

        public Encoder(BandMode mode)
        {
        }
        
        /// <summary>
        /// Uses an existing encoder state to encode one frame of speech pointed to by
        /// "input". The encoded bit-stream is saved in "bits".
        /// </summary>
        /// <param name="input">frame to encode</param>
        /// <param name="to">encoded bit-stream is saved to</param>
        /// <returns>0 if frame needs not be transmitted (DTX only), 1 otherwise</returns>
        public int Encode(float[] input, Bits bits)
        {
            return 0;
        }

        /// <summary>
        /// Uses an existing encoder state to encode one frame of speech pointed to by
        /// "input". The encoded bit-stream is saved in "bits".
        /// </summary>
        /// <param name="input">frame to encode</param>
        /// <param name="to">encoded bit-stream is saved to</param>
        /// <returns>0 if frame needs not be transmitted (DTX only), 1 otherwise</returns>
        public int Encode(short[] input, Bits bits)
        {
            return 0;
        }
    }
}
