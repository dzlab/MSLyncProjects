using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis;

namespace LyncPresence
{
    class AVCallHelper
    {
        
        internal AVCallHelper()
        {
            
        }
               
        public void AcceptAVCall(AudioVideoCall call)
        {
            //Console.WriteLine("Accepting incoming AV call");
            
            try
            {
                call.BeginAccept(
                ar =>
                {
                    try
                    {
                        call.Flow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(Flow_StateChanged);

                        call.EndAccept(ar);
                        SpeakMessage(call.Flow, string.Format("Hello, {0}. Thanks for calling. "
                            + "Your SIP URI is {1}",
                            call.RemoteEndpoint.Participant.DisplayName,
                            call.RemoteEndpoint.Participant.Uri));
                    }
                    catch (RealTimeException ex)
                    {
                        Console.WriteLine("Failed tp accept call.", ex);
                    }
                },
                null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Failed tp accept call.", ex);
            }            
        }

        public void SpeakMessage(AudioVideoFlow flow, string message)
        {
            try
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                SpeechAudioFormatInfo formatInfo = new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
                SpeechSynthesisConnector connector = new SpeechSynthesisConnector();

                synth.SetOutputToAudioStream(connector.Stream, formatInfo);

                connector.AttachFlow(flow);
                connector.Start();

                synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(
                    (sender, args) =>
                    {
                        connector.Stop();
                        synth.Dispose();
                    });

                synth.SpeakAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to play the message. {0}", ex);
            }

        }

        private void Flow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            try
            {
                AudioVideoFlow flow = sender as AudioVideoFlow;

                if (e.State == MediaFlowState.Terminated)
                {
                    flow.SpeechSynthesisConnector.DetachFlow();
                }

                flow.StateChanged -= Flow_StateChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ExtablishOutboundAVCall(ApplicationEndpoint appEndpoint, string destinationSipUri, string message)
        {
            
            try
            {
                Conversation conversation = new Conversation(appEndpoint);
                AudioVideoCall call = new AudioVideoCall(conversation);

                call.BeginEstablish(destinationSipUri,
                    new CallEstablishOptions(),
                    result =>
                    {
                        try
                        {
                            call.EndEstablish(result);
                            SpeakMessage(call.Flow, message);
/*                                string.Format("Hello, {0}. This my UCMA app calling you. " + "Your SIP URI is {1}",
                                call.RemoteEndpoint.Participant.DisplayName,
                                call.RemoteEndpoint.Participant.Uri));
 * */
                        }
                        catch (RealTimeException ex)
                        {
                            Console.WriteLine("Failed establishing AV call {0}", ex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Failed establishing AV call {0}", ex);
            }
        }
    }
}
