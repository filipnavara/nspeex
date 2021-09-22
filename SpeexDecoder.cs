//
// Copyright (C) 2003 Jean-Marc Valin
// Copyright (C) 1999-2003 Wimba S.A., All Rights Reserved.
// Copyright (C) 2008 Filip Navara
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// - Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// 
// - Neither the name of the Xiph.org Foundation nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;

namespace NSpeex
{
	/// <summary>
	/// Main Speex Decoder class. This class decodes the given Speex packets into PCM
	/// 16bit samples.
	/// 
	/// Here's an example that decodes and recovers one Speex packet.
	/// 
	/// SpeexDecoder speexDecoder = new SpeexDecoder();
	/// speexDecoder.processData(data, packetOffset, packetSize);
	/// byte[] decoded = new byte[speexDecoder.getProcessedBataByteSize()];
	/// speexDecoder.getProcessedData(decoded, 0);
	/// </summary>
	public class SpeexDecoder
	{
		/// <summary>
		/// Version of the Speex Decoder
		/// </summary>
		private const String Version = "Java Speex Decoder v0.9.7 ($Revision: 1.4 $)";

		private int sampleRate;
		private int channels;
		private float[] decodedData;
		private short[] outputData;
		private int outputSize;
		private Bits bits;
		private IDecoder decoder;
		private int frameSize;

		/// <summary>
		/// Constructor
		/// </summary>
		public SpeexDecoder()
		{
			bits = new Bits();
			sampleRate = 0;
			channels = 0;
		}

		/// <summary>
		/// Initialise the Speex Decoder.
		/// </summary>
		/// <param name="mode">the mode of the decoder (0=NB, 1=WB, 2=UWB).</param>
		/// <param name="sampleRate_0">the number of samples per second.</param>
		/// <param name="channels_1">the number of audio channels (1=mono, 2=stereo, ...).</param>
		/// <param name="enhanced">whether to enable perceptual enhancement or not.</param>
		/// <returns>true if initialisation successful.</returns>
		public bool Init(int mode, int sampleRate, int channels, bool enhanced)
		{
			switch (mode)
			{
				case 0: decoder = new NbDecoder(); break;
				// Wideband
				case 1: decoder = new SbDecoder(false); break;
				case 2: decoder = new SbDecoder(true); break;
				// */
				default: return false;
			}

			/* initialize the speex decoder */
			decoder.PerceptualEnhancement = enhanced;
			/* set decoder format and properties */
			this.frameSize = decoder.FrameSize;
			this.sampleRate = sampleRate;
			this.channels = channels;
			int secondSize = sampleRate * channels;
			decodedData = new float[secondSize * 2];
			outputData = new short[secondSize * 2];
			outputSize = 0;
			return true;
		}

		public int SampleRate
		{
			get
			{
				return sampleRate;
			}
		}

		public int Channels
		{
			get
			{
				return channels;
			}
		}

		/// <summary>
		/// Pull the decoded data out into a byte array at the given offset and
		/// returns the number of bytes processed and just read.
		/// </summary>
		/// <returns>the number of bytes processed and just read.</returns>
		public int GetProcessedData(byte[] data, int offset)
		{
			if (outputSize <= 0)
			{
				return outputSize;
			}
			for (int i = 0; i < outputSize; i++)
			{
				int dx = offset + (i << 1);
				data[dx] = (byte)(outputData[i] & 0xff);
				data[dx + 1] = (byte)((outputData[i] >> 8) & 0xff);
			}
			int size = outputSize * 2;
			outputSize = 0;
			return size;
		}

		/// <summary>
		/// Pull the decoded data out into a short array at the given offset and
		/// returns tne number of shorts processed and just read
		/// </summary>
		/// <returns>the number of samples processed and just read.</returns>
		public int GetProcessedData(short[] data, int offset)
		{
			if (outputSize <= 0)
			{
				return outputSize;
			}
			System.Array.Copy(outputData, 0, data, offset, outputSize);
			int size = outputSize;
			outputSize = 0;
			return size;
		}

		public int ProcessedDataByteSize
		{
			get
			{
				return (outputSize * 2);
			}
		}

		/// <summary>
		/// This is where the actual decoding takes place
		/// </summary>
		/// <exception cref="InvalidDataException">If the input stream is invalid.</exception>
		public void ProcessData(byte[] data, int offset, int len)
		{
			if (data == null)
			{
				ProcessData(true);
			}
			else
			{
				// Read packet bytes into bitstream
				bits.ReadFrom(data, offset, len);
				ProcessData(false);
			}
		}

		/// <summary>
		/// This is where the actual decoding takes place.
		/// </summary>
		/// <exception cref="InvalidDataException">If the input stream is invalid.</exception>
		public void ProcessData(bool lost)
		{
			int i;

			// Decode the bitstream
			if (lost)
				decoder.Decode(null, decodedData);
			else
				decoder.Decode(bits, decodedData);
			if (channels == 2)
				decoder.DecodeStereo(decodedData, frameSize);

			// PCM saturation
			for (i = 0; i < frameSize * channels; i++)
			{
				if (decodedData[i] > 32767.0f)
					decodedData[i] = 32767.0f;
				else if (decodedData[i] < -32768.0f)
					decodedData[i] = -32768.0f;
			}

			// Convert to short and save to buffer
			for (i = 0; i < frameSize * channels; i++, outputSize++)
			{
				outputData[outputSize] = (decodedData[i] > 0) ? (short)(decodedData[i] + .5d)
						: (short)(decodedData[i] - .5d);
			}
		}
	}
}
