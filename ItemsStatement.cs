using System.Numerics;

namespace RayTraceApplication
{
    public class Canvas
    {//代表Canvas长度和宽度的像素的个数
        public int CanvasHeight { get; }
        public int CanvasWidth { get; }

        public int CanvasDistance { get; }
        public Canvas(int height = 3*LG.Unit, int width = 4 * LG.Unit, int distance = 2 * LG.Unit) //默认构造函数构造为3*4的标准单位画布
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

    //定义边界体结构,这里暂时都定义为正方形，并且不考虑Z轴，这里边界体结构直接生成在Canvas上
    public class Boundary
    {
        public Vector3 center { get; set; } //边界体的中心世界坐标
        public float length { get; set; } //边界体的边长
        public float x_min { get; set; }
        public float x_max { get; set; }
        public float y_min { get; set; }
        public float y_max { get; set; }
        public Sphere sphere { get; set; } //边界体内的球体
        public Boundary(Vector3 centerPosition, float length, Sphere sphere)
        {
            this.center = centerPosition;
            this.length = length;
            this.sphere = sphere;
            x_min = center.X - length / 2;
            x_max = center.X + length / 2;
            y_min = center.Y - length / 2;
            y_max = center.Y + length / 2;
        }
    }


    public class Sphere
    {
        public Vector3 center { get; set; } //球心
        public float radius { get; set; } //半径

        public float radius_square { get; set; } //半径平方
        public Vector3 color { get; set; } //球体颜色

        public float specular { get; set; } //锐利度

        public float reflection { get; set; } //反射度
    }

    public class Color
    {
        public Color(int _r, int _g, int _b)
        {
            color = new Vector3(_r, _g, _b); 
        }

        public static Vector3 operator *(double n, Color c) { return new Vector3((float)(c.color.X * n), (float)(c.color.Y * n), (float)(c.color.Z * n)); } //定义向量数乘

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

     public abstract class Light //光源
    {
        public Vector3 position { get; set; } //灯的位置

        public float intensity { get; set; } //灯的强度

        public abstract string type { get; } //灯的类型
    }

    public class PointLight : Light //点光源
    {
        public override string type { get; }
        public PointLight()
        {
            this.type = "Point";
        }
    }

    public class GlobalLight : Light //全局光照
    {
        public override string type { get; }
        public GlobalLight()
        {
            this.type = "Global";
        }
    }

    public class DirectionalLight : Light //方向光照
    {
        public Vector3 direction { get; set; }

        public override string type { get; }
        public DirectionalLight()
        {
            this.type = "Direction";
        }
    }
}
