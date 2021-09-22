using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NSpeex;

namespace NSpeex.Test
{
	class Program
	{
		/** Version of the Speex Encoder */
		public static String VERSION = "Java Speex Command Line Encoder v0.9.7 ($Revision: 1.5 $)";
		/** Copyright display String */
		public static String COPYRIGHT = "Copyright (C) 2002-2004 Wimba S.A.";

		/** Print level for messages : Print debug information */
		public static int DEBUG = 0;
		/** Print level for messages : Print basic information */
		public static int INFO = 1;
		/** Print level for messages : Print only warnings and errors */
		public static int WARN = 2;
		/** Print level for messages : Print only errors */
		public static int ERROR = 3;
		/** Print level for messages */
		protected int printlevel = DEBUG;

		/** File format for input or output audio file: Raw */
		public static int FILE_FORMAT_RAW = 0;
		/** File format for input or output audio file: Ogg */
		public static int FILE_FORMAT_OGG = 1;
		/** File format for input or output audio file: Wave */
		public static int FILE_FORMAT_WAVE = 2;
		/** Defines File format for input audio file (Raw, Ogg or Wave). */
		protected int srcFormat = FILE_FORMAT_OGG;
		/** Defines File format for output audio file (Raw or Wave). */
		protected int destFormat = FILE_FORMAT_WAVE;

		/** Defines the encoder mode (0=NB, 1=WB and 2=UWB). */
		protected int mode = -1;
		/** Defines the encoder quality setting (integer from 0 to 10). */
		protected int quality = 8;
		/** Defines the encoders algorithmic complexity. */
		protected int complexity = 3;
		/** Defines the number of frames per speex packet. */
		protected int nframes = 1;
		/** Defines the desired bitrate for the encoded audio. */
		protected int bitrate = -1;
		/** Defines the sampling rate of the audio input. */
		protected int sampleRate = -1;
		/** Defines the number of channels of the audio input (1=mono, 2=stereo). */
		protected int channels = 1;
		/** Defines the encoder VBR quality setting (float from 0 to 10). */
		protected float vbr_quality = -1;
		/** Defines whether or not to use VBR (Variable Bit Rate). */
		protected bool vbr = false;
		/** Defines whether or not to use VAD (Voice Activity Detection). */
		protected bool vad = false;
		/** Defines whether or not to use DTX (Discontinuous Transmission). */
		protected bool dtx = false;

		/** The audio input file */
		protected String srcFile;
		/** The audio output file */
		protected String destFile;

		static void Main(string[] args)
		{
			Program encoder = new Program();
			if (encoder.parseArgs(args))
			{
				encoder.encode();
			}

			/*
			JSpeexEnc enc = new JSpeexEnc();
			enc.srcFile = filename + ".wav";
			enc.destFile = filename + ".spx";
			enc.srcFormat = JSpeexEnc.FILE_FORMAT_WAVE;
			enc.destFormat = JSpeexEnc.FILE_FORMAT_OGG;
			enc.printlevel = JSpeexEnc.ERROR;
			enc.mode = mode; // Narrowband
			//enc.vbr_quality = 8f; // default 8
			//enc.quality = 8;      // default 8
			//enc.complexity = 3;   // default 3
			//enc.nframes = 1;      // default 1
			enc.vbr = vbr;        // default false
			//enc.vad = false;      // default false
			//enc.dtx = false;      // default false
			enc.sampleRate = sampleRate;
			enc.channels = channels;
			return enc;
			//NSpeex.*/
		}

		/**
		 * Parse the command line arguments.
		 * @param args Command line parameters.
		 * @return true if the parsed arguments are sufficient to run the encoder.
		 */
		public bool parseArgs(String[] args)
		{
			// make sure we have command args
			if (args.Length < 2)
			{
				if (args.Length == 1 && (args[0].Equals("-v", StringComparison.OrdinalIgnoreCase) || args[0].Equals("--version", StringComparison.OrdinalIgnoreCase)))
				{
					version();
					return false;
				}
				usage();
				return false;
			}
			// Determine input, output and file formats
			srcFile = args[args.Length - 2];
			destFile = args[args.Length - 1];
			if (srcFile.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
			{
				srcFormat = FILE_FORMAT_WAVE;
			}
			else
			{
				srcFormat = FILE_FORMAT_RAW;
			}
			if (destFile.EndsWith(".spx", StringComparison.OrdinalIgnoreCase))
			{
				destFormat = FILE_FORMAT_OGG;
			}
			else if (destFile.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
			{
				destFormat = FILE_FORMAT_WAVE;
			}
			else
			{
				destFormat = FILE_FORMAT_RAW;
			}
			// Determine encoder options
			/*
		  for (int i=0; i<args.Length-2; i++) {
			if (args[i].equalsIgnoreCase("-h") || args[i].equalsIgnoreCase("--help")) {
			  usage();
			  return false;
			}
			else if (args[i].equalsIgnoreCase("-v") || args[i].equalsIgnoreCase("--version")) {
			  version();
			  return false;
			}
			else if (args[i].equalsIgnoreCase("--verbose")) {
			  printlevel = DEBUG;
			}
			else if (args[i].equalsIgnoreCase("--quiet")) {
			  printlevel = WARN;
			}
			else if (args[i].equalsIgnoreCase("-n") || 
					 args[i].equalsIgnoreCase("-nb") ||
					 args[i].equalsIgnoreCase("--narrowband")) {
			  mode = 0;
			}
			else if (args[i].equalsIgnoreCase("-w") ||
					 args[i].equalsIgnoreCase("-wb") ||
					 args[i].equalsIgnoreCase("--wideband")) {
			  mode = 1;
			}
			else if (args[i].equalsIgnoreCase("-u") ||
					 args[i].equalsIgnoreCase("-uwb") ||
					 args[i].equalsIgnoreCase("--ultra-wideband")) {
			  mode = 2;
			}
			else if (args[i].equalsIgnoreCase("-q") || args[i].equalsIgnoreCase("--quality")) {
			  try {
				vbr_quality = Float.parseFloat(args[++i]);
				quality = (int) vbr_quality;
			  }
			  catch (NumberFormatException e) {
				usage();
				return false;
			  }
			}
			else if (args[i].equalsIgnoreCase("--complexity")) {
			  try {
				complexity = Integer.parseInt(args[++i]);
			  }
			  catch (NumberFormatException e) {
				usage();
				return false;
			  }
			}
			else if (args[i].equalsIgnoreCase("--nframes")) {
			  try {
				nframes = Integer.parseInt(args[++i]);
			  }
			  catch (NumberFormatException e) {
				usage();
				return false;
			  }
			}
			else if (args[i].equalsIgnoreCase("--vbr")) {
			  vbr = true;
			}
			else if (args[i].equalsIgnoreCase("--vad")) {
			  vad = true;
			}
			else if (args[i].equalsIgnoreCase("--dtx")) {
			  dtx = true;
			}
			else if (args[i].equalsIgnoreCase("--rate")) {
			  try {
				sampleRate = Integer.parseInt(args[++i]);
			  }
			  catch (NumberFormatException e) {
				usage();
				return false;
			  }
			}
			else if (args[i].equalsIgnoreCase("--stereo")) {
			  channels = 2;
			}
			else {
			  usage();
			  return false;
			}
		  }
			 */
			return true;
		}

		/**
		 * Prints the usage guidelines.
		 */
		public static void usage()
		{
			version();
			Console.Out.WriteLine("");
			Console.Out.WriteLine("Usage: JSpeexEnc [options] input_file output_file");
			Console.Out.WriteLine("Where:");
			Console.Out.WriteLine("  input_file can be:");
			Console.Out.WriteLine("    filename.wav  a PCM wav file");
			Console.Out.WriteLine("    filename.*    a raw PCM file (any extension other than .wav)");
			Console.Out.WriteLine("  output_file can be:");
			Console.Out.WriteLine("    filename.spx  an Ogg Speex file");
			Console.Out.WriteLine("    filename.wav  a Wave Speex file (beta!!!)");
			Console.Out.WriteLine("    filename.*    a raw Speex file");
			Console.Out.WriteLine("Options: -h, --help     This help");
			Console.Out.WriteLine("         -v, --version  Version information");
			Console.Out.WriteLine("         --verbose      Print detailed information");
			Console.Out.WriteLine("         --quiet        Print minimal information");
			Console.Out.WriteLine("         -n, -nb        Consider input as Narrowband (8kHz)");
			Console.Out.WriteLine("         -w, -wb        Consider input as Wideband (16kHz)");
			Console.Out.WriteLine("         -u, -uwb       Consider input as Ultra-Wideband (32kHz)");
			Console.Out.WriteLine("         --quality n    Encoding quality (0-10) default 8");
			Console.Out.WriteLine("         --complexity n Encoding complexity (0-10) default 3");
			Console.Out.WriteLine("         --nframes n    Number of frames per Ogg packet, default 1");
			Console.Out.WriteLine("         --vbr          Enable varible bit-rate (VBR)");
			Console.Out.WriteLine("         --vad          Enable voice activity detection (VAD)");
			Console.Out.WriteLine("         --dtx          Enable file based discontinuous transmission (DTX)");
			Console.Out.WriteLine("         if the input file is raw PCM (not a Wave file)");
			Console.Out.WriteLine("         --rate n       Sampling rate for raw input");
			Console.Out.WriteLine("         --stereo       Consider input as stereo");
			Console.Out.WriteLine("More information is available from: http://jspeex.sourceforge.net/");
			Console.Out.WriteLine("This code is a Java port of the Speex codec: http://www.speex.org/");
		}

		/**
		 * Prints the version.
		 */
		public static void version()
		{
			Console.Out.WriteLine(VERSION);
			Console.Out.WriteLine("using " + SpeexEncoder.Version);
			Console.Out.WriteLine(COPYRIGHT);
		}

		/**
		 * Encodes a PCM file to Speex. 
		 * @exception IOException
		 */
		public void encode()
		{
			encode(srcFile, destFile);
		}

		/**
   * Encodes a PCM file to Speex. 
   * @param srcPath
   * @param destPath
   * @exception IOException
   */
		public void encode(string srcPath, string destPath)
		{
			byte[] temp = new byte[2560]; // stereo UWB requires one to read 2560b
			const int HEADERSIZE = 8;
			const String RIFF = "RIFF";
			const String WAVE = "WAVE";
			const String FORMAT = "fmt ";
			const String DATA = "data";
			const int WAVE_FORMAT_PCM = 0x0001;
			// Display info
			if (printlevel <= INFO) version();
			if (printlevel <= DEBUG) Console.Out.WriteLine("");
			if (printlevel <= DEBUG) Console.Out.WriteLine("Input File: " + srcPath);
			// Open the input stream
			FileStream dis = new FileStream(srcPath, FileMode.Open);
			// Prepare input stream
			if (srcFormat == FILE_FORMAT_WAVE)
			{
				// read the WAVE header
				dis.Read(temp, 0, HEADERSIZE + 4);
				// make sure its a WAVE header
				if (!RIFF.Equals(System.Text.Encoding.ASCII.GetString(temp, 0, 4)) &&
					!WAVE.Equals(System.Text.Encoding.ASCII.GetString(temp, 8, 4)))
				{
					Console.Error.WriteLine("Not a WAVE file");
					return;
				}
				// Read other header chunks
				dis.Read(temp, 0, HEADERSIZE);
				String chunk = System.Text.Encoding.ASCII.GetString(temp, 0, 4);
				int size = readInt(temp, 4);
				while (!chunk.Equals(DATA))
				{
					dis.Read(temp, 0, size);
					if (chunk.Equals(FORMAT))
					{
						/*
						typedef struct waveformat_extended_tag {
						WORD wFormatTag; // format type
						WORD nChannels; // number of channels (i.e. mono, stereo...)
						DWORD nSamplesPerSec; // sample rate
						DWORD nAvgBytesPerSec; // for buffer estimation
						WORD nBlockAlign; // block size of data
						WORD wBitsPerSample; // Number of bits per sample of mono data
						WORD cbSize; // The count in bytes of the extra size 
						} WAVEFORMATEX;
						*/
						if (readShort(temp, 0) != WAVE_FORMAT_PCM)
						{
							Console.Error.WriteLine("Not a PCM file");
							return;
						}
						channels = readShort(temp, 2);
						sampleRate = readInt(temp, 4);
						if (readShort(temp, 14) != 16)
						{
							Console.Error.WriteLine("Not a 16 bit file " + readShort(temp, 18));
							return;
						}
						// Display audio info
						if (printlevel <= DEBUG)
						{
							Console.Out.WriteLine("File Format: PCM wave");
							Console.Out.WriteLine("Sample Rate: " + sampleRate);
							Console.Out.WriteLine("Channels: " + channels);
						}
					}
					dis.Read(temp, 0, HEADERSIZE);
					chunk = System.Text.Encoding.ASCII.GetString(temp, 0, 4);
					size = readInt(temp, 4);
				}
				if (printlevel <= DEBUG) Console.Out.WriteLine("Data size: " + size);
			}
			else
			{
				if (sampleRate < 0)
				{
					switch (mode)
					{
						case 0:
							sampleRate = 8000;
							break;
						case 1:
							sampleRate = 16000;
							break;
						case 2:
							sampleRate = 32000;
							break;
						default:
							sampleRate = 8000;
							break;
					}
				}
				// Display audio info
				if (printlevel <= DEBUG)
				{
					Console.Out.WriteLine("File format: Raw audio");
					Console.Out.WriteLine("Sample rate: " + sampleRate);
					Console.Out.WriteLine("Channels: " + channels);
					Console.Out.WriteLine("Data size: " + srcPath.Length);
				}
			}

			// Set the mode if it has not yet been determined
			if (mode < 0)
			{
				if (sampleRate < 100) // Sample Rate has probably been given in kHz
					sampleRate *= 1000;
				if (sampleRate < 12000)
					mode = (int)BandMode.Narrow; // Narrowband
				else if (sampleRate < 24000)
					mode = (int)BandMode.Wide; // Wideband
				else
					mode = (int)BandMode.UltraWide; // Ultra-wideband
			}
			// Construct a new encoder
			SpeexEncoder speexEncoder = new SpeexEncoder((BandMode)mode, quality, sampleRate, channels);
			/*if (complexity > 0)
			{
				speexEncoder.Encoder.Complexity = complexity;
			}
			if (bitrate > 0)
			{
				speexEncoder.Encoder.BitRate = bitrate;
			}
			if (vbr)
			{
				speexEncoder.Encoder.Vbr = vbr;
				if (vbr_quality > 0)
				{
					speexEncoder.Encoder.VbrQuality = vbr_quality;
				}
			}
			if (vad)
			{
				speexEncoder.Encoder.Vad = vad;
			}
			if (dtx)
			{
				speexEncoder.Encoder.Dtx = dtx;
			}*/

			// Display info
			if (printlevel <= DEBUG)
			{
				Console.Out.WriteLine("");
				Console.Out.WriteLine("Output File: " + destPath);
				Console.Out.WriteLine("File format: Ogg Speex");
				Console.Out.WriteLine("Encoder mode: " + (mode == 0 ? "Narrowband" : (mode == 1 ? "Wideband" : "UltraWideband")));
				Console.Out.WriteLine("Quality: " + (vbr ? vbr_quality : quality));
				Console.Out.WriteLine("Complexity: " + complexity);
				Console.Out.WriteLine("Frames per packet: " + nframes);
				Console.Out.WriteLine("Varible bitrate: " + vbr);
				Console.Out.WriteLine("Voice activity detection: " + vad);
				Console.Out.WriteLine("Discontinouous Transmission: " + dtx);
			}
			// Open the file writer
			AudioFileWriter writer;
			if (destFormat == FILE_FORMAT_OGG)
			{
				writer = new OggSpeexWriter(mode, sampleRate, channels, nframes, vbr);
			}
			else if (destFormat == FILE_FORMAT_WAVE)
			{
				nframes = PcmWaveWriter.WAVE_FRAME_SIZES[mode - 1, channels - 1, quality];
				writer = new PcmWaveWriter(mode, quality, sampleRate, channels, nframes, vbr);
			}
			else
			{
				writer = new RawWriter();
			}
			writer.Open(destPath);
			writer.WriteHeader("Encoded with: " + VERSION);
			int pcmPacketSize = 2 * channels * speexEncoder.FrameSize;
			//try
			{
				// read until we get to EOF
				while (dis.Position < dis.Length)
				{
					dis.Read(temp, 0, nframes * pcmPacketSize);
					for (int i = 0; i < nframes; i++)
						speexEncoder.ProcessData(temp, i * pcmPacketSize, pcmPacketSize);
					int encsize = speexEncoder.GetProcessedData(temp, 0);
					if (encsize > 0)
					{
						writer.WritePacket(temp, 0, encsize);
					}
				}
			}
			//catch (EOFException e) { }
			writer.Close();
			dis.Close();
		}
		/**
		 * Converts Little Endian (Windows) bytes to an int (Java uses Big Endian).
		 * @param data the data to read.
		 * @param offset the offset from which to start reading.
		 * @return the integer value of the reassembled bytes.
		 */
		protected static int readInt(byte[] data, int offset)
		{
			return (data[offset] & 0xff) |
				   ((data[offset + 1] & 0xff) << 8) |
				   ((data[offset + 2] & 0xff) << 16) |
				   (data[offset + 3] << 24); // no 0xff on the last one to keep the sign
		}

		/**
		 * Converts Little Endian (Windows) bytes to an short (Java uses Big Endian).
		 * @param data the data to read.
		 * @param offset the offset from which to start reading.
		 * @return the integer value of the reassembled bytes.
		 */
		protected static int readShort(byte[] data, int offset)
		{
			return (data[offset] & 0xff) |
				   (data[offset + 1] << 8); // no 0xff on the last one to keep the sign
		}
	}
}
