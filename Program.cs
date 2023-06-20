using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RayTraceApplication
{
    class Program
    {
        //在program中创建(可以定义)相机与视口
        static Canvas canvas = new Canvas();

        static ViewPort viewPort = new ViewPort(canvas);//进行自适应

        static Vector3 Org = new Vector3(0,0,0);//定义坐标原点

        //Create the Environment
        static Environment environment = new Environment();

        static Form CanvasForm = new Form() 
        { Height = canvas.CanvasHeight * LG.PixelPerUnit, 
            Width = canvas.CanvasWidth * LG.PixelPerUnit };


        static void Main(string[] args)
        {

            CanvasForm.Text = "简易光线追踪渲染器";

            CanvasForm.Paint += CanvasForm_Paint;//该事件处理器中进行绘制

            CanvasForm.ShowDialog();

            Console.ReadLine();
        }

        private static void CanvasForm_Paint(object sender, PaintEventArgs e)//使用该函数来进行光线追踪的显示
        {


            int global_t_min = 1;
            int global_t_max = int.MaxValue;
            SolidBrush fillBrush = new SolidBrush(Environment.backgroundColor);

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

            //暂时不使用线程
            for (int x = 0; x < CanvasForm.Width; x++)//填充所有的像素点
            {
                for (int y = 0; y < CanvasForm.Height; y++)
                {

                    //Console.WriteLine("On canvas:{0} {1} {2}",x,y, canvas.CanvasDistance);
                    Vector3 canvasPoint = FaceToCanvas(x,y,0);//0代表在canvas上

                    Vector3 viewPoint = CanvasToViewPort(canvasPoint.X,canvasPoint.Y, canvasPoint.Z);//先作为正的Z值

                    Console.WriteLine("On viewPort {0} {1} {2}", viewPoint.X,viewPoint.Y, viewPoint.Z);

                    Color myFillColor = TraceRay(Org, viewPoint,global_t_min,global_t_max, LG.Max_depth);

                    int Draw_colorX, Draw_colorY, Draw_colorZ;
                    Draw_colorX = ClampToColor((int)myFillColor.color.X);
                    Draw_colorY = ClampToColor((int)myFillColor.color.Y);
                    Draw_colorZ = ClampToColor((int)myFillColor.color.Z);


                    Console.WriteLine("The result of the TraceRay is{0} {1} {2}", Draw_colorX, Draw_colorY, Draw_colorZ);
                    System.Drawing.Color fillColor = System.Drawing.Color.FromArgb(255,Draw_colorX, Draw_colorY, Draw_colorZ);

                    Console.WriteLine("On face {0} {1}",x,y);
                    fillBrush.Color = fillColor;//仅仅改变颜色值，避免创造对象影响性能
                    Rectangle fillRect = new Rectangle(x, y, 1, 1);//填充的像素点位置
                    e.Graphics.FillRectangle(fillBrush, fillRect);
                }
            }
        }

        static int ClampToColor(int num)
        {
            int Max = 255;
            int Min = 0;
            if (num >= Min && num <= Max)
            {
                return num;
            }
            if(num>Max) return Max;
            return Min;
        }


        static Vector3 FaceToCanvas(double Face_x, double Face_y, double Face_z)
        {
            float Canvas_x = (float)Face_x / LG.PixelPerUnit - ((float)canvas.CanvasWidth)/2;
            float Canvas_y = -(float)Face_y / LG.PixelPerUnit + ((float)canvas.CanvasHeight)/2;
            float Canvas_z = (float)Face_z / LG.PixelPerUnit + canvas.CanvasDistance;

            return new Vector3(Canvas_x, Canvas_y, Canvas_z);
        }

        static Vector3 CanvasToViewPort(float vx, float vy, float vz)
        {
            float x, y, z;
            x = vx * viewPort.Width/canvas.CanvasWidth;
            y = vy * viewPort.Height/canvas.CanvasHeight;
            z = vz* viewPort.distance/canvas.CanvasDistance;
            return new Vector3(x, y, z);
        }

        static Color TraceRay(Vector3 O, Vector3 d, double t_min, double t_max, int depth)//返回自定义的Color结构体
        {//0代表起始点，d代表viewport上的点,t_min代表探测的最小值t*d,t_max代表探测的最大区域
         //对场景中的每个球体进行解方程
            Color ret_color = new Color(LG.BACKR, LG.BACKG, LG.BACKB);
            double t_ret = t_max;
            Sphere sphere_active = null;

            sphere_active = ClosestInter(O, d, t_min, t_max, out t_ret);

            if(sphere_active==null || depth==0)
                return ret_color;

            if (t_ret > t_min && t_ret < t_max && sphere_active!=null)
            {
                Vector3 P = O + d * new Vector3((float)t_ret, (float)t_ret, (float)t_ret);
                Vector3 V = O - P;
                Vector3 N = (P - sphere_active.center)/sphere_active.radius;

                double pointIntensity = 0;
                Color active_Color = new Color((int)sphere_active.color.X, (int)sphere_active.color.Y, (int)sphere_active.color.Z);
                ret_color.color = ComputeLighting(P, N, V, (float)sphere_active.specular) * active_Color;
                float reflection = (float)sphere_active.reflection;

                if (reflection > 0 && reflection <= 1)
                {
                    Vector3 ReflectRay = Reflect(N,V);
                    ret_color.color = (1 - reflection) * ret_color + reflection * TraceRay(P, ReflectRay, 0.001, t_max, depth-1);
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


        //}

        //解方程
        static double[] IntersectRaySphere(Vector3 o, Vector3 d,Sphere sphere, double t_min, double t_max)
        {
            double t1 = t_max, t2 = t_max;

            double a = Vector3.Dot(d,d);
            double b = 2 * Vector3.Dot(d,-sphere.center);
            double c = Vector3.Dot(-sphere.center,-sphere.center)- Math.Pow(sphere.radius,2);

            double sq = b * b - 4 * a * c;
            if(sq>0)
            {
                t1 = (-b - Math.Sqrt(sq)) / (2 * a);
                t2 = (-b + Math.Sqrt(sq)) / (2 * a);
            }
            return new double[] { t1,t2};
        }

        static double ComputeLighting(Vector3 P, Vector3 N, Vector3 V, float S)
        {
            double i = 0.0;
            Vector3 L = Vector3.Zero;//光向量
            double t_max = double.MaxValue;//用来检测阴影
            foreach (Light light in environment.lights)
            {
                if(light.type == "Global")
                {
                    i+=light.intensity;
                }
                else
                {
                    if(light.type == "Point")
                    {
                        L = light.position - P;
                        t_max = 1;
                    }
                    else if(light.type == "Direction")
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
                    double n_dot_l = Vector3.Dot(L,N);
                    if (n_dot_l > 0)
                        i += light.intensity * n_dot_l /(Length(N)*Length(L)) ;

                    //镜面反射计算
                    if(S>-1)
                    {
                        Vector3 R = Reflect(N,L);
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
