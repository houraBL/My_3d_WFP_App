using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;
using System.Windows.Markup;
using System.Xml;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using HelixToolkit.Wpf.SharpDX;

namespace My_3d_WFP_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<BasicModel> ModelList;
        static MyScene myScene;
        static List<string> modelList;

        /*
        1 имя модели для отображения
        2 выбрана модель или нет
        3 ссылка на модель
        */

        /// <summary>
        /// Contains unic Model names, paths and Model3DGroup - model3D container
        /// </summary>
        class MyScene
        {
            private List<string> names;
            private List<string> paths;
            private Model3DGroup scene;
            private List<double[]> transformations;


            public MyScene()
            {
                names = new List<string>();
                paths = new List<string>();
                scene = new Model3DGroup();
                transformations = new List<double[]>();
            }

            /// <summary>
            /// Add element from path to scene, names model as filename
            /// </summary>
            /// <param name="path .obj file please"></param>
            public void AddElement(string path)
            {
                var unicName = Path.GetFileNameWithoutExtension(path);
                var almostUnicName = unicName;
                var unicNumber = 0;
                while (names.Contains(almostUnicName)) {
                    almostUnicName = unicName + unicNumber++;
                }
                names.Add(almostUnicName);
                paths.Add(path);
                transformations.Add(new double[7] { 0, 0, 0, 0, 0, 0, 100 }); // x, y, z, xangle, yangle, zangle, scale;
                scene.Children.Add(new HelixToolkit.Wpf.ModelImporter().Load(path));
                RotateTransform3D myRotateTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
                scene.Children.Last().Transform = myRotateTransform;
                var sizex = scene.Children.Last().Bounds.SizeX;
                var x = scene.Children.Last().Bounds.X;

            }

            /// <summary>
            /// Delete chosen element
            /// </summary>
            /// <param name="name of chosen element"></param>
            public void DeleteElement(string selectedItem)
            {
                if (names.IndexOf(selectedItem) != -1)
                {
                    paths.RemoveAt(names.IndexOf(selectedItem));
                    scene.Children.RemoveAt(names.IndexOf(selectedItem));
                    names.RemoveAt(names.IndexOf(selectedItem));
                }
            }

            /// <summary>
            /// Rotation method
            /// </summary>
            /// <param name="selectedModel"></param>
            /// <param name="angleX"></param>
            /// <param name="angleY"></param>
            /// <param name="angleZ"></param>
            public void Rotate(string selectedModel, double angleX, double angleY, double angleZ)
            {
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(angleX, 3);
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(angleY, 4);
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(angleZ, 5);
                Transform(selectedModel);
            }

            /// <summary>
            /// Method for Model relocation
            /// </summary>
            /// <param name="selected Model name"></param>
            /// <param name="New X coordinate"></param>
            /// <param name="New Y coordinate"></param>
            /// <param name="New Z coordinate"></param>
            public void Move(string selectedModel, double offsetX, double offsetY, double offsetZ)
            {
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(offsetX, 0);
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(offsetY, 1);
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(offsetZ, 2);
                Transform(selectedModel);
            }


            public void Resize(string selectedModel, double size)
            {
                transformations.ElementAt(names.IndexOf(selectedModel)).SetValue(size, 6);
                Transform(selectedModel);
            }

            /// <summary>
            /// Transform model when values changed
            /// </summary>
            /// <param name="selectedModel"></param>
            public void Transform(string selectedModel)
            {
                Transform3DGroup transform = new Transform3DGroup();
                transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90 + transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(3))));
                transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0 + transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(4))));
                transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0 + +transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(5))));
                transform.Children.Add(new TranslateTransform3D(transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(0),
                                                                transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(1),
                                                                transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(2)));
                var size = transformations.ElementAt(names.IndexOf(selectedModel)).ElementAt(6);
                transform.Children.Add(new ScaleTransform3D(size/100, size / 100, size / 100));
                scene.Children.ElementAt(names.IndexOf(selectedModel)).Transform = transform;
            }

            public double[] GetTransformations(string selectedModel)
            {
                return transformations.ElementAt(names.IndexOf(selectedModel));
            }

            public List<string> Names
            {
                get
                {
                    return names;
                }
            }

            public List<string> Paths
            {
                get
                {
                    return paths;
                }
            }

            public Model3DGroup Scene
            {
                get
                {
                    return scene;
                }
            }

        }

        /// <summary>
        /// Initialisation of MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            modelList = new List<string>();
            myScene = new MyScene();
            foo.Content = myScene.Scene;
        }

        #region Add/Delete Model from scene

        /// <summary>
        /// Add avaliable models from Furniture folder to modelListCombobx and modelList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitializeModelList(object sender, RoutedEventArgs e)
        {
            string[] filePaths = Directory.GetFiles("C:/Users/Anastasiia/source/repos/My_3d_WFP_App/furniture/");
            foreach (var file in filePaths)
            {
                if (Path.GetExtension(file) == ".obj")
                {
                    modelList.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            cbModelList.ItemsSource = modelList;
        }

        private void UploadModelFromFile(object sender, RoutedEventArgs e)
        {
            AddNewFileToModelList();
        }

        /// <summary>
        /// Add model to scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddModelToScene(object sender, RoutedEventArgs e)
        {
            myScene.AddElement("C:/Users/Anastasiia/source/repos/My_3d_WFP_App/furniture/" + (string)cbModelList.SelectedItem + ".obj");
            lvScene.ItemsSource = myScene.Names;
            lvScene.Items.Refresh();
        }

        private void AddNewFileToModelList()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Obj files (*.obj)|*.obj";
            Nullable<bool> result = fileDialog.ShowDialog();
            string newModel;
            if (result == true)
            {
                newModel = fileDialog.FileName;
                string directoryPath = System.IO.Path.GetDirectoryName(fileDialog.FileName);
                string newModelName = Path.GetFileNameWithoutExtension(newModel);               
                
                File.Copy(newModel, $"C:/Users/Anastasiia/source/repos/My_3d_WFP_App/furniture/{Path.GetFileName(newModel)}");
                modelList.Add($"C:/Users/Anastasiia/source/repos/My_3d_WFP_App/furniture/{Path.GetFileName(newModel)}");
                myScene.AddElement(newModel);
                lvScene.ItemsSource = myScene.Names;
                
            }
            lvScene.Items.Refresh();
        }

        /// <summary>
        /// Delete model from scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteModelFromScene_Click(object sender, RoutedEventArgs e)
        {
            myScene.DeleteElement((string)lvScene.SelectedItem);
            lvScene.ItemsSource = myScene.Names;
            lvScene.Items.Refresh();
            btnDeleteModelFromScene.IsEnabled = false;
        }
        #endregion

        #region Model Controls

        /// <summary>
        /// Turn model
        /// </summary>
        /// <param name="Chosen slider"></param>
        /// <param name="e"></param>
        private void angle_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (((Slider)sender).IsEnabled)
            {
                var selectedModel = (string)lvScene.SelectedItem;

                var angleX = (double)slXangle.Value;
                var angleY = (double)slYangle.Value;
                var angleZ = (double)slZangle.Value;

                myScene.Rotate(selectedModel, angleX, angleY, angleZ);
            }
        }

        private void Coordinate_ValueChanging(object sender, Syncfusion.Windows.Shared.ValueChangingEventArgs e)
        {
            var x = (double)udXCoord.Value;
            var y = (double)udYCoord.Value;
            var z = (double)udZCoord.Value;
            var selectedModel = (string)lvScene.SelectedItem;
            myScene.Move(selectedModel, x, y, z);
        }

        
        private void udModelSize_ValueChanging(object sender, Syncfusion.Windows.Shared.ValueChangingEventArgs e)
        {
            var size = (double)udModelSize.Value;
            var selectedModel = (string)lvScene.SelectedItem;
            myScene.Resize(selectedModel, size);
        }

        #endregion

        #region Floor and Ceiling
        /// <summary>
        /// Changing Floor and Ceiling length
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FloorLength_ValueChanging(object sender, Syncfusion.Windows.Shared.ValueChangingEventArgs e)
        {
            Floor.Length = Ceiling.Length = (double)e.NewValue;
            Grid.Length = (int)(double)e.NewValue + 20;
        }

        /// <summary>
        /// Changing Floor and Ceiling width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FloorWidth_ValueChanging(object sender, Syncfusion.Windows.Shared.ValueChangingEventArgs e)
        {
            Floor.Width = Ceiling.Width = (double)e.NewValue;
            Grid.Width = (int)(double)e.NewValue + 20;
        }

        /// <summary>
        /// Changing Celining Heigth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CeilingHeigth_ValueChanging(object sender, Syncfusion.Windows.Shared.ValueChangingEventArgs e)
        {
            Ceiling.Origin = new Point3D(0, 0, (double)e.NewValue);
        }

        /// <summary>
        /// Ceiling is vivble
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CeilingVisability_Checked(object sender, RoutedEventArgs e)
        {
            if (Ceiling != null)
            {
                Ceiling.Width = (double)FloorWidth.Value;
                Ceiling.Length = (double)FloorLength.Value;
                CeilingHeigth.IsEnabled = true;
            }
        }

        /// <summary>
        /// Ceiling is invisible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CeilingVisability_Unchecked(object sender, RoutedEventArgs e)
        {
            Ceiling.Width = Ceiling.Length = 0;
            CeilingHeigth.IsEnabled = false;
        }
        #endregion

        #region Model list

        private void cbModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddModelToScene.IsEnabled = true;
        }

        private void lvScene_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (lvScene.SelectedItems.Count == 0)
            {
                lChosenModel.Content = "Chose model to work with";
                changeModelControlsEnablement(false);
            }
            else
            {
                lChosenModel.Content = lvScene.SelectedItem;
                changeModelControlsEnablement(false);
                updateControls();
                changeModelControlsEnablement(true);
                btnDeleteModelFromScene.IsEnabled = true;
            }
        }
        
        private void updateControls()
        {
            var sourse = myScene.GetTransformations((string)lvScene.SelectedItem);
            udXCoord.Value = sourse.ElementAt(0);
            udYCoord.Value = sourse.ElementAt(1);
            udZCoord.Value = sourse.ElementAt(2);
            slXangle.Value = sourse.ElementAt(3); /// -> value changed
            slYangle.Value = sourse.ElementAt(4);
            slZangle.Value = sourse.ElementAt(5);
            udModelSize.Value = sourse.ElementAt(6);
        }


        private void changeModelControlsEnablement(bool isEnabled)
        {            
            udXCoord.IsEnabled = isEnabled;
            udYCoord.IsEnabled = isEnabled;
            udZCoord.IsEnabled = isEnabled;
            slXangle.IsEnabled = isEnabled;
            slYangle.IsEnabled = isEnabled;
            slZangle.IsEnabled = isEnabled;
            udModelSize.IsEnabled = isEnabled;
        }

        private void lvScene_LostFocus(object sender, RoutedEventArgs e)
        {
            var k = lvScene.SelectedItem;
            lChosenModel.Content = "Chose model to work with";
            changeModelControlsEnablement(false);
        }
       
        private void lvScene_GotFocus(object sender, RoutedEventArgs e)
        {
            changeModelControlsEnablement(true);
        }
        #endregion

        private void slXangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

