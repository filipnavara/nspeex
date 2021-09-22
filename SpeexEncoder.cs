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
	/// Main Speex Encoder class. This class encodes the given PCM 16bit samples into
	/// Speex packets.
	/// </summary>
	public class SpeexEncoder
	{
		/// <summary>
		/// Version of the Speex Encoder
		/// </summary>
		public const String Version = "Java Speex Encoder v0.9.7 ($Revision: 1.6 $)";

		private IEncoder encoder;
		private Bits bits, cbits;
		private float[] rawData;
		private int sampleRate;
		private int channels;
		private int frameSize;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mode">the mode of the encoder (0=NB, 1=WB, 2=UWB).</param>
		/// <param name="quality">the quality setting of the encoder (between 0 and 10).</param>
		/// <param name="sampleRate_0">the number of samples per second.</param>
		/// <param name="channels_1">the number of audio channels (1=mono, 2=stereo, ...).</param>
		/// <returns>true if initialisation successful.</returns>
		public SpeexEncoder(BandMode mode, int quality, int sampleRate_0, int channels_1)
		{
			bits = new Bits();
			cbits = new Bits();

			switch (mode)
			{
				case BandMode.Narrow:
					encoder = new NbEncoder();
					break;
				case BandMode.Wide:
					encoder = new SbEncoder(false);
					break;
				case BandMode.UltraWide:
					encoder = new SbEncoder(true);
					break;
				default:
					throw new System.ArgumentException("Invalid mode", "mode");
			}

			/* initialize the speex decoder */
			encoder.Quality = quality;

			/* set decoder format and properties */
			this.frameSize = encoder.FrameSize;
			this.sampleRate = sampleRate_0;
			this.channels = channels_1;
			rawData = new float[channels_1 * frameSize];
		}

		public IEncoder Encoder
		{
			get
			{
				return encoder;
			}
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

		public int FrameSize
		{
			get
			{
				return frameSize;
			}
		}

		/// <summary>
		/// Pull the decoded data out into a byte array at the given offset and
		/// returns the number of bytes of encoded data just read.
		/// </summary>
		/// <returns>the number of bytes of encoded data just read.</returns>
		public int GetProcessedData(byte[] data, int offset)
		{
			int size = bits.BufferSize;
			System.Array.Copy(bits.Buffer, 0, data, offset, size);
			bits = new Bits();
			cbits = new Bits();
			return size;
		}

		public int ProcessedDataByteSize
		{
			get
			{
				return bits.BufferSize;
			}
		}

		/// <summary>
		/// This is where the actual encoding takes place
		/// </summary>
		/// <returns>true if successful.</returns>
		public bool ProcessData(byte[] data, int offset, int len)
		{
			// converty raw bytes into float samples
			MapPcm16bitLittleEndian2Float(data, offset, rawData, 0, len / 2);
			// encode the bitstream
			return ProcessData(rawData, len / 2);
		}

		/// <summary>
		/// Encode an array of shorts.
		/// </summary>
		/// <returns>true if successful.</returns>
		public bool ProcessData(short[] data, int offset, int numShorts)
		{
			int numSamplesRequired = channels * frameSize;
			if (numShorts != numSamplesRequired)
			{
				throw new ArgumentException("SpeexEncoder requires "
						+ numSamplesRequired + " samples to process a Frame, not "
						+ numShorts);
			}
			// convert shorts into float samples,
			for (int i = 0; i < numShorts; i++)
				rawData[i] = (float)data[offset + i];
			// encode the bitstream
			return ProcessData(rawData, numShorts);
		}

		/// <summary>
		/// Encode an array of floats.
		/// </summary>
		/// <returns>true if successful.</returns>
		public bool ProcessData(float[] data, int numSamples)
		{
			int numSamplesRequired = channels * frameSize;
			if (numSamples != numSamplesRequired)
			{
				throw new ArgumentException("SpeexEncoder requires "
						+ numSamplesRequired + " samples to process a Frame, not "
						+ numSamples);
			}
			// encode the bitstream
			if (channels == 2)
			{
//				NSpeex.Stereo.Encode(cbits, data, frameSize);
				NSpeex.Stereo.Encode(bits, data, frameSize);
			}
//			cencoder.encode(cbits, data);
			encoder.Encode(bits, data);
			/*if (bits.BufferSize != cbits.BufferSize)
				return false;
			for (int i = 0; i < bits.BufferSize; i++)
				if (bits.Buffer[i] != cbits.Buffer[i])
					return false;*/
			return true;
		}

		/// <summary>
		/// Converts a 16 bit linear PCM stream (in the form of a byte array) into a
		/// floating point PCM stream (in the form of an float array). Here are some
		/// important details about the encoding:
		/// 
		/// - Java uses big endian for shorts and ints, and Windows uses little
		///   Endian. Therefore, shorts and ints must be read as sequences of bytes and
		///   combined with shifting operations.
		/// </summary>
		private static void MapPcm16bitLittleEndian2Float(
			byte[] pcm16bitBytes, int offsetInput, float[] samples, int offsetOutput, int length)
		{
			if (pcm16bitBytes.Length - offsetInput < 2 * length)
				throw new ArgumentException("Insufficient Samples to convert to floats");
			if (samples.Length - offsetOutput < length)
				throw new ArgumentException("Insufficient float buffer to convert the samples");
			for (int i = 0; i < length; i++)
			{
				samples[offsetOutput + i] = (float)(short)((pcm16bitBytes[offsetInput + 2
						* i] & 0xff) | (pcm16bitBytes[offsetInput + 2 * i + 1] << 8));
				// no & 0xff at the end to keep the sign
			}
		}
	}
}
