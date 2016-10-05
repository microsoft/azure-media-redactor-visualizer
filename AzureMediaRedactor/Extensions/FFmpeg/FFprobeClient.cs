using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class FFprobeClient
    {
        private readonly string _ffprobeToolPath;
        private readonly string _videoUrl;
        private VideoMetadata _metadata;

        public VideoMetadata Metadata => _metadata;

        public FFprobeClient(string ffprobeToolPath, string videoUrl)
        {
            _ffprobeToolPath = ffprobeToolPath;
            _videoUrl = videoUrl;
        }

        public async Task<bool> RunAsync()
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _ffprobeToolPath,
                    Arguments = $"-i \"{_videoUrl}\" -show_frames -show_entries stream=width,height,time_base,r_frame_rate,duration,duration_ts:frame=pkt_pts,pkt_pts_time,pkt_dts,pkt_dts_time,pkt_duration,pkt_duration_time -print_format json",
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            if (!process.Start())
            {
                throw new Exception("Start FFMPEG client failed.");
            }

            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            process.Exited += (s, e) =>
            {
                source.TrySetResult(true);
            };

            HandleStandInput(process.StandardInput);
            HandleStandError(process.StandardError);
            HandleStandOutput(process.StandardOutput);

            await source.Task;

            return process.ExitCode == 0;
        }

        async void HandleStandInput(StreamWriter writer)
        {
            await Task.Yield();
            /*Do nothing*/
        }

        async void HandleStandError(StreamReader reader)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                Debug.WriteLine(line);
            }
        }

        void HandleStandOutput(StreamReader reader)
        {
            JObject jsonObj;
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                jsonObj = JObject.Load(jsonReader);
                _metadata = new VideoMetadata(jsonObj);
            }           
        }
    }
}
