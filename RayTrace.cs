using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

namespace RayTraceApplication
{
    public class RayTrace
    {
        public static RayTrace instance { get; private set; } = new RayTrace();

        //在program中创建(可以定义)相机与视口
        static Canvas canvas = new Canvas();

        static ViewPort viewPort = new ViewPort(canvas);//进行自适应

        //Create the Environment
        static Environment environment = new Environment();

        public static Stopwatch stopwatch = new Stopwatch(); //创建一个计时器

        public static string logFilepath = "Log.txt";

        public static Form CanvasForm = new Form()
        {
            Height = canvas.CanvasHeight * LG.PixelPerUnit,
            Width = canvas.CanvasWidth * LG.PixelPerUnit
        };


        int global_t_min = 1;
        int global_t_max = int.MaxValue;
        SolidBrush fillBrush = new SolidBrush(Environment.backgroundColor);

        System.Drawing.Color fillColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);

        //---------------------------------------

        //System.Drawing.Color color = System.Drawing.Color.Blue;
        //// Create solid brush.
        //SolidBrush blueBrush = new SolidBrush(color);
        //SolidBrush blackBrush = new SolidBrush(Environment.backgroundColor);

        //// Create rectangle.
        //Rectangle rect = new Rectangle(0, 0, 1, 1);//单一像素矩形;x,y像素位置;

        //// Fill rectangle to screen.
        //e.Graphics.FillRectangle(blueBrush, rect);

        //--------------------------------------
        Rectangle fillRect = new Rectangle(0, 0, 1, 1);//填充的像素点位置
        Vector3 canvasPoint = new Vector3();
        Vector3 viewPoint = new Vector3();
        Color myFillColor = new Color(0, 0, 0);

        //多线程光线追踪
        int numThreads = 16; // 定义线程数量

        public void CanvasForm_Paint(object sender, PaintEventArgs e)//使用该函数来进行光线追踪的显示
        {
            //单线程光线追踪
            // SingleThreadTraceRay(e);
            //多线程光线追踪
            MultiThreadTraceRay(e);
            //结束计时
            stopwatch.Stop();
            //把时间附加输出到当前文件夹中的Log.txt文件中
            WriteResultToFile();
        }

        private void MultiThreadTraceRay(PaintEventArgs e)
        {
            // 使用多线程进行光线追踪
            int width = CanvasForm.Width;
            int height = CanvasForm.Height;
            int stepX = LG.WStep;
            int stepY = LG.HStep;

            // 创建线程数组
            Thread[] threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                int threadIndex = i; // 保存线程索引

                threads[i] = new Thread(() =>
                {
                    for (int x = threadIndex; x < width; x += numThreads)
                    {
                        for (int y = 0; y < height; y += stepY)
                        {
                            // 光线追踪逻辑
                            canvasPoint = FaceToCanvas(x, y, 0);
                            viewPoint = CanvasToViewPort(canvasPoint.X, canvasPoint.Y, canvasPoint.Z);
                            myFillColor = TraceRay(LG.Org, viewPoint, global_t_min, global_t_max, LG.Max_depth);

                            int Draw_colorX, Draw_colorY, Draw_colorZ;
                            Draw_colorX = (int)myFillColor.color.X;
                            Draw_colorY = (int)myFillColor.color.Y;
                            Draw_colorZ = (int)myFillColor.color.Z;
                            GarmmaFixed(ref Draw_colorX, ref Draw_colorY, ref Draw_colorZ);
                            Draw_colorX = ClampToColor(Draw_colorX);

                            // 绘制像素点
                            lock (e)
                            {
                                e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(Draw_colorX, Draw_colorY, Draw_colorZ)), x, y, stepX, stepY);
                            }
                        }
                    }
                });

                threads[i].Start(); // 启动线程
            }

            // 等待所有线程完成
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }


        private void SingleThreadTraceRay(PaintEventArgs e)
        {
            //暂时不使用线程
            for (int x = 0; x < CanvasForm.Width; x += LG.WStep)//填充所有的像素点
            {
                for (int y = 0; y < CanvasForm.Height; y += LG.HStep)
                {

                    //Console.WriteLine("On canvas:{0} {1} {2}",x,y, canvas.CanvasDistance);
                    canvasPoint = FaceToCanvas(x, y, 0);//0代表在canvas上

                    viewPoint = CanvasToViewPort(canvasPoint.X, canvasPoint.Y, canvasPoint.Z);//先作为正的Z值

                    Console.WriteLine("On viewPort {0} {1} {2}", viewPoint.X, viewPoint.Y, viewPoint.Z);

                    myFillColor = TraceRay(LG.Org, viewPoint, global_t_min, global_t_max, LG.Max_depth);

                    int Draw_colorX, Draw_colorY, Draw_colorZ;
                    Draw_colorX = (int)myFillColor.color.X;
                    Draw_colorY = (int)myFillColor.color.Y;
                    Draw_colorZ = (int)myFillColor.color.Z;
                    GarmmaFixed(ref Draw_colorX, ref Draw_colorY, ref Draw_colorZ);

                    Draw_colorX = ClampToColor(Draw_colorX);
                    Draw_colorY = ClampToColor(Draw_colorY);
                    Draw_colorZ = ClampToColor(Draw_colorZ);

                    Console.WriteLine("The result of the TraceRay is{0} {1} {2}", Draw_colorX, Draw_colorY, Draw_colorZ);
                    fillColor = System.Drawing.Color.FromArgb(255, Draw_colorX, Draw_colorY, Draw_colorZ);
                    fillRect.X = x;
                    fillRect.Y = y;
                    Console.WriteLine("On face {0} {1}", x, y);

                    fillBrush.Color = fillColor;//仅仅改变颜色值，避免创造对象影响性能
                    e.Graphics.FillRectangle(fillBrush, fillRect);
                }
            }
        }

        private static void WriteResultToFile()
        {
            //把时间附加输出到当前文件夹中的Log.txt文件中
            System.IO.File.AppendAllText(logFilepath, "Time elapsed: " + stopwatch.ElapsedMilliseconds + "ms\t" + stopwatch.ElapsedMilliseconds / 1000 + "s\t\t");

            if (System.IO.File.ReadAllLines(logFilepath).Length > 0)
            {
                //获取Log.txt文件中第一行的"Time elapsed: " + stopwatch.ElapsedMilliseconds + "ms\t" + stopwatch.ElapsedMilliseconds/1000 + "s\t\t的stopwatch.ElapsedMilliseconds/1000然后计算好当前的stopwatch.ElapsedMilliseconds/1000占比

                string firstLine = System.IO.File.ReadLines(logFilepath).First();
                string[] parts = firstLine.Split(':');
                string[] times = parts[1].Split('m');
                string timeElapsed = times[0];
                int mil = int.Parse(timeElapsed.Substring(1, timeElapsed.Length - 1));

                double percentage = (double)stopwatch.ElapsedMilliseconds / mil * 100;
                System.IO.File.AppendAllText(logFilepath, percentage + "%\n");
            }
        }

        static void GarmmaFixed(ref int X, ref int Y, ref int Z)
        {
            X = (int)Math.Pow(X, LG.Garmma);
            Y = (int)Math.Pow(Y, LG.Garmma);
            Z = (int)Math.Pow(Z, LG.Garmma);
        }

        static int ClampToColor(int num)
        {
            int Max = 255;
            int Min = 0;
            if (num >= Min && num <= Max)
            {
                return num;
            }
            if (num > Max) return Max;
            return Min;
        }

        static Vector3 FaceToCanvas(double Face_x, double Face_y, double Face_z)
        {
            float Canvas_x = (float)Face_x / LG.PixelPerUnit - ((float)canvas.CanvasWidth) / 2;
            float Canvas_y = -(float)Face_y / LG.PixelPerUnit + ((float)canvas.CanvasHeight) / 2;
            float Canvas_z = (float)Face_z / LG.PixelPerUnit + canvas.CanvasDistance;

            return new Vector3(Canvas_x, Canvas_y, Canvas_z);
        }

        static Vector3 CanvasToViewPort(float vx, float vy, float vz)
        {
            float x, y, z;
            x = vx * viewPort.Width / canvas.CanvasWidth;
            y = vy * viewPort.Height / canvas.CanvasHeight;
            z = vz * viewPort.distance / canvas.CanvasDistance;
            return new Vector3(x, y, z);
        }

        static Color TraceRay(Vector3 O, Vector3 d, double t_min, double t_max, int depth)//返回自定义的Color结构体
        {//0代表起始点，d代表viewport上的点,t_min代表探测的最小值t*d,t_max代表探测的最大区域
         //对场景中的每个球体进行解方程
            Color ret_color = new Color(LG.BACKR, LG.BACKG, LG.BACKB);
            double t_ret = t_max;
            Sphere sphere_active = null;

            sphere_active = ClosestInter(O, d, t_min, t_max, out t_ret);

            if (sphere_active == null || depth == 0)
                return ret_color;

            if (t_ret > t_min && t_ret < t_max && sphere_active != null)
            {
                Vector3 P = O + d * new Vector3((float)t_ret, (float)t_ret, (float)t_ret);
                Vector3 V = O - P;
                Vector3 N = (P - sphere_active.center) / sphere_active.radius;

                double pointIntensity = 0;
                Color active_Color = new Color((int)sphere_active.color.X, (int)sphere_active.color.Y, (int)sphere_active.color.Z);
                ret_color.color = ComputeLighting(P, N, V, (float)sphere_active.specular) * active_Color;
                float reflection = (float)sphere_active.reflection;

                if (reflection > 0 && reflection <= 1)
                {
                    Vector3 ReflectRay = Reflect(N, V);
                    ret_color.color = (1 - reflection) * ret_color + reflection * TraceRay(P, ReflectRay, 0.001, t_max, depth - 1);
                }
            }
            return ret_color;
        }

        //获取最短的t,用于阴影检测
        //static double ClosestInter(Vector3 o, Vector3 d, double t_min, double t_max) { 

        static Sphere ClosestInter(Vector3 P, Vector3 L, double t_min, double t_max, out double Sphere_t)
        {
            double t_ret = t_max;
            Sphere sphere_active = null;
            foreach (var sphere in environment.spheres)
            {
                double t_active;
                double[] ts = new double[2];
                ts = IntersectRaySphere(P, L, sphere, t_min, t_max);
                t_active = ts.Min();
                if (t_active > t_min && t_active < t_max)
                {
                    if (t_active < t_ret)
                    {
                        t_ret = t_active;
                        sphere_active = sphere;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("Find a Sphere!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }
            }

            Sphere_t = t_ret;
            return sphere_active;
        }

        //解方程
        static double[] IntersectRaySphere(Vector3 o, Vector3 d, Sphere sphere, double t_min, double t_max)
        {
            double t1 = t_max, t2 = t_max;

            double a = Vector3.Dot(d, d);
            double b = 2 * Vector3.Dot(d, -sphere.center);
            double c = Vector3.Dot(-sphere.center, -sphere.center) - Math.Pow(sphere.radius, 2);

            double sq = b * b - 4 * a * c;
            if (sq > 0)
            {
                t1 = (-b - Math.Sqrt(sq)) / (2 * a);
                t2 = (-b + Math.Sqrt(sq)) / (2 * a);
            }
            return new double[] { t1, t2 };
        }

        static double ComputeLighting(Vector3 P, Vector3 N, Vector3 V, float S)
        {
            double i = 0.0;
            Vector3 L = Vector3.Zero;//光向量
            double t_max = double.MaxValue;//用来检测阴影
            foreach (Light light in environment.lights)
            {
                if (light.type == "Global")
                {
                    i += light.intensity;
                }
                else
                {
                    if (light.type == "Point")
                    {
                        L = light.position - P;
                        t_max = 1;
                    }
                    else if (light.type == "Direction")
                    {
                        object o = light;//使用装箱操作,也可以使用动态绑定,都是在运行时使用的
                        DirectionalLight direc = (DirectionalLight)o;
                        L = direc.direction;
                        t_max = double.MaxValue;
                    }

                    Func<Vector3, double> Length = (vec) => Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);

                    Sphere Shadow_sphere;
                    double Shadow_t;
                    //阴影检测
                    Shadow_sphere = ClosestInter(P, L, 0.001, t_max, out Shadow_t);
                    if (Shadow_sphere != null)
                        continue;


                    //漫反射计算
                    double n_dot_l = Vector3.Dot(L, N);
                    if (n_dot_l > 0)
                        i += light.intensity * n_dot_l / (Length(N) * Length(L));

                    //镜面反射计算
                    if (S > -1)
                    {
                        Vector3 R = Reflect(N, L);
                        double r_dot_v = Vector3.Dot(R, V);
                        if (r_dot_v > 0)
                        {
                            i += light.intensity * Math.Pow(r_dot_v / (Length(R) * Length(V)), S);
                        }
                    }
                }
            }
            return i;
        }

        static Vector3 Reflect(Vector3 N, Vector3 L)
        {
            return 2 * N * Vector3.Dot(N, L) - L;
        }
    }
}





