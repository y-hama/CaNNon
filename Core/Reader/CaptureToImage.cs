using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace Core.Reader
{
    class CaptureToImage : Reader
    {
        private int DeviceID { get; set; }
        private string SourceLocation { get; set; }

        private System.IO.FileInfo[] files { get; set; }
        VideoCapture cap;

        private int index { get; set; } = 0;


        public CaptureToImage(int deviceID, string location)
        {
            DeviceID = deviceID;
            SourceLocation = location;
        }

        protected override void Initialize()
        {
            files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            index = 0;

            cap = new VideoCapture(DeviceID);
        }

        protected override void Get(ref BufferItem buffer)
        {
            var frame = new Mat();
            cap.Read(frame);
            frame = frame.Flip(FlipMode.Y);
            buffer.Epoch = epoch;
            buffer.Input.ReadFrom(frame);
            buffer.Teacher.ReadFrom(new OpenCvSharp.Mat(files[index].FullName));

            index++;
            if (index >= files.Length)
            {
                epoch++;
                index = 0;
                files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            }
        }
    }
}
