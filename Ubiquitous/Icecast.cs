using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using NAudio.Utils;
using NAudio.Wave;

namespace Ubiquitous
{
    // Signature for event
    public delegate void WaveDataMicHandler(object sender, WaveInEventArgs e);

    class Icecast
    {
        // Event available
        public event WaveDataMicHandler WaveDataMicEvent;

        // Capture data
        private BufferedWaveProvider buffer;
        private WaveIn capture;
        VorbisEncoder encoder = new VorbisEncoder(2, 44100, 0.6F);
        
        // Output data
        private WaveOut output;

        private Stream outputStream;

        private Stream fileStream;

        public Icecast( String server, String inPoint, String outPoint, String password )
        {
            outputStream = InitializeOutStream( server, outPoint, password );
            //fileStream = File.Open("machin.ogg", FileMode.Create);
            
            capture = InitializeCapture();
            output = InitializeOutput();
        }

        public void Stream( )
        {
            encoder.WriteHeader(outputStream);

            capture.StartRecording();
            output.Play();
        }

        private WaveIn InitializeCapture()
        {
            WaveIn capture = new WaveIn();
            capture.WaveFormat = new WaveFormat(44100, 2);
            capture.BufferMilliseconds = 1000;
            capture.DataAvailable += new EventHandler<WaveInEventArgs>(SendCaptureSample);

            return capture;
        }

        private WaveOut InitializeOutput()
        {
            WaveOut output = new WaveOut();
            buffer = new BufferedWaveProvider(capture.WaveFormat);

            output.Init(buffer);

            return output;
        }

        private Stream InitializeInStream(String server, String inPoint )
        {
            return null;
        }        

        private Stream InitializeOutStream( String server, String outPoint, String password )
        {           
            String outURL = "http://" + server + "/" + outPoint + ".ogg";
            HttpWebRequest client = (HttpWebRequest) WebRequest.Create( outURL );

            String encodedPassword = 
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes( "source:" + password));

            client.Method = "SOURCE";
            client.ContentType = "audio/mpeg";
            client.Headers.Add("Authorization", "Basic " + encodedPassword );
            client.Headers.Add("ice-name", "Ubiquitous Sound " + outPoint );
            client.Headers.Add("ice-url", outURL );
            client.Headers.Add("ice-genre", "Rock");
            client.Headers.Add("ice-description", "a simple application to dual stream.");
            client.Headers.Add("ice-audio-info", "ice-samplerate=44100;ice-bitrate=192;ice-channels=2");
            client.Headers.Add("ice-public", "1");
            client.Headers.Add("ice-private", "0");

            client.SendChunked = true;
            client.KeepAlive = true;

            Stream output = client.GetRequestStream();            
            return output;
        }

        private void SendCaptureSample( object sender, WaveInEventArgs e )
        {
            encoder.WriteAudio( outputStream, e.Buffer, e.BytesRecorded);
            buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

            WaveDataMicHandler handler = WaveDataMicEvent;

            if( handler != null )
            {
                handler(this, e);
            }
        }
    }
}
