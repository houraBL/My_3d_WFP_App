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

            public MyScene()
            {
                names = new List<string>();
                paths = new List<string>();
                scene = new Model3DGroup();
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
                scene.Children.Add(new ModelImporter().Load(path));
                RotateTransform3D myRotateTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
                myRotateTransform.CenterX = 0;
                myRotateTransform.CenterY = 0;
                myRotateTransform.CenterZ = 0;
                scene.Children.Last().Transform = myRotateTransform;
                var sizex = scene.Children.Last().Bounds.SizeX;
                var sizey = scene.Children.Last().Bounds.SizeY;
                var sizez = scene.Children.Last().Bounds.SizeZ;

                var x = scene.Children.Last().Bounds.X;
                var y = scene.Children.Last().Bounds.Y;
                var z = scene.Children.Last().Bounds.Z;
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
            /// Rotate method
            /// </summary>
            /// <param name="selected Model name"></param>
            /// <param name="vector (format: (0,1,0) - only 1 axis is chosen)"></param>
            /// <param name="angle"></param>
            public void Rotate(string selectedModel, Vector3D vector, double angle)
            {                
                var matrix = scene.Children.ElementAt(names.IndexOf(selectedModel)).Transform.Value;
                matrix.Rotate(new Quaternion(vector, angle));
                scene.Children.ElementAt(names.IndexOf(selectedModel)).Transform = new MatrixTransform3D(matrix);
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
                Transform3DGroup transform = new Transform3DGroup();
                RotateTransform3D rotateTrans = new RotateTransform3D();
                TranslateTransform3D translateTrans = new TranslateTransform3D(offsetX, offsetY, offsetZ);
                transform.Children.Add(rotateTrans);
                transform.Children.Add(translateTrans);
                scene.Children.ElementAt(names.IndexOf(selectedModel)).Transform = transform;
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

        }

        /// <summary>
        /// Add model to scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddModelToScene(object sender, RoutedEventArgs e)
        {
            myScene.AddElement(@"furniture/" + (string)cbModelList.SelectedItem + ".obj");
            lvScene.ItemsSource = myScene.Names;
            lvScene.Items.Refresh();
        }

        private static void AddNewFileToModelList(string newModel)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            string newModelName = Path.GetFileNameWithoutExtension(newModel);
            var IsKeyFound = false;
            foreach (var modelName in modelList)
            {
                if (modelName == newModelName) { IsKeyFound = true; }
            }
            if (!IsKeyFound)
            {
                File.Copy(newModel, $"C:/Users/Anastasiia/source/repos/My_3d_WFP_App/furniture/{Path.GetFileName(newModel)}");
                modelList.Add(Path.GetFileNameWithoutExtension(newModelName));
            }
               
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
            var angle = ((Slider)sender).Value;
            var sender_name = ((Slider)sender).Name;
            var modelName = (string)lvScene.SelectedItem;
            var axis = new Vector3D(0, 0, 1);
            switch (sender_name)
            {
                case "Xangle":
                    {
                        axis = new Vector3D(1, 0, 0);
                        break;
                    }
                case "Yangle":
                    {
                        axis = new Vector3D(0, 1, 0);
                        break;
                    }
                case "Zangle":
                    {
                        axis = new Vector3D(0, 0, 1);
                        break;
                    }
                default:
                    break;
            }
            myScene.Rotate(modelName, axis, angle);
        }

        private void Coordinate_ValueChanging(object sender, Syncfusion.Windows.Shared.ValueChangingEventArgs e)
        {
            var x = (double)udXCoord.Value;
            var y = (double)udYCoord.Value;
            var z = (double)udZCoord.Value;
            var selectedModel = (string)lvScene.SelectedItem;
            myScene.Move(selectedModel, x, y, z);
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
                changeModelControlsEnablement(true);
                btnDeleteModelFromScene.IsEnabled = true;
            }
        }

        private void changeModelControlsEnablement(bool isEnabled)
        {            
            udXCoord.IsEnabled = isEnabled;
            udYCoord.IsEnabled = isEnabled;
            udZCoord.IsEnabled = isEnabled;
            Xangle.IsEnabled = isEnabled;
            Yangle.IsEnabled = isEnabled;
            Zangle.IsEnabled = isEnabled;
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
    }
}


/*
public static void RemoveTextLines(string filename)
{
    // Initial values
    string tempFilename = Path.GetFileNameWithoutExtension(filename) + "1" + ".xaml";

    var openLine = "<Model3DGroup";
    var isOpenLineFound = false;
    int openLineCount = 0, closedLineCount = 0;
    var closedLine = "</Model3DGroup>";
    var isClosedLineFound = false;

    var wrapOpenLine = "<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">";
    var wrapClosedLine = "</ResourceDictionary>";

    // Read file
    using (var streamReader = new StreamReader(filename))
    {
        // Write new file
        using (var streamWriter = new StreamWriter(tempFilename))
        {
            string line;
            // write wrap line into temp file
            streamWriter.WriteLine(wrapOpenLine);
            // do while its not the end of file
            while ((line = streamReader.ReadLine()) != null)
            {
                //if we have already found closed line, still checking if there is more open-close pairs
                //просто проверочка, если следом сразу идет еще одно закрытие
                if (isClosedLineFound)
                {
                    if (line.Contains(closedLine))
                    {
                        closedLineCount++;
                        streamWriter.WriteLine(line);
                        continue;
                    }
                }
                // если одно закрытие найдено, то могут быть еще. обнуляем и продолжаем искать
                if (isClosedLineFound) isClosedLineFound = isOpenLineFound = false;
                // наверное, тут еще куча таких тегов, о которых я знать не знаю, но пока так. заплаточка
                // убрать весь свет нафиг
                if (line.Contains("<AmbientLight")) continue;
                if (line.Contains("<DirectionalLight")) continue;

                // if we haven't found Open Line
                if (!isOpenLineFound)
                {
                    //if curr line is Open Line -> open line is found, write that line
                    if (line.Contains(openLine))
                    {
                        isOpenLineFound = true;
                        openLineCount++;
                        //add KEY word for 1st match
                        if (openLineCount == 1)
                        {
                            streamWriter.WriteLine($"<Model3DGroup x:Key=\"{filename}\" >");
                        }
                        else
                        {
                            // write "<Model3DGroup >" instead of "<Model3DGroup x:Name="Box04OR24">"
                            // comments r usually started with new line
                            // and this line: "<Model3DGroup x:Name="Box04OR24">" is usually closed with ">"
                            streamWriter.WriteLine("<Model3DGroup >");
                        }
                    }
                }
                // if we have found Open line, but not the Closed One
                else if (!isClosedLineFound)
                {
                    // if curr line is Closed line -> closed line is found, write that line
                    if (line.Contains(closedLine))
                    {
                        isClosedLineFound = true;
                        closedLineCount++;
                    }
                    streamWriter.WriteLine(line);
                }
            }
            streamWriter.WriteLine(wrapClosedLine);
        }
    }
    // Delete original file
    File.Delete(filename);

    // ... and put the temp file in its place.
    File.Move(tempFilename, filename);
}
*/
/*
private void CreateScene()
{
    Model3DGroup model; //= HelixToolkit.Wpf.ModelImporter.Load(@"C:/Users/Anastasiia/source/repos/My_3d_WFP_App/cnek.obj",   );
    ModelImporter importer = new ModelImporter();
    model = new Model3DGroup();
    Model3D chair = importer.Load("C:/Users/Anastasiia/source/repos/My_3d_WFP_App/cnek.obj");
    model.Children.Add(chair);
}*/
