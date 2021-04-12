using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Threading;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using Control = System.Windows.Forms.Control;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;
using CT3DMachine.Tools;

namespace CT3DMachine.View3D
{
    /// <summary>
    /// Interaction logic for GLView3D.xaml
    /// </summary>
    public partial class GLView3D : System.Windows.Controls.UserControl
    {
        private int frames;
        private GLControl glcontrol;
        private DateTime lastMeasureTime;
        private Renderer renderer;
        public int mTextId = 0;
        private int displayList = -1;
        public Point mRefPoint;
        public bool mouseDown = false;
        public double scaleRate = 1.0;

        public GLView3D()
        {
            InitializeComponent();
            renderer = new Renderer();
            this.lastMeasureTime = DateTime.Now;
            this.frames = 0;

            this.glcontrol = new GLControl();
            this.glcontrol.Load += this.GL_Load;
            this.glcontrol.Resize += this.GL_Resize;
            this.glcontrol.Paint += this.GlcontrolOnPaint;
            this.glcontrol.Dock = DockStyle.Fill;
            this.WDHost3D.Child = this.glcontrol;
            this.glcontrol.MouseDown += this.GL_MouseDown;
            this.glcontrol.MouseUp += this.GL_MouseUp;
            this.glcontrol.MouseWheel += this.GL_MouseWheel;
            this.glcontrol.MouseMove += this.GL_MouseMove;                        
        }
        private void GL_Load(object sender, EventArgs e)
        {
            this.glcontrol.MakeCurrent();
            mTextId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture3D, mTextId);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexImage3D<byte>(TextureTarget.Texture3D, 0, PixelInternalFormat.Rgba, renderer.mImageWidth, renderer.mImageHeight, renderer.mImageCount, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, renderer.mRGBAData);
            //GL.TexSubImage3D<byte>(TextureTarget.Texture3D, 0, 0, 0, 0, 256, 256, 109, PixelFormat.Rgba, PixelType.UnsignedByte, renderer.mRGBAData);
            GL.BindTexture(TextureTarget.Texture3D, 0);

            if (this.displayList <= 0)
            {
                this.displayList = GL.GenLists(1);
                GL.NewList(this.displayList, ListMode.Compile);

                GL.Begin(PrimitiveType.Quads);

                for (double fIndx = -1.0f; fIndx <= 1.0f; fIndx += 0.01f)
                {
                    GL.TexCoord3(0.0, 0.0, (fIndx + 1.0) / 2.0);
                    GL.Vertex3(-1.0, -1.0, fIndx);

                    GL.TexCoord3(1.0, 0.0, (fIndx + 1.0) / 2.0);
                    GL.Vertex3(1.0, -1.0, fIndx);

                    GL.TexCoord3(1.0, 1.0, (fIndx + 1.0) / 2.0);
                    GL.Vertex3(1.0, 1.0, fIndx);

                    GL.TexCoord3(0.0, 1.0, (fIndx + 1.0) / 2.0);
                    GL.Vertex3(-1.0, 1.0, fIndx);
                }

                GL.End();

                GL.EndList();
            }
            //GL.CallList(this.displayList);
        }
        private void GL_Resize(object sender, EventArgs e)
        {
            this.glcontrol.MakeCurrent();
            double wWidth = (double)this.glcontrol.Size.Width;
            double wHeight = (double)this.glcontrol.Size.Height;
            double aspectRatio = wWidth / wHeight;
            GL.Viewport(0, 0, (int)wWidth, (int)wHeight);
            GL.MatrixMode(MatrixMode.Projection);           
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void GlcontrolOnPaint(object sender, PaintEventArgs e)
        {
            this.glcontrol.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.05f);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();

            //Rotate and translate
            GL.Translate(0.5, 0.5, 0.5);
            GL.Scale(scaleRate, -scaleRate, (double)renderer.mImageWidth / (double)renderer.mImageCount * scaleRate);

            GL.MultMatrix(renderer.mdRotation);

            GL.Translate(-0.5, -0.5, -0.5);

            GL.Enable(EnableCap.Texture3DExt);
            GL.BindTexture(TextureTarget.Texture3D, mTextId);
            
            GL.CallList(this.displayList);

            this.glcontrol.SwapBuffers();
            this.frames++;
        }

        private void GL_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var mouseState = OpenTK.Input.Mouse.GetState();
            if (mouseState.IsButtonDown(OpenTK.Input.MouseButton.Left))
            {
                mRefPoint.X = e.X;
                mRefPoint.Y = e.Y;
                mouseDown = true;
            }
            else if (mouseState.IsButtonDown(OpenTK.Input.MouseButton.Middle))
            {
                Console.Write("\n=====> Mouse Middle Down");
            }
            else
            {
                Console.Write("\n=====> Mouse Right Down");
            }

        }

        private void GL_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var mouseState = OpenTK.Input.Mouse.GetState();
            if (mouseState.IsButtonUp(OpenTK.Input.MouseButton.Left))
            {
                mouseDown = false;
            }
            else if (mouseState.IsButtonUp(OpenTK.Input.MouseButton.Middle))
            {
                Console.Write("\n=====> Mouse Middle Up");
            }
            else
            {
                Console.Write("\n=====> Mouse Right Up");
            }
        }

        private void GL_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (mouseDown)
            {
                renderer.rotate(mRefPoint.Y - e.Y, mRefPoint.X - e.X, 0);
                this.glcontrol.Invalidate();
            }
            mRefPoint.X = e.X;
            mRefPoint.Y = e.Y;
        }

        private void GL_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Console.Write("\n=====> Mouse Wheel at: [ " + e.Delta + " ]");
            double delta = e.Delta > 0 ? -0.1 : 0.1;
            scaleRate += delta;
            if (scaleRate < 0.1) scaleRate = 0.1;
            if (scaleRate > 10) scaleRate = 10;
            this.glcontrol.Invalidate();
        }
        
    }
}
