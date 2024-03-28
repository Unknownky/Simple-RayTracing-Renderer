using System;

namespace RayTraceApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            RayTrace.CanvasForm.Text = "简易光线追踪渲染器";

            //开始计时 
            RayTrace.stopwatch.Start();

            RayTrace.CanvasForm.Paint += RayTrace.instance.CanvasForm_Paint;//该事件处理器中进行绘制

            RayTrace.CanvasForm.ShowDialog();

            Console.ReadLine();
        }

    }
        
}
