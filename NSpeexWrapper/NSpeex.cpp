// This is the main DLL file.

#include "stdafx.h"

#include "NSpeex.h"

NSpeex::NativeEncoder::NativeEncoder(EncodingMode mode)
{
	bits = new SpeexBits();
	speex_bits_init(bits);
	enc_state = speex_encoder_init(ConvertToSpeexMode(mode));
}

NSpeex::NativeEncoder::~NativeEncoder()
{
	speex_bits_destroy(bits);
	speex_encoder_destroy(enc_state); 
}

int NSpeex::NativeEncoder::Encode(array<short>^ inData, array<Byte>^ outData)
{
	pin_ptr<short> speexIn = &inData[0];
	pin_ptr<Byte> speexOut = &outData[0];
	
	speex_bits_reset(bits);
	int encoded_len = 0;
	if (speex_encode_int(enc_state, speexIn, bits) != 0)
		encoded_len = speex_bits_write(bits, (char*)speexOut, outData->Length);

	return encoded_len;
}

NSpeex::NativeDecoder::NativeDecoder(EncodingMode mode)
{
	bits = new SpeexBits();
	speex_bits_init(bits);
	dec_state = speex_decoder_init(ConvertToSpeexMode(mode));
}

NSpeex::NativeDecoder::~NativeDecoder()
{
	speex_bits_destroy(bits);
	speex_decoder_destroy(dec_state); 
}

int NSpeex::NativeDecoder::Decode(array<Byte>^ inData, int count, array<short>^ outData)
{
	pin_ptr<short> speexOut = &outData[0];

	if (count == 0)
	{
		return speex_decode_int(dec_state, 0, speexOut);
	}
	else
	{
		pin_ptr<Byte> speexIn = &inData[0];
		speex_bits_reset(bits);
		speex_bits_read_from(bits, (char*)speexIn, count);
		return speex_decode_int(dec_state, bits, speexOut);
	}
}

NSpeex::NativePreprocessor::NativePreprocessor(int frameSize, int samplingRate)
{
	state = speex_preprocess_state_init(frameSize, samplingRate);
}

NSpeex::NativePreprocessor::~NativePreprocessor()
{
	speex_preprocess_state_destroy(state);
}

bool NSpeex::NativePreprocessor::Process(array<short>^ frame)
{
	pin_ptr<short> pFrame = &frame[0];
	int result = speex_preprocess_run(state, pFrame);

	return result != 0;
}

NSpeex::NativeJitterBuffer::NativeJitterBuffer(NSpeex::NativeDecoder^ decoder)
{
	this->decoder = decoder;
	jitter = jitter_buffer_init(1);
	bits = new SpeexBits();
	speex_bits_init(bits);
	currentPutTimestamp = 0;
}

NSpeex::NativeJitterBuffer::~NativeJitterBuffer()
{
	jitter_buffer_destroy(jitter);
	speex_bits_destroy(bits);
	if (outBuffer != 0)
		delete outBuffer;
}

void NSpeex::NativeJitterBuffer::Put(array<Byte>^ data)
{
	pin_ptr<Byte> pData = &data[0];

	JitterBufferPacket p;
	p.len = data->Length;
	p.sequence = 0;
	p.span = 1;
	p.timestamp = this->currentPutTimestamp++;
	p.data = (char*)pData;
	jitter_buffer_put(jitter, &p);
}

void NSpeex::NativeJitterBuffer::Get(array<short>^ decodedData)
{
	if (outBuffer == 0)
		outBuffer = new char[decodedData->Length];
	
	JitterBufferPacket p;
	p.len = decodedData->Length;
	p.data = outBuffer;

	memset(p.data, 0, p.len);

	int ret = jitter_buffer_get(jitter, &p, 1, 0);
	pin_ptr<short> pinDecodedData = &decodedData[0];
	if (ret != JITTER_BUFFER_OK)
	{
		// no packet found
		speex_decode_int(decoder->dec_state, 0, pinDecodedData);
	}
	else
	{
		speex_bits_read_from(bits, p.data, p.len);
		int ret = speex_decode_int(decoder->dec_state, bits, pinDecodedData);
		if (ret != 0)
		{
			// error while decoding
			//decodedData->Clear(decodedData, 0, decodedData->Length);
		}
	}

	jitter_buffer_tick(jitter);
}