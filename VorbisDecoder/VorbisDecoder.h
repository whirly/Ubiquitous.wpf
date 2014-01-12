// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the VORBISDECODER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// VORBISDECODER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef VORBISDECODER_EXPORTS
#define VORBISDECODER_API __declspec(dllexport)
#else
#define VORBISDECODER_API __declspec(dllimport)
#endif


VORBISDECODER_API void	Initialize(void);
VORBISDECODER_API char* ProcessAudio( char* buffer, int bufferLen, int* audioLen );
VORBISDECODER_API void	End(void);

