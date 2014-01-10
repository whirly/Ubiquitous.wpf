// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the VORBISENCODER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// VORBISENCODER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef VORBISENCODER_EXPORTS
#define VORBISENCODER_API __declspec(dllexport)
#else
#define VORBISENCODER_API __declspec(dllimport)
#endif
 
// This class is exported from the VorbisEncoder.dll
VORBISENCODER_API void Initialize(int channels, int rate, float quality);
VORBISENCODER_API char*	GetHeader(int* headerLen);
VORBISENCODER_API char*	GetAudio(char* buffer, int bufferLen, int* audioLen);
VORBISENCODER_API void	End(void);

