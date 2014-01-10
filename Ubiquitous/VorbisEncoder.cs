using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ubiquitous
{
    class VorbisEncoder
    {
        [DllImport(@"VorbisEncoder.dll", CallingConvention=CallingConvention.Cdecl) ]
        private static extern void Initialize( [In] int channels, [In] int rate, [In] float quality );

        [DllImport(@"VorbisEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetHeader([In, Out] ref int headerLen);

        [DllImport(@"VorbisEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetAudio([In] byte[] buffer, [In] int bufferLen, [In, Out] ref int audioLen);

        [DllImport(@"VorbisEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void	End();


        public VorbisEncoder( int channels, int rate, float quality )
        {
            Initialize(channels, rate, quality);
        }

        public void WriteHeader( Stream stream )
        {
            int     headerLen = 0;
            IntPtr  header;
            byte[]  buffer;

            header = GetHeader( ref headerLen);

            buffer = new byte[ headerLen ];
            Marshal.Copy(header, buffer, 0, headerLen);

            stream.Write(buffer, 0, headerLen);
        }

        public void WriteAudio( Stream stream, byte[] buffer, int bufferLen )
        {
            int audioLen = 0;
            IntPtr audio;
            byte[] processedBuffer;

            audio = GetAudio( buffer, bufferLen, ref audioLen );

            processedBuffer = new byte[audioLen];
            Marshal.Copy(audio, processedBuffer, 0, audioLen );

            stream.Write(processedBuffer, 0, audioLen);
        }

        public void Close()
        {
        }
    }
}
