using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AForge;
using AForge.Math;
using AForge.Imaging;
using AForge.Math.Geometry;
using AForge.Imaging.Filters;


using Image = System.Drawing.Image; //Remove ambiguousness between AForge.Image and System.Drawing.Image
using Point = System.Drawing.Point; //Remove ambiguousness between AForge.Point and System.Drawing.Point

namespace AEES_Desktop_1._0._0._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static Bitmap bitmap, crni;
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open Image";
            dlg.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Set the size of the PictureBox control. 
                this.pictureBox1.Size = new System.Drawing.Size(396, 590);

                //Set the SizeMode to center the image. 
                this.pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

                // Set the border style to a three-dimensional border. 
                this.pictureBox1.BorderStyle = BorderStyle.Fixed3D;


                //Load picture
                pictureBox1.Image = new Bitmap(dlg.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                bitmap = new Bitmap(dlg.FileName) as Bitmap;
            }
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap temp = bitmap;
            // lock image
            BitmapData bitmapData = temp.LockBits(
                new Rectangle(0, 0, temp.Width, temp.Height),
                ImageLockMode.ReadWrite, temp.PixelFormat);

            // step 1 - turn background to black
            ColorFiltering colorFilter = new ColorFiltering();

            colorFilter.Red   = new IntRange( 0, 64 );
            colorFilter.Green = new IntRange( 0, 64 );
            colorFilter.Blue  = new IntRange( 0, 64 );
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace( bitmapData );

            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 5;
            blobCounter.MinWidth = 5;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            temp.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(temp);
            Pen pen = new Pen(Color.White, 30);

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                AForge.Point center;
                float radius;

                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(pen,
                        (int)(center.X - radius),
                        (int)(center.Y - radius),
                        (int)(radius * 2),
                        (int)(radius * 2));
                }
            }

            g.Dispose();
            pen.Dispose();

            // Set the size of the PictureBox control. 
            this.pictureBox2.Size = new System.Drawing.Size(396, 590);

            //Set the SizeMode to center the image. 
            this.pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

            // Set the border style to a three-dimensional border. 
            this.pictureBox2.BorderStyle = BorderStyle.Fixed3D;

            pictureBox2.Image = temp;
            crni = temp;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            Bitmap orig = crni;

            Bitmap clone = new Bitmap(orig.Width, orig.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            }

            FiltersSequence commonSeq = new FiltersSequence();
            commonSeq.Add(Grayscale.CommonAlgorithms.BT709);
            commonSeq.Add(new BradleyLocalThresholding());
            commonSeq.Add(new DifferenceEdgeDetector());

            Bitmap temp = commonSeq.Apply(clone);

            BlobsFiltering filter = new BlobsFiltering();
            filter.CoupledSizeFiltering = true;
            filter.MinWidth = 10;
            filter.MinHeight = 10;
            // apply the filter
            filter.ApplyInPlace(temp);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(temp);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            Bitmap temp4 = (Bitmap)pictureBox1.Image;
            Graphics g = Graphics.FromImage(temp4);
            Pen pen = new Pen(Color.Red, 2);

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                AForge.Point center;
                float radius;

                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(pen,
                        (int)(center.X - radius),
                        (int)(center.Y - radius),
                        (int)(radius * 2),
                        (int)(radius * 2));
                }
            }

            g.Dispose();
            pen.Dispose();

            // Set the size of the PictureBox control. 
            this.pictureBox3.Size = new System.Drawing.Size(396, 590);

            //Set the SizeMode to center the image. 
            this.pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;

            // Set the border style to a three-dimensional border. 
            this.pictureBox3.BorderStyle = BorderStyle.Fixed3D;

            pictureBox3.Image = temp4;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}