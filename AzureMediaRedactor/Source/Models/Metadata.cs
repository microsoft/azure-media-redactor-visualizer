using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzureMediaRedactor.Models
{
    class Metadata
    {
        public static Metadata Parse(string fileUrl)
        {
            JObject obj;
            using (StreamReader sr = new StreamReader(fileUrl))
            {
                using (JsonTextReader jtr = new JsonTextReader(sr))
                {
                    obj = JObject.Load(jtr);
                }
            }

            long timescale = obj.Value<long>("timescale");
            int width = obj.Value<int>("width");
            int height = obj.Value<int>("height");

            List<Frame> frames = new List<Frame>();
            foreach(JToken fragment in obj.Value<JArray>("fragments"))
            {
                long start = fragment.Value<long>("start");
                long interval = fragment.Value<long>("interval");

                int index = 0;
                foreach (JArray frame in fragment.Value<JArray>("events"))
                {
                    List<Annotation> annotations = new List<Annotation>();
                    foreach (JToken annotation in frame)
                    {
                        annotations.Add(new Annotation(
                            annotation.Value<int>("id"),
                            annotation.Value<float>("x") * width,
                            annotation.Value<float>("y") * height,
                            annotation.Value<float>("width") * width,
                            annotation.Value<float>("height") * height));
                    }

                    long timestamp = start + index++ * interval;
                    frames.Add(new Frame(1.0f * timestamp / timescale, annotations.ToArray()));
                }
            }

            return new Metadata(frames);
        }

        private float[] _frameTimes;
        private Frame[] _frames;
        private Dictionary<int, float> _object2FrameTimestampMap;

        public Metadata(IEnumerable<Frame> frames)
        {
            _frameTimes = frames.Select(frame => frame.Timestamp).OrderBy(t => t).ToArray();
            _frames = frames.OrderBy(f => f.Timestamp).ToArray();
            _object2FrameTimestampMap = new Dictionary<int, float>();
            foreach (Frame frame in frames)
            {
                foreach(Annotation annotation in frame.Annotations)
                {
                    if(!_object2FrameTimestampMap.ContainsKey(annotation.Id))
                    {
                        _object2FrameTimestampMap.Add(annotation.Id, frame.Timestamp);
                    }
                }
            }
        }
        
        public Frame GetFrame(float timestamp)
        {
            if(timestamp < _frameTimes.First() || timestamp > _frameTimes.Last())
            {
                return null;
            }

            int pos = Array.BinarySearch(_frameTimes, timestamp);
            if(pos < 0)
            {
                pos = ~pos;
                if(pos == _frameTimes.Length)
                {
                    return null;
                }
            }

            return _frames[pos];            
        }

        public float GetObjectTimestamp(int label)
        {
            return _object2FrameTimestampMap[label];
        }
    }
}
