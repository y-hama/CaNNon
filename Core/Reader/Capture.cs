using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace Core.Reader
{
    class Capture : Reader
    {
        private int DeviceID { get; set; }
        VideoCapture cap;

        private int index { get; set; } = 0;

        protected override int BufferingSize { get => 1; }

        public Capture(int readChannels, int deviceID) : base(readChannels)
        {
            DeviceID = deviceID;
        }

        protected override void Initialize()
        {
            cap = new VideoCapture(DeviceID);
        }

        protected override void Get(ref BufferItem buffer)
        {
            var frame = new Mat();
            cap.Read(frame);
            frame = frame.Flip(FlipMode.Y);
            buffer.Epoch = epoch;
            buffer.Input.ReadFrom(frame);
            buffer.Teacher.ReadFrom(frame);
        }
    }
}
