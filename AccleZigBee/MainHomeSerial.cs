using System.Threading;

namespace AccleZigBee
{

    //串口
    public partial class MainHome
    {
        #region 串口相关成员定义

        /*环形缓冲区*/
        private static readonly int LEN = 2048;  //环形缓冲区长度
        private byte[] QueueBuf = new byte[LEN + 1];//循环队列
        private int head, tail;//循环队列的头尾指针
        /*串口类的对象*/
        private System.IO.Ports.SerialPort serialPort;
        /*环形缓冲区操作的两个线程，一个用于读一个用于写*/
        private Thread writeThread;
        private Thread readThread;
        /*用于环形缓冲区的同步*/
        private object lm;
        private object lo;
        /*帧结构，开始和终止符*/
        private const int START = 0xfe;//开始帧结构
        private const int END = 0xfb;//尾巴帧结构
        /*循环队列标志*/
        private int empty, full;// 空或者满的标志， empty=1为空，empty=0为非空；full=1为满，full=0为不满
        /*提取数据帧的缓冲区*/
        private byte[] tmpBuf;

        #endregion
        //初始化串口
        private void InitSerialPort()
        {
            head = tail = 0;//初始化索引指针
            empty = 1;//为空
            full = 0;//非满
            tmpBuf = new byte[LEN];//定义临时缓冲区
        }
        //打开串口
        private bool openSerialPort()  /*打开串口*/
        {
            InitSerialPort();
            return true;
        }
        //创建循环队列读写线程
        public void StartSerialPort()//启动线程
        {
            if (openSerialPort())
            {
                writeThread = new Thread(new ThreadStart(writeIntoQueueBuf));
                readThread = new Thread(new ThreadStart(readFromQueueBuf));
                writeThread.Start();
                readThread.Start();
            }
        }

        #region 串口相关函数
        #region 读串口并写到循环队列线程
        //从串口读取数据然后写入循环队列
        private void writeIntoQueueBuf()//从串口读数据存入放入循环队列
        {
            Thread.Sleep(3000);
            byte[] tmpBufFromQueue = new byte[LEN + 1];/*从串口读数据缓冲区*/
            int n, i;
            for (; ; )
            {
                n = serialPort.Read(tmpBufFromQueue, 0, LEN);
                if (n <= 0)
                {
                    continue;
                }
                Monitor.Enter(this.lm);//进入临界区
                for (i = 0; i < n; i++)
                {
                    if ((tail + 1) % LEN == head)//缓冲区满
                    {
                        full = 1;//缓冲区满
                        Monitor.Wait(this.lm);  //等待缓冲区有空闲
                        i--;
                    }
                    else
                    {
                        QueueBuf[tail] = tmpBufFromQueue[i];
                        tail = (tail + 1) % LEN;
                        if (empty == 1)// 循环队列以前为空
                        {
                            empty = 0;//循环队列现在不为空
                            Monitor.Pulse(this.lm);//通知有数据可以读取
                        }
                    }
                }
                Monitor.Exit(this.lm);
            }
        }
        #endregion
        #region  读循环队列线程
        /*从循环队列读取数据，供应用使用*/
        private void readFromQueueBuf()
        {
            Thread.Sleep(3000);
            bool sFlag = false;//帧的开始
            byte tmp;//临时变量
            int n;
            n = 0;
            for (; ; )
            {
                Monitor.Enter(this.lm);
                if (head == tail)//缓冲区为空
                {
                    empty = 1;//缓冲区为空标志
                    Monitor.Wait(this.lm);  //等待缓冲区有有数据 
                }
                tmp = QueueBuf[head];//取头元素
                if (!sFlag)//暂未找到切入点
                {
                    if (tmp == START) //找到切入点
                    {
                        sFlag = true;
                        tmpBuf[n++] = tmp;
                        head = (head + 1) % LEN;
                        if (tail == head)
                            empty = 1;
                    }
                    else//丢弃
                    {
                        head = (head + 1) % LEN;//丢弃
                        if (tail == head)
                            empty = 1;
                    }
                }
                else//开始寻找完整帧
                {

                    tmpBuf[n++] = tmp;
                    head = (head + 1) % LEN;
                    if (full == 1)
                    {
                        full = 0;
                        Monitor.Pulse(this.lm);//有空闲空间，通知其他线程
                    }
                    if (n >= 20)
                    {
                        sFlag = false;
                        n = 0;
                    }
                    if (tmp == END && tmpBuf[1] == n)
                    {
                        sFlag = false;
                        // Console.WriteLine("{0},{1}",n,tmpBuf[1]);
                        // Monitor.Enter(this.lb);
                        DataProcess();
                        /*
                        if ((tmpBuf[2] == 0x00) ||(tmpBuf[2] == 0x07))
                        {
                            Console.WriteLine("收到的数据为");
                            for (int index = 0; index < n; index++)
                                Console.WriteLine("{0} = 0x{1}",index,Convert.ToString(tmpBuf[index], 16).PadLeft(2, '0'));

                        }*/
                        // Monitor.Exit(this.lb);
                        n = 0;
                    }
                }
                Monitor.Exit(this.lm);
            }
        }
        #endregion
        #endregion
    }
}
