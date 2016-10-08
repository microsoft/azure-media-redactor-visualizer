using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class FrameMetadata
    {
        public float Time { get; }
        public long TimeInTicks { get; }
        public float Duration { get; }
        public long DurationInTicks { get; }

        public FrameMetadata(JToken frame)
        {
            if(frame["pkt_pts_time"] != null)
            {
                Time = frame.Value<float>("pkt_pts_time");
            }
            else
            {
                Time = frame.Value<float>("pkt_dts_time");
            }

            if(frame["pkt_pts"] != null)
            {
                TimeInTicks = frame.Value<long>("pkt_pts");
            }
            else
            {
                TimeInTicks = frame.Value<long>("pkt_dts");
            }

            Duration = frame.Value<float>("pkt_duration_time");
            DurationInTicks = frame.Value<long>("pkt_duration");
        }
    }

    class VideoMetadata
    {
        public int Width { get; }
        public int Height { get; }
        public long TimeScale { get; }
        public float FrameRate { get; }
        public float Duration { get; }
        public long DurationInTicks { get; }
        public FrameMetadata[] Frames { get; }

        private float[] _frameTimes;

        public VideoMetadata(JToken jsonObj)
        {
            JToken videoStream = jsonObj
                            .Value<JArray>("streams")
                            .First(stream => stream["width"] != null && stream["height"] != null);

            Width = videoStream.Value<int>("width");
            Height = videoStream.Value<int>("height");
            TimeScale = GetTimeScale(videoStream.Value<string>("time_base"));
            FrameRate = GetFrameRate(videoStream.Value<string>("r_frame_rate"));
            Duration = videoStream.Value<float>("duration");
            DurationInTicks = videoStream.Value<long>("duration_ts");

            int stream_index = videoStream.Value<int>("index");
            Frames = jsonObj
                .Value<JArray>("frames")
                .Where(frame => frame.Value<int>("stream_index") == stream_index)
                .Select(frame => new FrameMetadata(frame))
                .ToArray();
            _frameTimes = Frames.Select(frame => frame.Time).ToArray();
        }

        public int FindFrameIndexByTime(float time)
        {
            int pos = Array.BinarySearch(_frameTimes, time);

            if (pos < 0)
            {
                pos = ~pos;
                if(pos == Frames.Length)
                {
                    return -1;
                }
            }

            return pos;
        }

        static long GetTimeScale(string time_base)
        {
            if (!time_base.StartsWith("1/"))
            {
                throw new ArgumentException("Time_base is not in regular format.");
            }

            return long.Parse(time_base.Substring(2));
        }

        static float GetFrameRate(string frame_rate)
        {
            int pos = frame_rate.IndexOf('/');
            if (pos == -1)
            {
                throw new ArgumentException("Frame_rate is not in regular format.");
            }

            int num = int.Parse(frame_rate.Substring(0, pos));
            int den = int.Parse(frame_rate.Substring(pos + 1));

            return 1.0f * num / den;
        }
    }
}