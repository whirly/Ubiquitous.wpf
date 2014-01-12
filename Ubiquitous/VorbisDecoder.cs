using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.IO;

namespace Ubiquitous
{
    class VorbisDecoder
    {
        [DllImport(@"VorbisDecoder.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Initialize();

        [DllImport(@"VorbisDecoder.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ProcessAudio( [In] byte[] bufferSrc, [In] int bufferSrcLen, [In,Out] ref int audioLen);

        [DllImport(@"VorbisDecoder.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void End();

        public VorbisDecoder()
        {
            Initialize();
        }

        public byte[] ReadAudio( Stream stream )
        {
            byte[] buffer = new byte[ 4096 ];
            int audioLen = 0;
            int sourceLen = 0;

            // Read 4K from the stream
            while( sourceLen < 4096 )
            {
                int bytes = stream.Read(buffer, sourceLen, 4096 - sourceLen );
                sourceLen += bytes;
            }

            IntPtr audio = ProcessAudio(buffer, sourceLen, ref audioLen);

            if (audioLen > 0)
            {
                byte[] bufferOut = new byte[audioLen]; 
                Marshal.Copy(audio, bufferOut, 0, audioLen);

                return bufferOut;
            }

            return null;
        }

        public void Close()
        {
            End();
        }
    }
}
