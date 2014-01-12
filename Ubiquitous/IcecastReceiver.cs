using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;

using NAudio.Utils;
using NAudio.Wave;

using System.Threading;

namespace Ubiquitous
{
    class IcecastReceiver
    {
        private Stream inputStream;
        private WaveOut output;

        private BufferedWaveProvider buffer;
        private VorbisDecoder decoder;

        Thread worker;

        public IcecastReceiver()
        {
            output = InitializeOutput();
        }

        public bool OpenStream(String server, String inPoint)
        {
            String outURL = "http://" + server + "/" + inPoint + ".ogg";
            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(outURL);

            try
            {
                HttpWebResponse response = (HttpWebResponse)client.GetResponse();
                inputStream = response.GetResponseStream();
                decoder = new VorbisDecoder();
                worker = new Thread(this.ProcessStream);
                
                output.Play();
                worker.Start();
            }
            catch( WebException e )
            {
                return false;
            }

            return true;
        }

        private void ProcessStream() 
        { 
            while( true )
            {
                byte[] audio = decoder.ReadAudio(inputStream);
                if (audio != null )
                {
                    buffer.AddSamples(audio, 0, audio.Length);                
                }
            }
        }

        private WaveOut InitializeOutput()
        {
            WaveOut output = new WaveOut();
            WaveFormat format = new WaveFormat();

            buffer = new BufferedWaveProvider( format );
            output.Init(buffer);

            return output;
        }
    }
}
