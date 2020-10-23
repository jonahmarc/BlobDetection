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
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace BlobDetection
{
    public partial class Form1 : Form
    {
        Bitmap processed;
        int five_cents = 0;
        int ten_cents = 0;
        int twentyfive_cents = 0;
        int one_peso = 0;
        int five_peso = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void browse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                processed = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = processed;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void process_Click(object sender, EventArgs e)
        {
            //grayscale
            Grayscale filter1 = new Grayscale(0.2125, 0.7154, 0.0721);
            processed = filter1.Apply(processed);

            //threshold
            var filter2 = new AForge.Imaging.Filters.Threshold(175);
            processed = filter2.Apply(processed);

            // erosion
            Erosion filter3 = new Erosion();
            filter3.Apply(processed);

            // create filter
            BlobsFiltering filter = new BlobsFiltering();
            // configure filter
            filter.CoupledSizeFiltering = true;
            filter.MinWidth = 25;
            filter.MinHeight = 25;
            // apply the filter
            filter.ApplyInPlace(processed);

            Invert filterInvert = new Invert();
            // apply the filter
            filterInvert.ApplyInPlace(processed);


            BlobCounterBase bc = new BlobCounter();
            bc.FilterBlobs = true;
            bc.MinWidth = 30; //give required value or ignore
            bc.MinHeight = 30; //give required value  or ignore
            bc.CoupledSizeFiltering = true; // if value are given and if you want both Width and Height to be applied as a constraint to identify blob, set it to true
            bc.ProcessImage(processed);
            Blob[] blobs = bc.GetObjectsInformation();

            int count = bc.ObjectsCount;

            // lock image to draw on it
            BitmapData data = processed.LockBits(
                new Rectangle(0, 0, processed.Width, processed.Height),
                    ImageLockMode.ReadWrite, processed.PixelFormat);


            // process each blob
            foreach (Blob blob in blobs)
            {
                List<IntPoint> leftPoints, rightPoints, edgePoints;
                edgePoints = new List<IntPoint>();

                // get blob's edge points
                bc.GetBlobsLeftAndRightEdges(blob,
                    out leftPoints, out rightPoints);

                edgePoints.AddRange(leftPoints);
                edgePoints.AddRange(rightPoints);

                IConvexHullAlgorithm hullFinder = new GrahamConvexHull();

                // blob's convex hull
                List<IntPoint> hull = hullFinder.FindHull(edgePoints);

                Drawing.Polygon(data, hull, Color.Yellow);

                if (blob.Area < 8000)
                    five_cents++;
                else if (blob.Area < 9000 && blob.Area > 8000)
                    ten_cents++;
                else if (blob.Area < 13000 && blob.Area > 11000)
                    twentyfive_cents++;
                else if (blob.Area < 17000 && blob.Area > 16000)
                    one_peso++;
                else
                    five_peso++;

            }

            processed.UnlockBits(data);


            pictureBox2.Image = processed;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

            textBox1.Text += count;
            textBox2.Text += ((five_cents * .05) + (ten_cents * .10) + (twentyfive_cents * .25) + (one_peso * 1) + (five_peso * 5));



        }
    }
}
