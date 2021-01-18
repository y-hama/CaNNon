using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reader
{
    abstract class Reader
    {
        private object __queuelock = new object();
        private Queue<BufferItem> ItemQueue { get; set; } = new Queue<BufferItem>();

        private static System.Threading.Thread gcThread { get; set; } = null;

        public int ReadChannels { get; private set; }

        protected virtual int BufferingSize { get; } = 10;

        protected int epoch { get; set; } = 0;

        protected Reader(int readChannels)
        {
            ReadChannels = readChannels;
        }

        public class BufferItem
        {
            public int Epoch { get; set; }
            public Field.BufferField Input = null;
            public Field.BufferField Teacher = null;

            public BufferItem Congruence()
            {
                var item = new BufferItem();
                item.Input = Input.Congruence();
                item.Teacher = Teacher.Congruence();
                return item;
            }
        }

        private BufferItem Source { get; set; }

        protected abstract void Initialize();
        protected abstract void Get(ref BufferItem buffer);

        private bool IsRunning { get; set; } = false;

        public BufferItem GetBuffer()
        {
            BufferItem item = null;
            while (item == null)
            {
                bool check = false;
                lock (__queuelock)
                {
                    if (ItemQueue.Count > 0)
                    {
                        item = ItemQueue.Dequeue();
                        check = true;
                    }
                }
                if (!check)
                {
                    //Console.WriteLine("!Reader BufferWait");
                    System.Threading.Thread.Sleep(1);
                }
            }

            return item;
        }

        public void ModelReflection(Alea.Gpu gpu, Common.ModelEdgeParameter param)
        {
            Initialize();
            Source = new BufferItem();
            Source.Input = new Field.BufferField(gpu, param.InputSize, param.InputChannels);
            Source.Teacher = new Field.BufferField(gpu, param.OutputSize, param.OutputChannels);
        }

        public void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                new System.Threading.Thread(() =>
                {
                    while (true)
                    {
                        bool check = false;
                        bool condition = false;
                        lock (__queuelock)
                        {
                            condition = ItemQueue.Count < BufferingSize;
                        }

                        if (condition)
                        {
                            var item = Source.Congruence();
                            Get(ref item);
                            lock (__queuelock)
                            {
                                ItemQueue.Enqueue(item);
                            }
                            check = true;
                        }
                        if (!check) { System.Threading.Thread.Sleep(1); }
                        else
                        {
                            //Console.WriteLine($"Reader : Load-> c/{ItemQueue.Count}");
                        }
                    }
                }).Start();
            }

            if (null == gcThread)
            {
                gcThread = new System.Threading.Thread(() =>
                {
                    while (true)
                    {
                        GC.Collect();
                        System.Threading.Thread.Sleep(1000);
                    }
                });
                gcThread.Start();
            }
        }

        protected void RandomNoize(ref Field.BufferField f, double noize, Random random)
        {
            for (int c = 0; c < f.Channels; c++)
            {
                for (int i = 0; i < f.Width; i++)
                {
                    for (int j = 0; j < f.Height; j++)
                    {
                        f.Buffer[c][i, j] += (random.NextDouble() * 2 - 1) * noize;
                    }
                }
            }
        }
    }
}
