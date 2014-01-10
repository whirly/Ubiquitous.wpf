// VorbisEncoder.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "MyVorbisEncoder.h"

// States
ogg_stream_state os; /* take physical pages, weld into a logical
					 stream of packets */
ogg_page         og; /* one Ogg bitstream page.  Vorbis packets are inside */
ogg_packet       op; /* one raw packet of data for decode */

vorbis_info      vi; /* struct that stores all the static vorbis bitstream
					 settings */
vorbis_comment   vc; /* struct that stores all the user comments */

vorbis_dsp_state vd; /* central working state for the packet->PCM decoder */
vorbis_block     vb; /* local working space for packet->PCM decode */

char*			writeBuffer;

// This is the constructor of a class that has been exported.
// see VorbisEncoder.h for the class definition
VORBISENCODER_API void Initialize(int channels, int rate, float quality)
{
	int returnCode;

	// Encode setup
	vorbis_info_init(&vi);
	returnCode = vorbis_encode_init_vbr(&vi, channels, rate, quality);

	vorbis_analysis_init(&vd, &vi);
	vorbis_block_init(&vd, &vb);

	srand((unsigned int)time(NULL));
	ogg_stream_init(&os, rand());

	// Ok this one is really... well...
	writeBuffer = (char*)malloc(2 * 1024 * 1024);

	return;
}

VORBISENCODER_API char* GetHeader(int* headerLen)
{
	long	writeOffset = 0;

	ogg_packet header;
	ogg_packet header_comm;
	ogg_packet header_code;

	vorbis_analysis_headerout(&vd, &vc, &header, &header_comm, &header_code);

	ogg_stream_packetin(&os, &header);
	ogg_stream_packetin(&os, &header_comm); 
	ogg_stream_packetin(&os, &header_code);

	while (ogg_stream_flush(&os, &og))
	{
		memcpy(&writeBuffer[writeOffset], og.header, og.header_len);
		writeOffset += og.header_len;

		memcpy(&writeBuffer[writeOffset], og.body, og.body_len);
		writeOffset += og.body_len;
	}

	*headerLen = writeOffset;
	return writeBuffer;
}

VORBISENCODER_API char* GetAudio(char* buffer, int bufferLen, int* audioLen)
{
	long writeOffset = 0;

	float** bufferProcessing = vorbis_analysis_buffer(&vd, bufferLen / 4);
	for (int i = 0; i < bufferLen / 4; i++)
	{
		bufferProcessing[0][i] = ((buffer[i * 4 + 1] << 8) | (0x00ff & (int)buffer[i * 4])) / 32768.f;
		bufferProcessing[1][i] = ((buffer[i * 4 + 3] << 8) | (0x00ff & (int)buffer[i * 4 + 2])) / 32768.f;
	}

	vorbis_analysis_wrote(&vd, bufferLen / 4 );

	while (vorbis_analysis_blockout(&vd, &vb) == 1 )
	{
		vorbis_analysis(&vb, NULL);
		vorbis_bitrate_addblock(&vb);

		while (vorbis_bitrate_flushpacket(&vd, &op))
		{
			ogg_stream_packetin(&os, &op);

			while (ogg_stream_pageout(&os, &og)) {
				memcpy(&writeBuffer[writeOffset], og.header, og.header_len);
				writeOffset += og.header_len;

				memcpy(&writeBuffer[writeOffset], og.body, og.body_len);
				writeOffset += og.body_len;
			}
		}
	}

	*audioLen = writeOffset;
	return writeBuffer;
} 

VORBISENCODER_API void End(void)
{
	ogg_stream_clear(&os);
	vorbis_block_clear(&vb);
	vorbis_dsp_clear(&vd);
	vorbis_comment_clear(&vc);
	vorbis_info_clear(&vi);

	free(writeBuffer);
}