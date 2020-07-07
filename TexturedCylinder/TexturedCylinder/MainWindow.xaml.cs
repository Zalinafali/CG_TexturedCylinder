using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;
using Rectangle = System.Drawing.Rectangle;

namespace TexturedCylinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap bmp = null;
        BitmapImage ibmp = null;

        Bitmap texture = null;
        bool usingTexture = false;

        int width = 1000;
        int height = 800;

        float sx = 1000;
        float sy = 800;

        // target position*
        Vector3 cTarget;

        // camera position
        Vector3 cPos;
        Vector3 cUp;
        double FoV = Math.PI / 3;
        double angX = 0;
        double angY = 0;
        float scale = 1;

        Cylinder cylinder;

        double h = 100;
        double r = 50;

        // Rotation options
        double theta = 5 * Math.PI / 180;

        Vector4 axisX = new Vector4(500, 0, 0, 1);
        Vector4 axisY = new Vector4(0, 500, 0, 1);
        Vector4 axisZ = new Vector4(0, 0, 500, 1);

        public MainWindow()
        {
            InitializeComponent();

            cTarget = new Vector3(sx/2, sy/2, 0);
            cPos = new Vector3(0, 0, 0);
            cUp = new Vector3(0, 1, 0);

            cylinder = new Cylinder(h, r);

            Draw();
        }

        private Bitmap CreateNewBitmap(int x, int y)
        {
            Bitmap nbmp = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(nbmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(System.Drawing.Brushes.LightGray, ImageSize);
            }

            return nbmp;
        }

        private void ShowBitmap()
        {
            ibmp = Bitmap2BitmapImage(bmp);
            image.Source = ibmp;
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private int MainCordX(int x)
        {
            //return (int)sx/2 + x;
            return x;
        }

        private int MainCordY(int y)
        {
            //return (int)sy/2 + y;
            return y;
        }

        private Matrix4x4 Model()
        {
            Matrix4x4 M = new Matrix4x4( 1, 0, 0, sx/2,
                                         0, 1, 0, sy/2,
                                         0, 0, 1, 0,
                                         0, 0, 0, 1);
            return M;
        }

        private Matrix4x4 RotationX()
        {
            Matrix4x4 M = new Matrix4x4(1, 0, 0, 0,
                                        0, (float)Math.Cos(angX), (float)(-Math.Sin(angX)), 0,
                                        0, (float)Math.Sin(angX), (float)Math.Cos(angX), 0,
                                        0, 0, 0, 1
                                        );
            return M;
        }

        private Matrix4x4 RotationY()
        {
            Matrix4x4 M = new Matrix4x4((float)Math.Cos(angY), 0, (float)Math.Sin(angY), 0,
                                        0, 1, 0, 0,
                                        (float)(-Math.Sin(angY)), 0, (float)Math.Cos(angY), 0,
                                        0, 0, 0, 1
                                        );
            return M;
        }

        private Matrix4x4 Scaling()
        {
            Matrix4x4 M = new Matrix4x4(scale, 0, 0, 0,
                                         0, scale, 0, 0,
                                         0, 0, scale, 0,
                                         0, 0, 0, 1);
            return M;
        }

        private Matrix4x4 Trans1()
        {
            Matrix4x4 M = new Matrix4x4( 1, 0, 0, 0,
                                         0, 1, 0, -50,
                                         0, 0, 1, 0,
                                         0, 0, 0, 1);
            return M;
        }

        private Matrix4x4 CameraView()
        {
            var tZ = Vector3.Subtract(cPos, cTarget);
            Vector3 cZ = Vector3.Divide(tZ, tZ.Length());

            var tX = Vector3.Cross(cUp, cZ);
            Vector3 cX = Vector3.Divide(tX, tX.Length());

            var tY = Vector3.Cross(cZ, cX);
            Vector3 cY = Vector3.Divide(tY, tY.Length());

            Matrix4x4 M = new Matrix4x4 (   cX.X, cX.Y, cX.Z, Vector3.Dot(cX, cPos),
                                            cY.X, cY.Y, cY.Z, Vector3.Dot(cY, cPos),
                                            cZ.X, cZ.Y, cZ.Z, Vector3.Dot(cZ, cPos),
                                               0,    0,    0,                    1);
            return M;
        }

        private Matrix4x4 PerspectiveProjection()
        {
            Matrix4x4 M = new Matrix4x4((float)(-sx / 2 / Math.Tan(FoV / 2)), 0, (float)(sx / 2), 0,
                                        0, (float)(sx / 2 / Math.Tan(FoV / 2)), (float)(sy / 2), 0,
                                        0, 0, 0, 1,
                                        0, 0, 1, 0  );
            return M;
        }

        private Matrix4x4 CreateTransformationMatrix()
        {
            Matrix4x4 model = Model();
            Matrix4x4 CVM = CameraView();
            Matrix4x4 PPM = PerspectiveProjection();

            Matrix4x4 rotX = RotationX();
            Matrix4x4 rotY = RotationY();
            Matrix4x4 scaling = Scaling();
            Matrix4x4 trans1 = Trans1();

            Matrix4x4 M = new Matrix4x4(1, 0, 0, 0,
                                          0, 1, 0, 0,
                                          0, 0, 1, 0,
                                          0, 0, 0, 1);

            M = Matrix4x4.Multiply(M, PPM);
            M = Matrix4x4.Multiply(M, CVM);
            M = Matrix4x4.Multiply(M, model);
            M = Matrix4x4.Multiply(M, rotY);
            M = Matrix4x4.Multiply(M, rotX);
            M = Matrix4x4.Multiply(M, scaling);
            M = Matrix4x4.Multiply(M, trans1);

            return M;
        }

        private void Draw()
        {
            Bitmap nbmp = CreateNewBitmap(width, height);

            // implement drawing

            nbmp = DrawCylinderVertices(nbmp);
            nbmp = DrawCylinderTriangles(nbmp);

            bmp = nbmp;
            ShowBitmap();
        }

        private void TestDraw()
        {
            Bitmap nbmp = CreateNewBitmap(width, height);

            // implement drawing

            Matrix4x4 M = CreateTransformationMatrix();

            for (int i = 0; i < 4 * cylinder.n + 2; i++)
            {
                //System.Windows.Forms.MessageBox.Show(cylinder.vertices[i].pv.X + "\n" +
                //                                     cylinder.vertices[i].pv.Y + "\n" +
                //                                     cylinder.vertices[i].pv.Z + "\n" +
                //                                     cylinder.vertices[i].pv.W + "\n");

                //cylinder.vertices[i].pv = Vector4.Transform(cylinder.vertices[i].pv, M);
                cylinder.vertices[i].pv = MyMultiply(cylinder.vertices[i].pv, M);

                //System.Windows.Forms.MessageBox.Show(cylinder.vertices[i].pv.X + "\n" +
                //                                     cylinder.vertices[i].pv.Y + "\n" +
                //                                     cylinder.vertices[i].pv.Z + "\n" +
                //                                     cylinder.vertices[i].pv.W + "\n");

                cylinder.vertices[i].pv /= cylinder.vertices[i].pv.W;

                //System.Windows.Forms.MessageBox.Show(cylinder.vertices[i].pv.X + "\n" +
                //                                     cylinder.vertices[i].pv.Y + "\n" +
                //                                     cylinder.vertices[i].pv.Z + "\n" +
                //                                     cylinder.vertices[i].pv.W + "\n");

                cylinder.vertices[i].pv.CopyTo(cylinder.vertices[i].p);
            }
            cylinder.CreateTriangles();

            nbmp = DrawCylinderVertices(nbmp);
            nbmp = DrawCylinderTriangles(nbmp);

            Vector4 aX = MyMultiply(axisX, M);
            Vector4 aY = MyMultiply(axisY, M);
            Vector4 aZ = MyMultiply(axisZ, M);
            DrawAxis(nbmp, aX, aY, aZ);

            cylinder = new Cylinder(h, r);

            bmp = nbmp;
            ShowBitmap();
        }

        private Bitmap DrawCylinderVertices(Bitmap cbmp)
        {
            System.Drawing.Color color = System.Drawing.Color.Black;

            int x, y;
            //int i = 0;

            foreach (var vertex in cylinder.vertices)
            {
                x = MainCordX((int)vertex.p[0]);
                y = MainCordY((int)vertex.p[1]);

                //System.Windows.Forms.MessageBox.Show("x: " + x + " y: " + y + "\n i: " + ++i);

                if (x > 0 && x < width && y > 0 && y < height)
                {
                    cbmp.SetPixel(x, y, color);
                }
            }

            return cbmp;
        }

        private Bitmap DrawCylinderTriangles(Bitmap cbmp)
        {
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.LightSeaGreen);

            System.Drawing.Point[] trianglePoints = new System.Drawing.Point[3];

            foreach(var triangle in cylinder.triangles)
            {
                if (BackFaceCulling(triangle))
                {
                    trianglePoints[0] = new System.Drawing.Point(MainCordX((int)triangle.vertices[0].pv.X), MainCordY((int)triangle.vertices[0].pv.Y));
                    trianglePoints[1] = new System.Drawing.Point(MainCordX((int)triangle.vertices[1].pv.X), MainCordY((int)triangle.vertices[1].pv.Y));
                    trianglePoints[2] = new System.Drawing.Point(MainCordX((int)triangle.vertices[2].pv.X), MainCordY((int)triangle.vertices[2].pv.Y));

                    using (var graphics = Graphics.FromImage(cbmp))
                    {
                        graphics.DrawLines(pen, trianglePoints);
                    }

                    if (usingTexture)
                    {
                        Matrix4x4 M = CreateTransformationMatrix();

                        //FillTriangle(triangle.vertices.ToList(), cbmp);
                        FillPatternTriangle(cbmp, triangle, M);

                        //MessageBox.Show(triangle.vertices[0].pv.X + "   " + triangle.vertices[0].pv.Y + "\n"
                        //                + triangle.vertices[1].pv.X + "   " + triangle.vertices[1].pv.Y + "\n"
                        //                        + triangle.vertices[2].pv.X + "   " + triangle.vertices[2].pv.Y);
                    }
                }
            }

            return cbmp;
        }

        private bool BackFaceCulling(Triangle t)
        {
            Vector3 v1 = new Vector3(t.vertices[1].pv.X - t.vertices[0].pv.X,
                                     t.vertices[1].pv.Y - t.vertices[0].pv.Y,
                                                                           0);

            Vector3 v2 = new Vector3(t.vertices[2].pv.X - t.vertices[0].pv.X,
                                     t.vertices[2].pv.Y - t.vertices[0].pv.Y,
                                                                           0);

            Vector3 res = Vector3.Cross(v1, v2);

            if (res.Z < 0)
                return true;
            else
                return false;
        }

        private Bitmap DrawAxis(Bitmap cbmp, Vector4 x, Vector4 y, Vector4 z)
        {
            System.Drawing.Pen penX = new System.Drawing.Pen(System.Drawing.Color.Orange);
            System.Drawing.Pen penY = new System.Drawing.Pen(System.Drawing.Color.Blue);
            System.Drawing.Pen penZ = new System.Drawing.Pen(System.Drawing.Color.Green);

            using (var graphics = Graphics.FromImage(cbmp))
            {
                graphics.DrawLine(penX, MainCordX((int)sx/2), MainCordY((int)sy/2), x.X, x.Y);
                graphics.DrawLine(penY, MainCordX((int)sx / 2), MainCordY((int)sy / 2), y.X, y.Y);
                graphics.DrawLine(penZ, MainCordX((int)sx / 2), MainCordY((int)sy / 2), z.X, z.Y);
            }
            return cbmp;
        }

        // Controls

        private void LoadTexture()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.bmp;*.jpg)|*.png;*.bmp;*.jpg|All files (*.*)|*.*";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo file = new FileInfo(openFileDialog.FileName);
                if (file.Extension == ".PNG" || file.Extension == ".JPG" || file.Extension == ".jpg" || file.Extension == ".png" || file.Extension == ".bmp" || file.Extension == ".BMP")
                {
                    texture = new Bitmap(file.FullName);
                }
            }
        }

        private void MouseWheel_Scaling(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //float s = 1.1f;
                //cylinder.Scaling(s, s, s);

                scale += 0.1f;

                TestDraw();
            }
            else if (e.Delta < 0)
            {
                //float s = 0.9f;
                //cylinder.Scaling(s, s, s);

                scale -= 0.1f;

                TestDraw();
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            CallKeyDown(e);
        }

        public void CallKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            //MessageBox.Show("Button down: " + e.Key.ToString());

            switch (e.Key)
            {
                case Key.A:
                    //cylinder.RotatingY(theta);
                    angY += theta;
                    TestDraw();
                    break;

                case Key.D:
                    //cylinder.RotatingY(-theta);
                    angY -= theta;
                    TestDraw();
                    break;

                case Key.W:
                    //cylinder.RotatingX(theta);
                    angX += theta;
                    TestDraw();
                    break;

                case Key.S:
                    //cylinder.RotatingX(-theta);
                    angX -= theta;
                    TestDraw();
                    break;

                default:
                    break;
            }
        }

        private void FillPatternTriangle(Bitmap cbmp, Triangle t, Matrix4x4 M)
        {
            int N = t.vertices.Length;
            var P = t.vertices.ToList();

            List<ActiveEdge> AET = new List<ActiveEdge>();

            int[] indices = new int[N];
            for (int j = 0; j < N; j++)
                indices[j] = j;

            indices = BubbleSortVertices(indices, P);

            int k = 0;
            int i = indices[k];

            int y = (int)P[indices[0]].pv.Y;

            int ymin = (int)P[indices[0]].pv.Y;
            int ymax = (int)P[indices[N - 1]].pv.Y;

            while (y != ymax)
            {
                while ((int)P[i].pv.Y == y)
                {
                    if (P[((i - 1) % N + N) % N].pv.Y > P[i].pv.Y)
                    {
                        ActiveEdge newEdge = new ActiveEdge(P[((i - 1) % N + N) % N].pv, P[i].pv, P[((i - 1) % N + N) % N], P[i]);
                        if (!newEdge.horizontal)
                            AET.Add(newEdge);
                    }
                    if (P[((i + 1) % N + N) % N].pv.Y > P[i].pv.Y)
                    {
                        ActiveEdge newEdge = new ActiveEdge(P[((i + 1) % N + N) % N].pv, P[i].pv, P[((i + 1) % N + N) % N], P[i]);
                        if (!newEdge.horizontal)
                            AET.Add(newEdge);
                    }

                    ++k;
                    i = indices[k];
                }

                AET = AET.OrderBy(e => e.x).ToList();

                if (y != ymin)
                {
                    for (int j = 0; j < AET.Count; j += 2)
                    {
                        if (j + 1 < AET.Count)
                        {
                            for (int x = (int)AET[j].x + 1; x < (int)AET[j + 1].x; x++)
                            {
                                //Vector2 textureCoord = TextureCoordToTexturePixel()

                                try
                                {
                                    cbmp.SetPixel(x, y, texture.GetPixel(dx, dy));
                                }
                                catch
                                {
                                    if (x < 0)
                                        x *= -1;
                                    cbmp.SetPixel(x % width, y, System.Drawing.Color.Red);
                                    MessageBox.Show("Error drawing");
                                    //MessageBox.Show(AET[j].x + " " + AET[j].ymax + "\n" + AET[j+1].x + " " + AET[j+1].ymax);
                                    //MessageBox.Show(AET[j].up.pv.X + " " + AET[j].up.pv.Y + "\n" +
                                    //                AET[j].down.pv.X + " " + AET[j].down.pv.Y + "\n" +
                                    //                AET[j+1].up.pv.X + " " + AET[j+1].up.pv.Y + "\n" +
                                    //                AET[j+1].down.pv.X + " " + AET[j+1].down.pv.Y);

                                }
                            }
                        }
                    }
                }

                y++;

                AET.RemoveAll(e => e.ymax == y);

                foreach (var edge in AET)
                {
                    edge.x += edge.m;
                }
            }
        }

        private int[] BubbleSortVertices(int[] indices, List<Vertex> points)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                for (int j = 0; j < indices.Length - 1; j++)
                {
                    if (points[indices[j]].pv.Y > points[indices[j + 1]].pv.Y)
                    {
                        int temp = indices[j];
                        indices[j] = indices[j + 1];
                        indices[j + 1] = temp;
                    }
                }
            }
            return indices;
        }

        private void FillTriangle(List<Vertex> vert, Bitmap cbmp)
        {
 
            int[] indices = new int[vert.Count];
            List<Vertex> tmp = new List<Vertex>();
            foreach (var v in vert)
            {
                tmp.Add(v);
            }
            tmp.Sort((a, b) => a.pv.Y.CompareTo(b.pv.Y));
            for (int j = 0; j < tmp.Count; j++)
                indices[j] = vert.IndexOf(vert.Find(x => x == tmp[j]));
            List<Tuple<int, double, double>> AET = new List<Tuple<int, double, double>>();
            int k = 0;
            int i = indices[0];
            int y, ymin, ymax, xmin;
            y = ymin = (int)vert[indices[0]].pv.Y;
            ymax = (int)vert[indices[vert.Count - 1]].pv.Y;
            xmin = (int)vert.OrderBy(p => p.pv.X).First().pv.X;

            while (y<ymax)
            {
                while ((int) vert[i].pv.Y == y)
                {
                    if (i > 0)
                    {
                        if (vert[i - 1].pv.Y > vert[i].pv.Y)
                            AET.Add(new Tuple<int, double, double>((int) Math.Max(vert[i - 1].pv.Y, vert[i].pv.Y), Low(vert[i - 1], vert[i]).pv.X,
                                                                (double) (vert[i - 1].pv.X - vert[i].pv.X) / (vert[i - 1].pv.Y - vert[i].pv.Y)));
                    }
                    else
                    {
                        if (vert[vert.Count - 1].pv.Y > vert[i].pv.Y)
                            AET.Add(new Tuple<int, double, double>((int) Math.Max(vert[vert.Count - 1].pv.Y, vert[i].pv.Y), Low(vert[vert.Count - 1], vert[i]).pv.X,
                                                                (double) (vert[vert.Count - 1].pv.X - vert[i].pv.X) / (vert[vert.Count - 1].pv.Y - vert[i].pv.Y)));
                    }
                    if (i<vert.Count - 1)
                    {
                        if (vert[i + 1].pv.Y > vert[i].pv.Y)
                            AET.Add(new Tuple<int, double, double>((int) Math.Max(vert[i + 1].pv.Y, vert[i].pv.Y), Low(vert[i + 1], vert[i]).pv.X,
                                                                (double) (vert[i + 1].pv.X - vert[i].pv.X) / (vert[i + 1].pv.Y - vert[i].pv.Y)));
                    }
                    else
                    {
                        if (vert[0].pv.Y > vert[i].pv.Y)
                            AET.Add(new Tuple<int, double, double>((int) Math.Max(vert[0].pv.Y, vert[i].pv.Y), Low(vert[0], vert[i]).pv.X,
                                                                (double) (vert[0].pv.X - vert[i].pv.X) / (vert[0].pv.Y - vert[i].pv.Y)));
                    }
                    ++k;
                    i = indices[k];
                }

                AET.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                for (int j = 0; j<AET.Count; j += 2)
                {
                    if (j + 1 < AET.Count)
                    {
                        if ((int) AET[j].Item2 >10)
                        {
                            for (int x = (int) AET[j].Item2; x <= (int) AET[j + 1].Item2; x++)
                            {
                                cbmp.SetPixel(x, y, texture.GetPixel(x, y));
                            }
                        }
                    }
                }
                y++;
                AET.RemoveAll(x => x.Item1 == y);
                for (int j = 0; j<AET.Count; j++)
                    AET[j] = new Tuple<int, double, double>(AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
            }
        }

        private Vertex Low(Vertex a, Vertex b)
        {
            if (a.pv.Y > b.pv.Y)
                return b;
            else
                return a;
        }

        private Vector2 TextureCoordToTexturePixel(Vector2 v)
        {
            Vector2 res = new Vector2(v.X * texture.Width, v.Y * texture.Height);

            return res;
        }

        // additional functions
        private Vector4 MyMultiply(Vector4 self, Matrix4x4 matrix)
        {
            return new Vector4(
                matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14 * self.W,
                matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24 * self.W,
                matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34 * self.W,
                matrix.M41 * self.X + matrix.M42 * self.Y + matrix.M43 * self.Z + matrix.M44 * self.W
            );
        }

        // Test buttons

        private void TestVerticesButton_Click(object sender, RoutedEventArgs e)
        {
            TestDraw();
        }

        private void RotatingLeftTestButton_Click(object sender, RoutedEventArgs e)
        {
            //cylinder.RotatingY(theta);
            angY += theta;

            TestDraw();
        }

        private void RotatingRightTestButton_Click(object sender, RoutedEventArgs e)
        {
            //cylinder.RotatingY(-theta);
            angY -= theta;

            TestDraw();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            cylinder = new Cylinder(h,r);
            angX = 0;
            angY = 0;
            scale = 1;

            TestDraw();
        }

        private void RotatingUpTestButton_Click(object sender, RoutedEventArgs e)
        {
            angX += theta;

            TestDraw();
        }

        private void RotatingDownTestButton_Click(object sender, RoutedEventArgs e)
        {
            angX -= theta;

            TestDraw();
        }

        private void ZoomInTestButton_Click(object sender, RoutedEventArgs e)
        {
            scale += 0.1f;

            TestDraw();
        }

        private void ZoomOutTestButton_Click(object sender, RoutedEventArgs e)
        {
            scale -= 0.1f;

            TestDraw();
        }

        private void LoadTextureTestButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTexture();
        }

        private void UseTextureTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (usingTexture)
                usingTexture = false;
            else
            {
                usingTexture = true;
            }

            TestDraw();
        }
    }
}
