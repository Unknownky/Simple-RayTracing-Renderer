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

        //在program中创建(可以定义)相机与视口，这里使用默认设置
        public static Canvas canvas = new Canvas();

        public static ViewPort viewPort = new ViewPort(canvas);//视口根据canvas的大小来自适应，暂不主动设置

        //Create the Environment
        static Environment environment = new Environment();

        public static Stopwatch stopwatch = new Stopwatch(); //创建一个计时器

        public static readonly string logFilepath = "Log.txt";

        public static Form CanvasForm = new Form() //根据渲染空间的画布大小来设置对应的窗体大小(也即像素大小)
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
        readonly int numThreads = 4; // 定义线程数量，经过测试最好为4

        readonly int alpha = 255;

        public void CanvasForm_Paint(object sender, PaintEventArgs e)//使用该函数来进行光线追踪的显示
        {
            environment.EquipBoundaries();//配置好边界体
            if (Program.isMultiThread)
            {
                //多线程光线追踪
                MultiThreadTraceRay(e);
            }
            else
            {
                //单线程光线追踪
                SingleThreadTraceRay(e);
            }
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

            // 创建线程数组和位图数组
            Thread[] threads = new Thread[numThreads];
            Bitmap[] bitmaps = new Bitmap[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                int threadIndex = i; // 保存线程索引
                bitmaps[i] = new Bitmap(width, height);

                threads[i] = new Thread(() =>
                {
                    using (Graphics g = Graphics.FromImage(bitmaps[threadIndex]))
                    {
                        for (int x = threadIndex; x < width; x += numThreads)
                        {
                            for (int y = 0; y < height; y += stepY)
                            {
                                // 光线追踪逻辑
                                // canvasPoint = FaceToCanvas(x, y, 0);
                                // viewPoint = CanvasToViewPort(canvasPoint.X, canvasPoint.Y, canvasPoint.Z);

                                //直接合并FaceToCanvas和CanvasToViewPort，优化为一步
                                viewPoint = FaceToViewPort(x, y, 0);

                                myFillColor = TraceRay(LG.Org, viewPoint, global_t_min, global_t_max, LG.Max_depth);

                                int Draw_colorX, Draw_colorY, Draw_colorZ;
                                Draw_colorX = (int)myFillColor.color.X;
                                Draw_colorY = (int)myFillColor.color.Y;
                                Draw_colorZ = (int)myFillColor.color.Z;
                                GarmmaFixed(ref Draw_colorX, ref Draw_colorY, ref Draw_colorZ);
                                Draw_colorX = ClampToColor(Draw_colorX);
                                Draw_colorY = ClampToColor(Draw_colorY);
                                Draw_colorZ = ClampToColor(Draw_colorZ);

                                // 在位图上绘制像素点
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(alpha, Draw_colorX, Draw_colorY, Draw_colorZ)), x, y, stepX, stepY);
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

            // 在主画布上绘制位图
            foreach (Bitmap bitmap in bitmaps)
            {
                e.Graphics.DrawImageUnscaled(bitmap, 0, 0);
                bitmap.Dispose(); // 释放位图资源
            }

            // 释放线程资源
            foreach (Thread thread in threads)
            {
                thread.Abort();
            }

            if (Program.isLoop)
            {
                // 重绘窗体
                CanvasForm.Invalidate();
            }
        }

        private void SingleThreadTraceRay(PaintEventArgs e)
        {
            //暂时不使用线程
            for (int x = 0; x < CanvasForm.Width; x += LG.WStep)//填充所有的像素点
            {
                for (int y = 0; y < CanvasForm.Height; y += LG.HStep)
                {
                    canvasPoint = FaceToCanvas(x, y, 0);//0代表在canvas上
                    viewPoint = CanvasToViewPort(canvasPoint.X, canvasPoint.Y, canvasPoint.Z);//先作为正的Z值

                    myFillColor = TraceRay(LG.Org, viewPoint, global_t_min, global_t_max, LG.Max_depth);

                    int Draw_colorX = (int)myFillColor.color.X;
                    int Draw_colorY = (int)myFillColor.color.Y;
                    int Draw_colorZ = (int)myFillColor.color.Z;
                    GarmmaFixed(ref Draw_colorX, ref Draw_colorY, ref Draw_colorZ);

                    Draw_colorX = ClampToColor(Draw_colorX);
                    Draw_colorY = ClampToColor(Draw_colorY);
                    Draw_colorZ = ClampToColor(Draw_colorZ);

                    fillColor = System.Drawing.Color.FromArgb(255, Draw_colorX, Draw_colorY, Draw_colorZ);
                    fillRect.X = x;
                    fillRect.Y = y;

                    fillBrush.Color = fillColor;
                    e.Graphics.FillRectangle(fillBrush, fillRect);
                }
            }
        }

        //将运行结果写入文件便于分析
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

        //garmma修正
        static void GarmmaFixed(ref int X, ref int Y, ref int Z)
        {
            X = (int)Math.Pow(X, LG.Garmma);
            Y = (int)Math.Pow(Y, LG.Garmma);
            Z = (int)Math.Pow(Z, LG.Garmma);
        }

        //颜色单值修正
        static int ClampToColor(int num)
        {
            return num < 0 ? 0 : (num > 255 ? 255 : num); // Custom implementation to clamp the value between 0 and 255
        }

        //Tag:优化部分，减少倒数的计算
        static readonly float invPixelPerUnit = 1.0f / LG.PixelPerUnit;
        static readonly float invCanvasWidth = 1.0f / canvas.CanvasWidth;
        static readonly float invCanvasHeight = 1.0f / canvas.CanvasHeight;
        static readonly float invCanvasDistance = 1.0f / canvas.CanvasDistance;


        //将像素点转换到Canvas上的坐标，Z的值都为CanvasDistance,FaceToCanvas才涉及到坐标系正负问题
        static Vector3 FaceToCanvas(float Face_x, float Face_y, float Face_z)
        {
            float Canvas_x = Face_x * invPixelPerUnit - canvas.CanvasWidth * 0.5f;
            float Canvas_y = -Face_y * invPixelPerUnit + canvas.CanvasHeight * 0.5f;
            float Canvas_z = Face_z * invPixelPerUnit + canvas.CanvasDistance;

            return new Vector3(Canvas_x, Canvas_y, Canvas_z);
        }

        //将Canvas上的点转换到ViewPort上
        static Vector3 CanvasToViewPort(float vx, float vy, float vz)
        {
            float x = vx * viewPort.Width * invCanvasWidth;
            float y = vy * viewPort.Height * invCanvasHeight;
            float z = vz * viewPort.distance * invCanvasDistance;

            return new Vector3(x, y, z);
        }

        //合并FaceToCanvas和CanvasToViewPort
        static Vector3 FaceToViewPort(float Face_x, float Face_y, float Face_z)
        {
            float Canvas_x = Face_x * invPixelPerUnit - canvas.CanvasWidth * 0.5f;
            float Canvas_y = -Face_y * invPixelPerUnit + canvas.CanvasHeight * 0.5f;
            float Canvas_z = Face_z * invPixelPerUnit + canvas.CanvasDistance;

            float x = Canvas_x * viewPort.Width * invCanvasWidth;
            float y = Canvas_y * viewPort.Height * invCanvasHeight;
            float z = Canvas_z * viewPort.distance * invCanvasDistance;

            return new Vector3(x, y, z);
        }


        //根据视口上的点进行光线追踪，视口后的物体才能被渲染在画布上，由于光线追踪反射的也能被渲染
        static Color TraceRay(Vector3 O, Vector3 d, float t_min, float t_max, int depth)//返回自定义的Color结构体
        {//0代表起始点，d代表viewport上的点,t_min代表探测的最小值t*d,t_max代表探测的最大区域
         //对场景中的每个球体进行解方程
            Color ret_color = new Color(LG.BACKR, LG.BACKG, LG.BACKB); //背景颜色
            float t_ret = t_max;
            Sphere sphere_active = null;

            sphere_active = ClosestInter(O, d, t_min, t_max, out t_ret);

            if (sphere_active == null || depth == 0)
                return ret_color;

            //边界体判断逻辑，避免后面的计算
            if (!IsWithinBoundary(d))
            {
                return ret_color;
            }


            if (t_ret > t_min && t_ret < t_max && sphere_active != null)
            {
                Vector3 P = O + d * new Vector3(t_ret, t_ret, t_ret);
                Vector3 V = O - P;
                Vector3 N = (P - sphere_active.center) / sphere_active.radius;

                float pointIntensity = 0;
                Color active_Color = new Color((int)sphere_active.color.X, (int)sphere_active.color.Y, (int)sphere_active.color.Z);
                ret_color.color = ComputeLighting(P, N, V, sphere_active.specular) * active_Color;
                float reflection = sphere_active.reflection;

                if (reflection > 0 && reflection <= 1)
                {
                    Vector3 ReflectRay = Reflect(N, V);
                    ret_color.color = (1 - reflection) * ret_color + reflection * TraceRay(P, ReflectRay, 0.001f, t_max, depth - 1);
                }
            }
            return ret_color;
        }

        static bool IsWithinBoundary(Vector3 viewPoint)
        {
            //先排除两种情况
            if (viewPoint.X < environment.minXArray[0])
            {
                return false;
            }
            if (viewPoint.X > environment.minXArray[environment.spheres.Length - 1])
            {
                return false;
            }
            //再进行二分查找，查找到大于的最大的minXArray的值的索引
            int index = Array.BinarySearch(environment.minXArray, viewPoint.X);
            if(environment.boundaries[index].x_max < viewPoint.X)
            {
                return false;
            }
            if(environment.boundaries[index].y_max < viewPoint.X)
            {
                return false;
            }
            if(environment.boundaries[index].y_min > viewPoint.X)
            {
                return false;
            }
            return true;//说明在边界体内
        }


        //获取最短的t,用于阴影检测
        //static double ClosestInter(Vector3 o, Vector3 d, double t_min, double t_max) { 
        static Sphere ClosestInter(Vector3 P, Vector3 L, float t_min, float t_max, out float Sphere_t)
        {
            float t_ret = t_max;
            Sphere sphere_active = null;
            foreach (var sphere in environment.spheres)
            {
                float t_active;
                float[] ts = new float[2];
                ts = IntersectRaySphere(P, L, sphere, t_min, t_max);
                t_active = ts.Min();
                if (t_active > t_min && t_active < t_max)
                {
                    if (t_active < t_ret)
                    {
                        t_ret = t_active;
                        sphere_active = sphere;
                        // Console.BackgroundColor = ConsoleColor.Red;
                        // Console.WriteLine("Find a Sphere!!!!!!!!!!!!!!!!!!!!!!!!!");
                        // Console.BackgroundColor = ConsoleColor.Black;
                    }
                }
            }

            Sphere_t = t_ret;
            return sphere_active;
        }

        //解方程，求交点
        static float[] IntersectRaySphere(Vector3 o, Vector3 d, Sphere sphere, float t_min, float t_max)
        {
            float t1 = t_max, t2 = t_max;

            float a = Vector3.Dot(d, d);
            float b = 2 * Vector3.Dot(d, -sphere.center);
            float c = Vector3.Dot(-sphere.center, -sphere.center) - sphere.radius_square; //添加radius的平方的优化  Math.Pow(sphere.radius, 2)-》sphere.radius_square

            float sq = b * b - 4 * a * c;
            if (sq > 0)
            {
                t1 = (-b - (float)Math.Sqrt(sq)) / (2 * a);
                t2 = (-b + (float)Math.Sqrt(sq)) / (2 * a);
            }
            return new float[] { t1, t2 };
        }

        //计算光照
        static float ComputeLighting(Vector3 P, Vector3 N, Vector3 V, float S)
        {
            float i = 0.0f;
            Vector3 L = Vector3.Zero;//光向量
            float t_max = float.MaxValue;//用来检测阴影
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
                        t_max = float.MaxValue;
                    }

                    Func<Vector3, float> Length = (vec) => (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);

                    Sphere Shadow_sphere;
                    float Shadow_t;
                    //阴影检测
                    Shadow_sphere = ClosestInter(P, L, 0.001f, t_max, out Shadow_t);
                    if (Shadow_sphere != null)
                        continue;


                    //漫反射计算
                    float n_dot_l = Vector3.Dot(L, N);
                    if (n_dot_l > 0)
                        i += light.intensity * n_dot_l / (Length(N) * Length(L));

                    //镜面反射计算
                    if (S > -1)
                    {
                        Vector3 R = Reflect(N, L);
                        float r_dot_v = Vector3.Dot(R, V);
                        if (r_dot_v > 0)
                        {
                            i += light.intensity * (float)Math.Pow(r_dot_v / (Length(R) * Length(V)), S);
                        }
                    }
                }
            }
            return i;
        }

        //计算反射向量
        static Vector3 Reflect(Vector3 N, Vector3 L)
        {
            return 2 * N * Vector3.Dot(N, L) - L;
        }
    }
}
