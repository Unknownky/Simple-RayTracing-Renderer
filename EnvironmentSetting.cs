using System;
using System.Numerics;
using System.Xml.Schema;

namespace RayTraceApplication
{
    public static class LG //标准量
    {
        public const int Unit = 1;  //渲染世界单位 
        public const int PixelPerUnit = 250; //每渲染世界对应窗口像素值
        public const int Max_depth = 3;  //最大递归深度
        public const int BACKR = 45;
        public const int BACKG = 45;
        public const int BACKB = 45;
        public const int Total_area = 5;
        public static Vector3 Org = new Vector3(0, 0, 0);//定义坐标原点
        public const float Garmma = 1.1f; //garmma纠正
        public static int HStep = 1;  //在当前逻辑中影响速度，同时影响渲染的效果
        public static int WStep = 1;  //在当前逻辑中基本不影响速度，只影响渲染的效果
    }

    public class Environment //渲染空间
    {
        public static int sphereCount = 4;

        public static int lightCount = 3;
        public static System.Drawing.Color backgroundColor = System.Drawing.Color.Black;
        public Environment()//初始化场景，暂时进行手动配置
        {
            spheres[0] = redSphere;
            spheres[1] = blueSphere;
            spheres[2] = greenSphere;
            spheres[3] = sphere3;

            lights[0] = globalLight;
            lights[1] = directionLight;
            lights[2] = pointLight;
        }

        public readonly Sphere[] spheres = new Sphere[sphereCount];

        public readonly Boundary[] boundaries = new Boundary[sphereCount]; //自动根据球体生成边界体

        public readonly float[] minXArray = new float[sphereCount]; //边界体的最小X坐标数组 

        public readonly Light[] lights = new Light[lightCount];

        Sphere redSphere =
            new Sphere()
            {
                center = new Vector3(0, -4, 8) * LG.Unit,
                radius = 2 * LG.Unit,
                radius_square = (float)Math.Pow(2 * LG.Unit, 2),
                color = new Vector3(255, 0, 0),
                specular = 200,
                reflection = 0.4f
            };

        Sphere sphere3 =
            new Sphere()
            {
                center = new Vector3(0, 3, 6) * LG.Unit,
                radius = 1 * LG.Unit,
                radius_square = (float)Math.Pow(1 * LG.Unit, 2),
                color = new Vector3(125, 125, 125),
                specular = 600,
                reflection = 0.4f
            };

        Sphere blueSphere =
            new Sphere()
            {
                center = new Vector3(5, 2, 6) * LG.Unit,
                radius = 2 * LG.Unit,
                radius_square = (float)Math.Pow(2 * LG.Unit, 2),
                color = new Vector3(0, 0, 255),
                specular = 100,
                reflection = 0.8f
            };

        Sphere greenSphere =
            new Sphere()
            {
                center = new Vector3(-6, 2, 14) * LG.Unit,
                radius = 4 * LG.Unit,
                radius_square = (float)Math.Pow(4 * LG.Unit, 2),
                color = new Vector3(0, 255, 0),
                specular = 600,
                reflection = 0.6f
            };

        Light globalLight = new GlobalLight() { intensity = 0.3f, position = new Vector3(0, 0, 12) * LG.Unit };
        Light directionLight = new DirectionalLight() { intensity = 0.5f, direction = new Vector3(2, -6, 0) * LG.Unit, position = new Vector3(2, 6, 8) * LG.Unit };
        Light pointLight = new PointLight() { intensity = 0.6f, position = new Vector3(-3, 7, 7) * LG.Unit };


        public void EquipBoundaries()
        {
            GenerateBoundaries();
            GenerateXMinArray();
        }

        //根据球体生成边界体结构
        public void GenerateBoundaries()
        {
            for (int i = 0; i < sphereCount; i++)
            {
                Sphere sphere = spheres[i];
                Vector3 center = new Vector3(sphere.center.X*RayTrace.canvas.CanvasDistance/sphere.center.Z, sphere.center.Y*RayTrace.canvas.CanvasDistance/sphere.center.Z, RayTrace.canvas.CanvasDistance);
                float length = 2 * sphere.radius * RayTrace.canvas.CanvasDistance / sphere.center.Z;
                //由于ViewPort的缩放，需要对边界体进行从Canvas到ViewPort的缩放
                center *= RayTrace.viewPort._scale;
                length *= RayTrace.viewPort._scale;
                boundaries[i] = new Boundary(center, length, sphere);
            }
        }

        public void GenerateXMinArray()
        {
            for (int i = 0; i < sphereCount; i++)
            {
                minXArray[i] = boundaries[i].x_min;
            }
            //对最小X坐标进行排序
            Array.Sort(minXArray);
        }
    }
}
