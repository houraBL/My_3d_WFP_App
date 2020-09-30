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
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace My_3d_WFP_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
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
                try
                {
                    var unicName = Path.GetFileNameWithoutExtension(path);
                    var almostUnicName = unicName;
                    var unicNumber = 0;
                    while (names.Contains(almostUnicName))
                    {
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
                catch (FileNotFoundException e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
                catch (DirectoryNotFoundException e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
            }

            public void AddElementFromArray(string[] modelData)
            {
                try
                {
                    if (modelData.Length != 9) throw new Exception("Model Data has more lements then expected");
                    names.Add(modelData[0]);
                    paths.Add(modelData[1]);
                    transformations.Add(new double[7] { Convert.ToDouble(modelData[2]), Convert.ToDouble(modelData[3]), Convert.ToDouble(modelData[4]),
                    Convert.ToDouble(modelData[5]), Convert.ToDouble(modelData[6]), Convert.ToDouble(modelData[7]), Convert.ToDouble(modelData[8])});
                    scene.Children.Add(new HelixToolkit.Wpf.ModelImporter().Load(modelData[1]));
                    this.Transform(names.Last());
                }
                catch (FileNotFoundException e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
                catch (DirectoryNotFoundException e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: Could not add Model. Original error: " + e.Message);
                }
            }

            public void ClearScene()
            {
                while(names.Count!=0)
                {
                    DeleteElement(names.Last());
                }
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
                    transformations.RemoveAt(names.IndexOf(selectedItem));
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
            Ceiling.Visible = !Ceiling.Visible;
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

        #region Menu, Saves and Resent Progects
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            // если на сцене что-то есть
            // предложить сохранить - диалог() - ответы - свитч
            //если да - SaveProject()
            //если нет - не сохранять, создать_новый_проект()
            //отмена - закрыть окошко
            // если на сцене пусто
            // создать новую сцену с дефолтными настройками
            if (lvScene.Items.Count != 0)
            {
                // предложить сохранить текущий проект
                MessageBoxResult r = (MessageBoxResult)MessageBox.Show("Would you like to save current Project?", "Closing program", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (r)
                {
                    case MessageBoxResult.Yes:
                        {
                            SaveProject();
                            MessageBox.Show("Project Saved Sucsessfully", "Project Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            myScene.ClearScene();
                            
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            myScene.ClearScene();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                lvScene.ItemsSource = myScene.Names;
                lvScene.Items.Refresh();
            }
        }

        private void OpenResent_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = (MessageBoxResult)MessageBox.Show("Would you like to save current Project?", "Closing program", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (r)
            {
                case MessageBoxResult.Yes:
                    {
                        SaveProject();
                        MessageBox.Show("Project Saved Sucsessfully", "Closing program", MessageBoxButtons.OK, MessageBoxIcon.Information);                        
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            OpenProject();            
        }

        private void OpenProject()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "My3Dformat files (*.my3Dformat)|*.my3Dformat";
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true)
            {
                string filename = Path.GetFileName(fileDialog.FileName);
                string path = Path.GetDirectoryName(fileDialog.FileName);

                // открыть файлы со специальным расширением
                // и прочитать их построчно :))))
                // Read file
                using (var streamReader = new StreamReader(fileDialog.FileName))
                {
                    string line;
                    string floorW = "floorWidth ";
                    string floorL = "floorLength ";
                    string ceilingV = "ceiling ";
                    string modelsC = "models ";
                    bool floorWFound = false, floorLFound = false, ceilingVFound = false, modelsCFound = false;
                    int modelsCount = 0;
                    myScene.ClearScene();

                    // do while its not the end of file
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        #region scene settings
                        if (!floorWFound && line.IndexOf(floorW) != -1)
                        {
                            FloorWidth.Value = Ceiling.Width = Convert.ToDouble(line.Substring(line.IndexOf(floorW) + floorW.Length));
                            floorWFound = !floorWFound;
                            continue;
                        }
                        if (!floorLFound && line.IndexOf(floorL) != -1)
                        {
                            FloorLength.Value = Ceiling.Length = Convert.ToDouble(line.Substring(line.IndexOf(floorL) + floorL.Length));
                            floorLFound = !floorLFound;
                            continue;
                        }
                        if (!ceilingVFound && line.IndexOf(ceilingV) != -1)
                        {
                            Ceiling.Visible = Convert.ToBoolean(line.Substring(line.IndexOf(ceilingV) + ceilingV.Length));
                            ceilingVFound = !ceilingVFound;
                            continue;
                        }
                        if (!modelsCFound && line.IndexOf(modelsC) != -1)
                        {
                            modelsCount = Convert.ToInt32(line.Substring(line.IndexOf(modelsC) + modelsC.Length));
                            modelsCFound = !modelsCFound;
                            continue;
                        }
                        if (line == "name source coordinateX coordinateY coordinateZ rotationX rotationY rotationZ size") continue;
                        #endregion

                        #region model opening

                        string[] delimiters = new string[] { " " };

                        string[] modelData = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        myScene.AddElementFromArray(modelData);

                        #endregion

                    }
                }
                lvScene.ItemsSource = myScene.Names;
                lvScene.Items.Refresh();
            }
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            SaveProject();
        }

        private void SaveProject()
        {
            // собственно сздать файл в котором будут:
            // информация о сцене: размер пола, виден ли потолок
            // количество моделей на сцене
            // название модели - путь - координаты - вращения - размер
            // ??? создать папку и туды закидывать все модели? meh
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".my3Dformat";
            saveFileDialog.FileName = "NewProject";
            Nullable<bool> result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string filename = Path.GetFileName(saveFileDialog.FileName);
                string path = Path.GetDirectoryName(saveFileDialog.FileName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(saveFileDialog.FileName))
                {
                    File.Create(saveFileDialog.FileName).Close();
                }
                string myProject = $"floorWidth {FloorWidth.Value}\nfloorLength {FloorLength.Value}\nceiling {Ceiling.Visible}\nmodels {lvScene.Items.Count}";
                myProject += "\nname source coordinateX coordinateY coordinateZ rotationX rotationY rotationZ size";
                foreach (string model in lvScene.Items)
                {
                    myProject += $"\n{model}" + //name
                        $" {myScene.Paths.ElementAt(myScene.Names.IndexOf(model))}" + //path
                        $" {myScene.GetTransformations(model)[0]} {myScene.GetTransformations(model)[1]} {myScene.GetTransformations(model)[2]}" + //coord x y z
                        $" {myScene.GetTransformations(model)[3]} {myScene.GetTransformations(model)[4]} {myScene.GetTransformations(model)[5]}" + //rot x y z
                        $" {myScene.GetTransformations(model)[6]}"; //size
                }
                File.WriteAllText(saveFileDialog.FileName, myProject);
                // other logic
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // предложить сохранить текущий проект
            MessageBoxResult r = (MessageBoxResult)MessageBox.Show("Would you like to save current Project?", "Closing program", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (r)
            {
                case MessageBoxResult.Yes:
                    {
                        SaveProject();
                        MessageBox.Show("Project Saved Sucsessfully", "Closing program", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        System.Windows.Application.Current.Shutdown();
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        System.Windows.Application.Current.Shutdown();
                        break; 
                    }
                default:
                    {
                        break;
                    }
            }            
        }

        private void Heart_Click(object sender, RoutedEventArgs e)
        {
            //сердечко
            MessageBoxResult heartResult = (MessageBoxResult)MessageBox.Show("I love you", "<3", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (heartResult == MessageBoxResult.OK)
            {
                MessageBox.Show("<3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 <3 ", "<3", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion
    }
}

