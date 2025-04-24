using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace SphereDispDemo
{
    struct Vector3
    {
        public double X, Y, Z;
        public Vector3(double x, double y, double z) { X = x; Y = y; Z = z; }

        // 绕X轴旋转
        public Vector3 RotateX(double pitchAngle)
        {
            double cos = Math.Cos(pitchAngle), sin = Math.Sin(pitchAngle);
            return new Vector3(X, Y * cos - Z * sin, Y * sin + Z * cos);
        }

        // 绕Y轴旋转
        public Vector3 RotateY(double yawAngle)
        {
            double cos = Math.Cos(yawAngle), sin = Math.Sin(yawAngle);
            return new Vector3(X * cos + Z * sin, Y, -X * sin + Z * cos);
        }
    }


    public partial class Form1 : Form
    {
        
        private float yaw = 0f;      // 初始观察位置的经度，即水平旋转角度
        private float pitch = 0f;    // 初始观察位置的纬度，即垂直旋转角度
        private Point lastMousePos; // 鼠标上次位置
        private bool isDragging = false;


        public Form1()
        {
            this.DoubleBuffered = true; // 减少闪烁

            InitializeComponent();

            this.MouseDown += Form1_MouseDown;
            this.MouseUp += Form1_MouseUp;
            this.MouseMove += Form1_MouseMove;
        }

        // 投影函数：3D 点 → 2D 屏幕坐标
        PointF Project(Vector3 v, float scale, Size clientSize)
        {
            // 这里的 scale 是一个缩放因子，用于将三维坐标转换为二维屏幕坐标
            // 可以理解为观察者到球体的距离远近
            // clientSize 是窗口的大小，用于计算屏幕坐标
            float x = (float)(v.X * scale) + clientSize.Width / 2;
            float y = (float)(-v.Y * scale) + clientSize.Height / 2;
            return new PointF(x, y);
        }







        // 重载绘图函数
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);    // 为什么要调用基类的 OnPaint 方法
                                // 因为基类的 OnPaint 方法可能会执行一些必要的初始化或清理工作。
                                // 否则，可能会导致绘图不完整或出现异常。
            Graphics g = e.Graphics;
            g.Clear(Color.Black);   //黑色背景
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // 抗锯齿
            //这是为了让绘制的图形更加平滑，减少锯齿状边缘的出现。

            // float radius = 1f;  // 球体半径，此处不使用
            int latStep = 10;   // 纬度步进（度）
            int lonStep = 10;   // 经度步进（度）
            float scale = Math.Min(this.ClientSize.Width, this.ClientSize.Height) * 0.4f;
            // scale 是一个缩放因子，用于将三维坐标转换为二维屏幕坐标
            // 右侧算式的意思是：取窗口的宽度和高度中的最小值，乘以 0.4f

            Pen pen = new Pen(Color.Green, 1);


            // 绘制纬线
            for (int lat = -80; lat <= 80; lat += latStep)
            {
                // 新建一个 List<PointF> 用于存储当前纬线上采样的点
                List<PointF> line = new List<PointF>();
                // 将循环指示变量lat的值转换为弧度
                double latRad = Math.PI * lat / 180;
                // 内层循环，在当前纬线上采样很多个点，有多少采样取决于 lonStep
                for (int lon = -180; lon <= 180; lon += lonStep)
                {
                    // 将循环指示变量lon的值转换为弧度
                    double lonRad = Math.PI * lon / 180;
                    // 计算球面上的点的坐标，使用了简单的球坐标系下的映射公式
                    double x = Math.Cos(latRad) * Math.Cos(lonRad);
                    double y = Math.Sin(latRad);
                    double z = Math.Cos(latRad) * Math.Sin(lonRad);
                    // 新建一个 Vector3 结构体实例，表示球面上的点
                    // 这里的链式调用的意义是：先将球面上的点绕X轴旋转 pitch 弧度，再绕Y轴旋转 yaw 弧度
                    var v = new Vector3(x, y, z).RotateX(pitch).RotateY(yaw);
                    line.Add(Project(v, scale, this.ClientSize));
                }
                // 将所有采样的点两两连接起来，因为样本很多，看起来就是弧线
                g.DrawLines(pen, line.ToArray());
            }


            // 绘制经线，同理
            for (int lon = -180; lon < 180; lon += lonStep)
            {
                List<PointF> line = new List<PointF>();
                for (int lat = -90; lat <= 90; lat += latStep)
                {
                    double latRad = Math.PI * lat / 180;
                    double lonRad = Math.PI * lon / 180;
                    double x = Math.Cos(latRad) * Math.Cos(lonRad);
                    double y = Math.Sin(latRad);
                    double z = Math.Cos(latRad) * Math.Sin(lonRad);
                    var v = new Vector3(x, y, z).RotateX(pitch).RotateY(yaw);
                    line.Add(Project(v, scale, this.ClientSize));
                }
                g.DrawLines(pen, line.ToArray());
            }
        }



        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            lastMousePos = e.Location;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;

                // 控制灵敏度
                yaw += dx * 0.01f;
                pitch += dy * 0.01f;

                // 限制pitch范围 [-π/2, π/2]
                pitch = Math.Max(-1.5f, Math.Min(1.5f, pitch));

                lastMousePos = e.Location;
                this.Invalidate(); // 触发重绘
            }
        }

    }
}
