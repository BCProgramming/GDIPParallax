using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParallaxTest
{
    public partial class ParallaxForm : Form
    {
        private Image body = null;
        private Image Crystal1 = null;
        private Image Crystal2 = null;
        private Image Crystal3 = null;
        private Image Road = null;
        private TextureBrush[] Layers = null;


        private TextureBrush[] ForegroundLayers = null;

        private void LoadImages()
        {
            String sDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            String bodyPath = Path.Combine(sDir, "body.png");
            String crystal1Path = Path.Combine(sDir, "crystal1.png");
            String crystal2Path = Path.Combine(sDir, "crystal2.png");
            String crystal3Path = Path.Combine(sDir, "crystal3.png");
            String roadPath = Path.Combine(sDir, "road.png");
            body = Image.FromFile(bodyPath);
            Crystal1 = Image.FromFile(crystal1Path);
            Crystal2 = Image.FromFile(crystal2Path);
            Crystal3 = Image.FromFile(crystal3Path);
            Road = Image.FromFile(roadPath);
        }
        private Image ResizeImage(Image Source, Size newSize)
        {
            Bitmap result = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppPArgb);
            using (Graphics bgr = Graphics.FromImage(result))
            {
                bgr.DrawImage(Source, 0, 0, newSize.Width, newSize.Height);
            }

            return result;
        }
        const int NumRoadLayers = 20;
        const float ScaleRatio = 0.2f;
        bool Prepared = false;
        private void PrepareTextures()
        {

            TextureBrush Crystal1Texture = new TextureBrush(Crystal1);
            TextureBrush Crystal2Texture = new TextureBrush(Crystal2);
            TextureBrush Crystal3Texture = new TextureBrush(Crystal3);
            TextureBrush Crystal4Texture = new TextureBrush(Crystal1);
            TextureBrush Crystal5Texture = new TextureBrush(Crystal2);
            TextureBrush Crystal6Texture = new TextureBrush(Crystal3);
            Layers = new TextureBrush[] { Crystal1Texture, Crystal2Texture, Crystal3Texture, Crystal4Texture, Crystal5Texture, Crystal6Texture };

            ForegroundLayers = new TextureBrush[NumRoadLayers];
            for (int i = 0; i < NumRoadLayers; i++)
            {
                //lowest value is smallest, we increase the scale for each.
                TextureBrush LayerTexture = new TextureBrush(Road);
                LayerTexture.ScaleTransform(ScaleRatio * i, ScaleRatio * i);
                ForegroundLayers[i] = LayerTexture;

            }
            


            Prepared = true;
        }
        private void SetTextureOrigins(int Value,TextureBrush[] TheLayers,double offsetScale = 1,double XScale = 0,double YScale = 1)
        {
            for(int i=0;i< TheLayers.Length;i++)
            {
                TheLayers[i].ResetTransform();
                float Offset = (float)((Value + i) * ((float)i*offsetScale));
                TheLayers[i].TranslateTransform((float)(Offset*XScale), (float)(Offset*YScale));
            }
        }
        public ParallaxForm()
        {
            LoadImages();
            PrepareTextures();
            InitializeComponent();
        }
        Thread ForcePaintThread = null;
        int CurrentPaintCount = 0;
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
            unchecked
            {
                CurrentPaintCount++;
            }
            e.Graphics.DrawImage(body, ClientRectangle);
            //set origins
            SetTextureOrigins(CurrentPaintCount,Layers);
            SetTextureOrigins(CurrentPaintCount, ForegroundLayers,-0.4,1,0);
            for(int i=0;i<Layers.Length;i++)
            {
                e.Graphics.FillRectangle(Layers[i], ClientRectangle);
            }

            float StartYPosition = ClientSize.Height*(3f / 4f);
            float CurrentYPosition = StartYPosition;
            float PerLayer = (ClientSize.Height * .25f) / NumRoadLayers;

            for (int i = 0; i < NumRoadLayers;i++)
            {
                e.Graphics.FillRectangle(ForegroundLayers[i], new RectangleF(0, CurrentYPosition, ClientSize.Width, PerLayer * 1.25f));
                CurrentYPosition += PerLayer;
            }





        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            ForcePaintThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            this.Invalidate();
                            this.Update();
                        }));
                        Thread.Sleep(50);
                    }
                }
                catch(Exception exr)
                {
                    //probably a dispose exception
                }
            });
            ForcePaintThread.IsBackground = true;
            ForcePaintThread.Start();
        }

        private void ParallaxForm_ResizeEnd(object sender, EventArgs e)
        {
            Prepared = false;
        }
    }
}
