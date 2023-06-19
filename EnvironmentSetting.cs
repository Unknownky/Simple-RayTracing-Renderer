using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTraceApplication
{
    public class Environment
    {
        public static System.Drawing.Color backgroundColor = System.Drawing.Color.Black;
        public Environment()//初始化场景
        {
            spheres[0] = redSphere;
            spheres[1] = blueSphere;
            spheres[2] = yellowSphere;

            lights[0] = globalLight;
            lights[1] = directionLight;
            lights[2] = pointLight;
        }

        public readonly Sphere[] spheres = new Sphere[3];
        public readonly Light[] lights = new Light[3];

        Sphere redSphere =
            new Sphere()
            {
                center = new Vector3(0, 0, 800),
                radius = 300,
                color = new Vector3(255, 0, 0),
                specular = 200,
                reflection = 100
            };

        Sphere blueSphere =
            new Sphere()
            {
                center = new Vector3(500, 500, 800),
                radius = 300,
                color = new Vector3(0, 0, 255),
                specular = 100,
                reflection = 2
            };

        Sphere yellowSphere =
            new Sphere()
            {
                center = new Vector3(-600, 200, 1000),
                radius = 320,
                color = new Vector3(0, 255, 0),
                specular = 600,
                reflection = 50
            };

        Light globalLight = new GlobalLight() { intensity = 0.2, position = new Vector3(0, 0, 1200) };
        Light directionLight = new DirectionalLight() { intensity = 0.3, direction = new Vector3(0, -600, 0), position = new Vector3(200, 600, 800) };
        Light pointLight = new PointLight() { intensity = 0.5, position = new Vector3(-300, 700, 800) };

    }
}
