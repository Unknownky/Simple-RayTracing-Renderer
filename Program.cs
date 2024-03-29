
namespace RayTraceApplication
{
    class Program
    {
        public static bool isLoop = false;

        public static bool isMultiThread = true;
        private static int loopCount = 2;
        static void Main(string[] args)
        {
            if(isLoop)
            {
                LoopRayTrace();
            }
            else
            {
                SingleRayTrace();
            }
        }

        private static void SingleRayTrace()
        {
            RayTrace.CanvasForm.Text = "简易光线追踪渲染器";

            //开始计时 
            RayTrace.stopwatch.Start();

            RayTrace.CanvasForm.Paint += RayTrace.instance.CanvasForm_Paint;//该事件处理器中进行绘制

            RayTrace.CanvasForm.ShowDialog();

        }

        //循环进行光纤追踪计算，用来测量数据
        private static void LoopRayTrace()
        {
            for (int i = 0; i < loopCount; i++)
            {
                SingleRayTrace();
            }
            isLoop = false;
        }
    }
}
