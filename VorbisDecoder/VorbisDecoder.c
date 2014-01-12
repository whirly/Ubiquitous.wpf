// VorbisDecoder.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "VorbisDecoder.h"

ogg_sync_state   oy; /* sync and verify incoming physical bitstream */
ogg_stream_state os; /* take physical pages, weld into a logical
					 stream of packets */
ogg_page         og; /* one Ogg bitstream page. Vorbis packets are inside */
ogg_packet       op; /* one raw packet of data for decode */

vorbis_info      vi; /* struct that stores all the static vorbis bitstream
					 settings */
vorbis_comment   vc; /* struct that stores all the bitstream user comments */
vorbis_dsp_state vd; /* central working state for the packet->PCM decoder */
vorbis_block     vb; /* local working space for packet->PCM decode */

char*			buffer;
int				state;
int				headerToRead;
int				convsize;

ogg_int16_t convbuffer[4096];
char complete_buffer[4096*64];

int				result;

void TryToReadHeader(void)
{
	while (headerToRead > 0)
	{
		result = ogg_sync_pageout(&oy, &og);
		if (result == 0) return;

		ogg_stream_pagein(&os, &og);
		result = ogg_stream_packetout(&os, &op);

		if (result == 0) return;
		vorbis_synthesis_headerin(&vi, &vc, &op);

		headerToRead--;
	}
}

VORBISDECODER_API void	Initialize(void)
{
	ogg_sync_init(&oy);
	state = 0;
}

VORBISDECODER_API char* ProcessAudio(char* bufferSrc, int bufferSrcLen, int* audioLen)
{
	switch (state) 
	{
	case 0:
		buffer = ogg_sync_buffer(&oy, 4096);
		memcpy(buffer, bufferSrc, bufferSrcLen);
		ogg_sync_wrote(&oy, bufferSrcLen);

		if (ogg_sync_pageout(&oy, &og) != 1)
		{
			return 0;
		}

		ogg_stream_init(&os, ogg_page_serialno(&og));

		vorbis_info_init(&vi);
		vorbis_comment_init(&vc);

		ogg_stream_pagein(&os, &og);
		ogg_stream_packetout(&os, &op);

		vorbis_synthesis_headerin(&vi, &vc, &op);
		state += 1;
		headerToRead = 2;

		TryToReadHeader();
		return 0;

	case 1:
		buffer = ogg_sync_buffer(&oy, 4096);
		memcpy(buffer, bufferSrc, bufferSrcLen);
		ogg_sync_wrote(&oy, bufferSrcLen);

		TryToReadHeader();

		if (headerToRead == 0) {
			state += 1;

			convsize = 4096 / vi.channels;

			if (vorbis_synthesis_init(&vd, &vi) == 0)
			{
				vorbis_block_init(&vd, &vb);
			}
		}
		return 0;

	case 2:
		*audioLen = 0;
		while ( 1 )
		{
			result = ogg_sync_pageout(&oy, &og);
			if (result > 0)
			{
				ogg_stream_pagein(&os, &og);
				while (1) {
					result = ogg_stream_packetout(&os, &op);
					if (result == 0) break;

					float **pcm;
					int samples;

					if (vorbis_synthesis(&vb, &op) == 0)
					{
						vorbis_synthesis_blockin(&vd, &vb);
					}

					while ((samples = vorbis_synthesis_pcmout(&vd, &pcm)) > 0) {
						int j;
						int clipflag;
						int bout = (samples < convsize ? samples : convsize);

						for (int i = 0; i < vi.channels; i++){
							ogg_int16_t *ptr = convbuffer + i;
							float  *mono = pcm[i];
							for (j = 0; j<bout; j++)
							{
								int val = (int)floor(mono[j] * 32767.f + .5f);

								/* might as well guard against clipping */
								if (val>32767){
									val = 32767;
									clipflag = 1;
								}

								if (val < -32768){
									val = -32768;
									clipflag = 1;
								}

								*ptr = val;
								ptr += vi.channels;
							}
						}
						memcpy( complete_buffer + *audioLen, convbuffer, bout * vi.channels * 2);
						*audioLen += bout * vi.channels * 2;

						vorbis_synthesis_read(&vd, bout);
					}
				}
			}
			else
			{
				buffer = ogg_sync_buffer(&oy, 4096);
				memcpy(buffer, bufferSrc, bufferSrcLen);
				ogg_sync_wrote(&oy, bufferSrcLen);

				if (*audioLen > 0) {
					return (char*)complete_buffer;
				}
				else
				{
					return 0;
				}
			}
		}
	}

	return 0;
}

VORBISDECODER_API void	End(void)
{

}