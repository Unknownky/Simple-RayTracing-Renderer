using System.Numerics;

namespace RayTraceApplication
{
    public class Canvas
    {//代表Canvas长度和宽度的像素的个数
        public int CanvasHeight { get; }
        public int CanvasWidth { get; }

        public int CanvasDistance { get; }
        public Canvas(int height = 3*LG.Unit, int width = 4 * LG.Unit, int distance = 2 * LG.Unit)
        {
            CanvasHeight = height;
            CanvasWidth = width;
            CanvasDistance = distance;
        }
    }

    //首先两者必须确保比例一致
    public class ViewPort
    {//代表对应ViewPort在坐标系中的长度和宽度的像素的个数
        public int Width { get; }
        public int Height { get; }

        public int distance { get; }//单纯先作为距离，之后的Z坐标再考虑进行调整

        private int _scale;
        public ViewPort(Canvas canvas, int scale = 2)
        {
            _scale = scale > 1 ? scale : 2;
            Height = canvas.CanvasHeight*_scale;
            Width = canvas.CanvasWidth*_scale;
            distance = canvas.CanvasDistance * _scale;
        }
    }

    public class Sphere
    {
        public Vector3 center { get; set; }
        public float radius { get; set; }
        public Vector3 color { get; set; }

        public double specular { get; set; }

        public double reflection { get; set; }
    }

    public class Color
    {
        public Color(int _r, int _g, int _b)
        {
            color = new Vector3(_r, _g, _b);
        }

        public static Vector3 operator *(double n, Color c) { return new Vector3((float)(c.color.X * n), (float)(c.color.Y * n), (float)(c.color.Z * n)); }

        private Vector3 _color;
        public Vector3 color
        {
            get { return _color; }
            set
            {
                float x = value.X > 255 ? 255 : value.X;
                x = value.X < 0 ? 0 : value.X;
                float y = value.Y > 255 ? 255 : value.Y;
                y = value.Y < 0 ? 0 : value.Y;
                float z = value.Z > 255 ? 255 : value.Z;
                z = value.Z < 0 ? 0 : value.Z;
                _color = new Vector3((float)x, (float)y, (float)z);
            }
        }//用一个三维向量来保存RGB数据
    }

    abstract public class Light
    {
        public Vector3 position { get; set; }

        public double intensity { get; set; }

        public abstract string type { get; }
    }

    public class PointLight : Light
    {
        public override string type { get; }
        public PointLight()
        {
            this.type = "Point";
        }
    }

    public class GlobalLight : Light
    {
        public override string type { get; }
        public GlobalLight()
        {
            this.type = "Global";
        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 direction { get; set; }

        public override string type { get; }
        public DirectionalLight()
        {
            this.type = "Direction";
        }
    }
}
