using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace Test1203.ViewModel
{
    internal class TextImage
    // Class for setting up creating temporary annotations in Revit. WIP.
    {
        private static List<String> img_paths { get; set; } = new List<String>();
        private static string CreateAndSaveImage(String text, int num)
        {
            Image img = DrawText(text, new Font(FontFamily.GenericMonospace, 60.0f), System.Drawing.Color.Black, System.Drawing.Color.FromArgb(255, 0, 128, 128));
            
            //Random rnd = new Random();
            //int num = rnd.Next(1000000, 9999999);
            var filePath = Path.GetTempPath() + "ABC_TEST" + num.ToString() + ".bmp";
            
            img.Save(filePath, ImageFormat.Bmp);

            img_paths.Add(filePath);

            return filePath;
        }
        private static Image DrawText(String text, Font font, System.Drawing.Color textColor, System.Drawing.Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }
        public static void CreateRevitGraphics(int elNumber, Element el, ElementId viewId)
        {
            var tgm = TemporaryGraphicsManager.GetTemporaryGraphicsManager(Command.doc);
            
            BoundingBoxXYZ bb = el.get_BoundingBox(null);
            XYZ cp = bb.Max - ((bb.Max - bb.Min) / 2);

            string path = CreateAndSaveImage(elNumber == 0 ? "Element 1" : "Element 2", elNumber);

            var control = new InCanvasControlData(path, cp);
            tgm.AddControl(control, viewId);
        }
        public static void RemoveAllGraphicElements()
        {
            var tgm = TemporaryGraphicsManager.GetTemporaryGraphicsManager(Command.doc);
            tgm.Clear();

            foreach (var img_path in img_paths)
            {
                File.Delete(img_path);
            }
            img_paths.Clear();
        }
        public static void RemoveOneGraphicElement(int num)
        {
            var tgm = TemporaryGraphicsManager.GetTemporaryGraphicsManager(Command.doc);
            tgm.RemoveControl(num);
        }
    }
}
