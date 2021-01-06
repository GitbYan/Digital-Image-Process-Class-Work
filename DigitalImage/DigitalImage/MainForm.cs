using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalImage
{
    public partial class MainForm : System.Windows.Forms.Form
    {

        public MainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

        }

        public Image ori_image;
        public string ori_imageName;
        public Bitmap currentBitmap;
        public Bitmap newBitmap;
        public Bitmap newBitmap_;
        public bool ready = false;
        public bool notdone = true;

        private void Btn_open_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = @"F:";
            openFileDialog.FileName = "";
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            ori_imageName = openFileDialog.FileName;
            ori_image = Image.FromStream(openFileDialog.OpenFile());
            Bitmap bitmap = (Bitmap)ori_image;
            currentBitmap = CopyBitmap(bitmap);
            pictureBox1.Image = ori_image;
            ready = true;
            notdone = true;
        }

        private void Btn_loadImage_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = ori_image;
        }

        private void Btn_balanced_Click(object sender, EventArgs e)
        {
            if (ready&&notdone)
            {
                Btn_open.Enabled = false;
                GetGrayBitmap();
                pictureBox2.Image = currentBitmap;
                GetChartsValue();
                GetChartsValue_();
                Btn_open.Enabled = true;
                notdone = false;
            }
        }


        //平均值法求灰度值
        /* 1.浮点法：Gray=R*0.3+G*0.59+B*0.11
         * 2.整数法：Gray=(R*30+G*59+B*11)/100
         * 3.移位法：Gray =(R*77+G*151+B*28)>>8;
         * 4.平均值法：Gray=（R+G+B）/3;
         * 5.仅取绿色：Gray=G;
         * 6.Gamma校正算法：[(R^2.2+(1.5*G)^2.2+(0.6*B)^2.2)/(1+(1.5)^2.2+(0.6)^2.2)]^(-2.2);
         */
        private void GetGrayBitmap()
        {
            label3.Text = "图像初始化准备中";
            for (int x = 0; x < currentBitmap.Width; x++)
            {
                for (int y = 0; y < currentBitmap.Height; y++)
                {
                    Color currentColor = currentBitmap.GetPixel(x, y);
                    int r = (currentColor.R + currentColor.G + currentColor.B) / 3;
                    currentBitmap.SetPixel(x, y, Color.FromArgb(r, r, r));
                }
            }
            label3.Text = "";
        }

        //直方图均衡化算法
        private void GetChartsValue()
        {
            label3.Text = "正在生成直方图";
            //计算原图像各灰度级像素个数
            int[] ori_Pixel = new int[256];
            for (int x = 0; x < currentBitmap.Width; x++)
            {
                for (int y = 0; y < currentBitmap.Height; y++)
                {
                    Color currentColor = currentBitmap.GetPixel(x, y);
                    int temp = (currentColor.R + currentColor.G + currentColor.B) / 3;
                    ori_Pixel[temp]++;
                }
            }
            chart1.Series[0].Points.DataBindY(ori_Pixel);
            //计算累计分布函数
            int[] temp_Pixel = new int[256];
            for(int i = 0; i < 256; i++)
            {
                if (i != 0)
                {
                    temp_Pixel[i] = ori_Pixel[i] + temp_Pixel[i - 1];
                }
                else
                {
                    temp_Pixel[i] = ori_Pixel[i];
                }
            }
            newBitmap = CopyBitmap(currentBitmap);
            //灰度映射
            int[] new_Pixel = new int[256];
            for(int i = 0; i < 256; i++)
            {
                new_Pixel[i] = (int)(255.0 * temp_Pixel[i] / (currentBitmap.Width * currentBitmap.Height) + 0.5);
            }
            chart2.Series[0].Points.DataBindY(new_Pixel);
            for (int x = 0; x < newBitmap.Width; x++)
            {
                for (int y = 0; y < newBitmap.Height; y++)
                {
                    int i = (newBitmap.GetPixel(x, y).R + newBitmap.GetPixel(x, y).G + newBitmap.GetPixel(x, y).B) / 3;
                    newBitmap.SetPixel(x, y, Color.FromArgb(new_Pixel[i], new_Pixel[i], new_Pixel[i]));
                }
            }
            pictureBox3.Image = newBitmap;
            label3.Text = "";
        }

        //改进直方图均衡化算法
        private void GetChartsValue_()
        {
            label3.Text = "正在生成直方图";
            //计算原图像各灰度级像素个数
            int[] ori_Pixel = new int[256];
            for (int x = 0; x < currentBitmap.Width; x++)
            {
                for (int y = 0; y < currentBitmap.Height; y++)
                {
                    Color currentColor = currentBitmap.GetPixel(x, y);
                    int temp = (currentColor.R + currentColor.G + currentColor.B) / 3;
                    ori_Pixel[temp]++;
                }
            }
            //先进行平滑处理
            for (int i = 0; i < 256; i++)
            {
                if (i != 0)
                {
                    ori_Pixel[i] = (int)(0.4 * ori_Pixel[i] + 0.6 * ori_Pixel[i - 1] + 0.5);
                }
            }
            //计算累计分布函数
            int[] temp_Pixel = new int[256];
            for (int i = 0; i < 256; i++)
            {
                if (i != 0)
                {
                    temp_Pixel[i] = ori_Pixel[i] + temp_Pixel[i - 1];
                }
                else
                {
                    temp_Pixel[i] = ori_Pixel[i];
                }
            }
            //灰度映射
            newBitmap_ = CopyBitmap(currentBitmap);
            int[] new_Pixel = new int[256];
            for (int i = 0; i < 256; i++)
            {
                new_Pixel[i] = (int)(255.0 * temp_Pixel[i] / (currentBitmap.Width * currentBitmap.Height) + 0.5);
            }
            chart3.Series[0].Points.DataBindY(new_Pixel);
            for (int x = 0; x < newBitmap_.Width; x++)
            {
                for (int y = 0; y < newBitmap_.Height; y++)
                {
                    int i = (newBitmap_.GetPixel(x, y).R + newBitmap_.GetPixel(x, y).G + newBitmap_.GetPixel(x, y).B) / 3;
                    newBitmap_.SetPixel(x, y, Color.FromArgb(new_Pixel[i], new_Pixel[i], new_Pixel[i]));
                }
            }
            filter();
            pictureBox4.Image = newBitmap_;
            label3.Text = "";
        }

        //中值滤波处理
        private void filter()
        {
            int[] L = new int[9];
            for (int x = 0; x < newBitmap_.Width; x++)
            {
                for (int y = 0; y < newBitmap_.Height; y++)
                {
                    if ((x == 0 && y == 0) || (x == 0 && y == newBitmap_.Height - 1) || (x == newBitmap_.Width - 1 && y == 0) || (x == newBitmap_.Width - 1 && y == newBitmap_.Height - 1))
                    {
                        newBitmap_.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else if (x == 0)
                    {
                        L[0] = L[1] = L[2] = 0;
                        L[3] = newBitmap_.GetPixel(x, y - 1).R;
                        L[4] = newBitmap_.GetPixel(x + 1, y - 1).R;
                        L[5] = newBitmap_.GetPixel(x, y).R;
                        L[6] = newBitmap_.GetPixel(x + 1, y).R;
                        L[7] = newBitmap_.GetPixel(x, y + 1).R;
                        L[8] = newBitmap_.GetPixel(x + 1, y + 1).R;
                        Array.Sort(L);
                        newBitmap_.SetPixel(x, y, Color.FromArgb(L[4], L[4], L[4]));
                    }
                    else if (x == newBitmap_.Width - 1)
                    {
                        L[0] = L[1] = L[2] = 0;
                        L[3] = newBitmap_.GetPixel(x, y - 1).R;
                        L[4] = newBitmap_.GetPixel(x - 1, y - 1).R;
                        L[5] = newBitmap_.GetPixel(x, y).R;
                        L[6] = newBitmap_.GetPixel(x - 1, y).R;
                        L[7] = newBitmap_.GetPixel(x, y + 1).R;
                        L[8] = newBitmap_.GetPixel(x - 1, y + 1).R;
                        Array.Sort(L);
                        newBitmap_.SetPixel(x, y, Color.FromArgb(L[4], L[4], L[4]));
                    }
                    else if (y == 0)
                    {
                        L[0] = L[1] = L[2] = 0;
                        L[3] = newBitmap_.GetPixel(x - 1, y).R;
                        L[4] = newBitmap_.GetPixel(x, y).R;
                        L[5] = newBitmap_.GetPixel(x + 1, y).R;
                        L[6] = newBitmap_.GetPixel(x + 1, y + 1).R;
                        L[7] = newBitmap_.GetPixel(x, y + 1).R;
                        L[8] = newBitmap_.GetPixel(x - 1, y + 1).R;
                        Array.Sort(L);
                        newBitmap_.SetPixel(x, y, Color.FromArgb(L[4], L[4], L[4]));
                    }
                    else if (y == newBitmap_.Height - 1)
                    {
                        L[0] = L[1] = L[2] = 0;
                        L[3] = newBitmap_.GetPixel(x - 1, y).R;
                        L[4] = newBitmap_.GetPixel(x, y).R;
                        L[5] = newBitmap_.GetPixel(x + 1, y).R;
                        L[6] = newBitmap_.GetPixel(x + 1, y - 1).R;
                        L[7] = newBitmap_.GetPixel(x, y - 1).R;
                        L[8] = newBitmap_.GetPixel(x - 1, y - 1).R;
                        Array.Sort(L);
                        newBitmap_.SetPixel(x, y, Color.FromArgb(L[4], L[4], L[4]));
                    }
                    else
                    {
                        L[0] = newBitmap_.GetPixel(x - 1, y - 1).R;
                        L[1] = newBitmap_.GetPixel(x, y - 1).R;
                        L[2] = newBitmap_.GetPixel(x + 1, y - 1).R;
                        L[3] = newBitmap_.GetPixel(x - 1, y).R;
                        L[4] = newBitmap_.GetPixel(x, y).R;
                        L[5] = newBitmap_.GetPixel(x + 1, y).R;
                        L[6] = newBitmap_.GetPixel(x + 1, y + 1).R;
                        L[7] = newBitmap_.GetPixel(x, y + 1).R;
                        L[8] = newBitmap_.GetPixel(x - 1, y + 1).R;
                        Array.Sort(L);
                        newBitmap_.SetPixel(x, y, Color.FromArgb(L[4], L[4], L[4]));
                    }
                }
            }
        }


        public Bitmap CopyBitmap(Bitmap source)
        {
            int depth = Bitmap.GetPixelFormatSize(source.PixelFormat);


            if (depth != 8 && depth != 24 && depth != 32)
            {
                return null;
            }

            Bitmap destination = new Bitmap(source.Width, source.Height, source.PixelFormat);

            BitmapData source_bitmapdata = null;
            BitmapData destination_bitmapdata = null;

            try
            {
                source_bitmapdata = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite,
                                                source.PixelFormat);
                destination_bitmapdata = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.ReadWrite,
                                                destination.PixelFormat);

                unsafe
                {
                    byte* source_ptr = (byte*)source_bitmapdata.Scan0;
                    byte* destination_ptr = (byte*)destination_bitmapdata.Scan0;

                    for (int i = 0; i < (source.Width * source.Height * (depth / 8)); i++)
                    {
                        *destination_ptr = *source_ptr;
                        source_ptr++;
                        destination_ptr++;
                    }
                }

                source.UnlockBits(source_bitmapdata);
                destination.UnlockBits(destination_bitmapdata);

                return destination;
            }
            catch
            {
                destination.Dispose();
                return null;
            }
        }
    }
}
