using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMediaRedactor.Models
{
    class Frame
    {
        public float Timestamp { get; }
        public Annotation[] Annotations { get; }

        public Frame(float timestamp, Annotation[] annotations)
        {
            this.Timestamp = timestamp;
            this.Annotations = annotations;
        }
    }
}
