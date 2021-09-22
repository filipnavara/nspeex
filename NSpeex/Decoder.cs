using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSpeex
{
    public class Decoder
    {
        public int FrameSize
        {
            get;
        }

        public Decoder(BandMode mode)
        {
        }

        /// <summary>
        /// Decode one frame of speech from bit-stream bits. 
        /// The output speech is saved written to output
        /// </summary>
        /// <param name="bits">Bit-stream from which to decode the frame (NULL if the packet was lost)</param>
        /// <param name="output">Where to write the decoded frame</param>
        /// <returns>return status (0 for no error, -1 for end of stream, -2 corrupt stream)</returns>
        public int Decode(Bits bits, float[] output)
        {
            return 0;
        }

        /// <summary>
        /// Decode one frame of speech from bit-stream bits. 
        /// The output speech is saved written to output
        /// </summary>
        /// <param name="bits">Bit-stream from which to decode the frame (NULL if the packet was lost)</param>
        /// <param name="output">Where to write the decoded frame</param>
        /// <returns>return status (0 for no error, -1 for end of stream, -2 corrupt stream)</returns>
        public int Decode(Bits bits, short[] output)
        {
            return 0;
        }
    }
}
