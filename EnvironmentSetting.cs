using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTraceApplication
{
    public static class LG
    {
        public const int Unit = 1;
        public const int PixelPerUnit = 250;
        public const int Max_depth = 3;
        public const int BACKR = 0;
        public const int BACKG = 0;
        public const int BACKB = 0;
        public const int Total_area = 5;
        public static Vector3 Org = new Vector3(0, 0, 0);//定义坐标原点

    }

    public class Environment
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
                color = new Vector3(255, 0, 0),
                specular = 200,
                reflection = 0.4
            };        
        
        Sphere sphere3 =
            new Sphere()
            {
                center = new Vector3(0, 3, 6)*LG.Unit,
                radius = 1 * LG.Unit,
                color = new Vector3(125, 125, 125),
                specular = 600,
                reflection = 0.4
            };

        Sphere blueSphere =
            new Sphere()
            {
                center = new Vector3(5, 2, 6) * LG.Unit,
                radius = 2 * LG.Unit,
                color = new Vector3(0, 0, 255),
                specular = 100,
                reflection = 0.8
            };

        Sphere greenSphere =
            new Sphere()
            {
                center = new Vector3(-6, 2, 14) * LG.Unit,
                radius = 4 * LG.Unit,
                color = new Vector3(0, 255, 0),
                specular = 600,
                reflection = 0.6
            };

        Light globalLight = new GlobalLight() { intensity = 0.3, position = new Vector3(0, 0, 12) * LG.Unit };
        Light directionLight = new DirectionalLight() { intensity = 0.5, direction = new Vector3(2, -6, 0) * LG.Unit, position = new Vector3(2, 6, 8) * LG.Unit };
        Light pointLight = new PointLight() { intensity = 0.6, position = new Vector3(-3, 7, 7) * LG.Unit };

    }
}
