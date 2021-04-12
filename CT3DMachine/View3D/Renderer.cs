using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Runtime.InteropServices;
using CT3DMachine.Tools;
namespace CT3DMachine.View3D
{
    
    /// <summary>
    /// The renderer.
    /// </summary>
    public class Renderer
    {
        public int mImageCount = 109;
        public int mImageWidth = 256;
        public int mImageHeight = 256;

        public byte[] mRawData;
        public byte[] mRGBAData;
        
        public int displayList;

        public double[] mfRot;
        public double[] mdRotation;
        
        public int mTextId;

        public Renderer()
        {
            string dataPath = PathTool.bingPathFromAppDir("data/head256x256x109");
            FileStream fs = new FileStream(dataPath, FileMode.Open);
            mImageWidth = 256;
            mImageHeight = 256;
            mImageCount = 109;
            BinaryReader br = new BinaryReader(fs);
            int sizeRawData = mImageWidth * mImageHeight * mImageCount;
            mRawData = br.ReadBytes(sizeRawData);
            mRGBAData = new byte[sizeRawData * 4];

            for (int i = 0; i < sizeRawData; i++)
            {
                mRGBAData[i * 4] = mRawData[i];
                mRGBAData[i * 4 + 1] = mRawData[i];
                mRGBAData[i * 4 + 2] = mRawData[i];
                mRGBAData[i * 4 + 3] = mRawData[i];
            }

            mfRot = new double[3] { 0.0, 0.0, 0.0 };
            mdRotation = new double[16] { 1.0, 0.0, 0.0, 0.0,
                                          0.0, 1.0, 0.0, 0.0,
                                          0.0, 0.0, 1.0, 0.0,
                                          0.0, 0.0, 0.0, 1.0 };
        }

        public void Render()
        {
			
		}

        public void rotate(double _dx, double _dy, double _dz)
        {
            mfRot[0] = _dx;
            mfRot[1] = _dy;
            mfRot[2] = _dz;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(mdRotation);
            GL.Rotate(mfRot[0], 1.0, 0.0, 0.0);
            GL.Rotate(mfRot[1], 0.0, 1.0, 0.0);
            GL.Rotate(mfRot[2], 0.0, 0.0, 1.0);
            GL.GetDouble(GetPName.ModelviewMatrix, mdRotation);
            GL.LoadIdentity();
        }
	}
}