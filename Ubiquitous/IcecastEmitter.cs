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

    class IcecastEmitter
    {
        // Event available
        public event WaveDataMicHandler WaveDataMicEvent;

        // Capture data
        private WaveIn capture;
        private VorbisEncoder encoder; 
        
        // Output data
        private Stream outputStream;

        public IcecastEmitter()
        {
        }

        public bool OpenStream( String server, String outPoint, String password )
        {
            // Create the encoder and initialize the capture of the mic
            encoder = new VorbisEncoder(2, 44100, 0.6F);
            capture = InitializeCapture();

            // Open the stream to the server
            String outURL = "http://" + server + "/" + outPoint + ".ogg";
            HttpWebRequest client = (HttpWebRequest) WebRequest.Create( outURL );

            String encodedPassword = 
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes( "source:" + password));

            client.Method = "SOURCE";
            client.ContentType = "audio/ogg";
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

            outputStream = client.GetRequestStream();

            // We start to write the header on the server
            encoder.WriteHeader(outputStream);
            capture.StartRecording();

            return true;
        }

        private void SendCaptureSample( object sender, WaveInEventArgs e )
        {
            encoder.WriteAudio(outputStream, e.Buffer, e.BytesRecorded);
            WaveDataMicHandler handler = WaveDataMicEvent;

            if( handler != null )
            {
                handler(this, e);
            }
        }
        private WaveIn InitializeCapture()
        {
            WaveIn capture = new WaveIn();
            capture.WaveFormat = new WaveFormat(44100, 2);
            capture.BufferMilliseconds = 1000;
            capture.DataAvailable += new EventHandler<WaveInEventArgs>(SendCaptureSample);

            return capture;
        }


    }
}
