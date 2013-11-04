using System.Collections.Generic;
using System.Threading;

namespace AccleZigBee
{

    public partial class MainHome
    {
        private ManualResetEvent resumeEvent;
        private volatile bool paused;

        //将循环线程挂起
        private void Pause()
        {
            resumeEvent.Reset();
            paused = true;
        }
        //将循环线程激活
        private void Resume()
        {
            paused = false;
            resumeEvent.Set();
        }
        //用来记录当前是否是周期发送的标志
        private volatile bool cycleFlag;
        private volatile bool ringFlag;
        private volatile bool oneFlag;
        private bool isFirstCycleFlag;
      //  private volatile bool manuallyFlag;
        private bool workFlag;

        //用来统计计数发送包编号
        private static byte cnt = 0;
        //用来回应cnt
        private byte ACK;
        //待发送控制包缓冲队列
        private LinkedList<CtlPacket> SendPacket;
        //用来保护缓冲队列
        private object ls;
        //没有控制命令时，发送到zigbee节点的包
        private byte[] noDataAck;
        //循环发送线程
        private Thread sendThread;
    }
}
