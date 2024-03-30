using System;
using System.Numerics;

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
        public static System.Drawing.Color backgroundColor = System.Drawing.Color.Black;
        public Environment()//初始化场景
        {
            spheres[0] = redSphere;
            spheres[1] = blueSphere;
            spheres[2] = greenSphere;
            spheres[3] = sphere3;

            lights[0] = globalLight;
            lights[1] = directionLight;
            lights[2] = pointLight;
        }

        public readonly Sphere[] spheres = new Sphere[4];
        public readonly Light[] lights = new Light[3];

        Sphere redSphere =
            new Sphere()
            {
                center = new Vector3(0, -4, 8)*LG.Unit,
                radius = 2 * LG.Unit,
                radius_square = (float)Math.Pow(2 * LG.Unit, 2),
                color = new Vector3(255, 0, 0),
                specular = 200,
                reflection = 0.4
            };        
        
        Sphere sphere3 =
            new Sphere()
            {
                center = new Vector3(0, 3, 6)*LG.Unit,
                radius = 1 * LG.Unit,
                radius_square = (float)Math.Pow(1 * LG.Unit, 2),
                color = new Vector3(125, 125, 125),
                specular = 600,
                reflection = 0.4
            };

        Sphere blueSphere =
            new Sphere()
            {
                center = new Vector3(5, 2, 6) * LG.Unit,
                radius = 2 * LG.Unit,
                radius_square = (float)Math.Pow(2 * LG.Unit, 2),
                color = new Vector3(0, 0, 255),
                specular = 100,
                reflection = 0.8
            };

        Sphere greenSphere =
            new Sphere()
            {
                center = new Vector3(-6, 2, 14) * LG.Unit,
                radius = 4 * LG.Unit,
                radius_square = (float)Math.Pow(4 * LG.Unit, 2),
                color = new Vector3(0, 255, 0),
                specular = 600,
                reflection = 0.6
            };

        Light globalLight = new GlobalLight() { intensity = 0.3, position = new Vector3(0, 0, 12) * LG.Unit };
        Light directionLight = new DirectionalLight() { intensity = 0.5, direction = new Vector3(2, -6, 0) * LG.Unit, position = new Vector3(2, 6, 8) * LG.Unit };
        Light pointLight = new PointLight() { intensity = 0.6, position = new Vector3(-3, 7, 7) * LG.Unit };

    }
}
