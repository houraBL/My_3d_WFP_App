using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Media;

namespace My_3d_WFP_App
{
    /// <summary>
    /// PRE-downloaded or downloaded models
    /// </summary>
    class BasicModel : UIElement3D
    {
        
        //private object model;
        public BasicModel(string resourcePath)
        {            
            this.Visual3DModel = Application.Current.Resources[resourcePath] as Model3DGroup;
            Debug.Assert(this.Visual3DModel != null);
        }
        /// <summary>
        /// Method for Model relocation
        /// </summary>
        /// <param name="New X coordinate"></param>
        /// <param name="New Y coordinate"></param>
        /// <param name="New Z coordinate"></param>
        public void Move(double offsetX, double offsetY, double offsetZ)
        {
            Transform3DGroup transform = new Transform3DGroup();
            RotateTransform3D rotateTrans = new RotateTransform3D();
            TranslateTransform3D translateTrans = new TranslateTransform3D(offsetX, offsetY, offsetZ);
            transform.Children.Add(rotateTrans);
            transform.Children.Add(translateTrans);
            this.Transform = transform;
        }

        /// <summary>
        /// Rotate model according to chosen Slider probably
        /// </summary>
        /// <param name="xAngle_curr"></param>
        /// <param name="yAngle_curr"></param>
        /// <param name="zAngle_curr"></param>
        public void Rotate(Vector3D vector, double angle)
        {            
            var matrix = this.Transform.Value;
            matrix.Rotate(new Quaternion(vector, angle));
            this.Transform = new MatrixTransform3D(matrix);
        }

        /// <summary>
        /// removes unnesesary elements (ligth, etc.)
        /// </summary>
        /// <param name="filename actually is a path, so make sure, you put all the path in it"></param>
        

    }

}
