using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using ComboBox = Autodesk.Revit.UI.ComboBox;
using Form = System.Windows.Forms.Form;
using Newtonsoft.Json;
using Autodesk.Revit.UI.Selection;


namespace kurs_material
{
    public partial class MainForm : Form
    {
        private ExternalEvent f_ExEvent;
        private CommandHandler f_Handler;
        List<MainBrick> bricksFromToJSON = new List<MainBrick>();
        List<Brick> bricks = new List<Brick>();
        List<ConcreteBlock> cBlocks = new List<ConcreteBlock>();
        private string[] typesOfBricks = new string[] { "Одинарный", "Полуторный", "Двойной" };
        private string[] typesOfFill = new string[] { "В 1 кирпич", "В 1.5 кирпича", "В 2 кирпича", "В 2.5 кирпича" };
        private string[] typesOfСoncreteBlocks= new string[] { "Для несущих", "Для ненесущих" };
        private string[] typesOfMaterial = new string[] { "<Нет>","Бетонный блок", "Кирпич" };
        private Wall wall;
        private UIApplication uiApplication;

        public MainForm(ExternalEvent exEvent, CommandHandler handler, UIApplication uiapp)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            comboBox1.Items.Add("Нет");
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Нет");

            comboBox3.Items.AddRange(typesOfFill);
            comboBox3.SelectedIndex = comboBox3.Items.IndexOf("В 2 кирпича");
            comboBox4.Items.AddRange(typesOfMaterial);
            comboBox4.SelectedIndex = comboBox4.Items.IndexOf("<Нет>");

            f_ExEvent = exEvent;
            f_Handler = handler;
            textBox1.ReadOnly = true;
            uiApplication = uiapp;

            String JSONBricks = File.ReadAllText(@"C:\Users\Sergey\AppData\Roaming\Autodesk\Revit\Addins\2020\Bricks.json");
            String JSONConcreteBlocks = File.ReadAllText(@"C:\Users\Sergey\AppData\Roaming\Autodesk\Revit\Addins\2020\ConcreteBlock.json");
            IEnumerable<Brick> bricksjson = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Brick>>(JSONBricks);
            IEnumerable<ConcreteBlock> cblocks = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ConcreteBlock>>(JSONConcreteBlocks);
            foreach (var elm in bricksjson)
            {
                comboBox1.Items.Add("Кирпич " + elm.Name + " - " + elm.Type);
                bricksFromToJSON.Add(elm);
                bricks.Add(new Brick(elm.Name, elm.Type, elm.InPack));
            }
            foreach (var elm in cblocks)
            {
                comboBox1.Items.Add("Бетонный блок " + elm.Name + " - " + elm.Type);
                cBlocks.Add(elm);
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            f_ExEvent.Dispose();
            f_ExEvent = null;
            f_Handler = null;
            base.OnFormClosed(e);
        }

        // расчет площади
        public double AreaCount()
        {
            Solid wallSolid = null;
            GeometryElement wallGeometry = wall.get_Geometry(new Options());  // Получение геометрии стены
            foreach (var geomObject in wallGeometry)
            {
                if (geomObject is Solid)
                {
                    Solid solid = geomObject as Solid;
                    if (solid.Volume > 0)
                        wallSolid = solid;
                }
                if (geomObject is GeometryInstance geomInst)
                {
                    // Геометрия экземпляра получается таким образом, что пересечение работает должным образом, не требуя преобразования

                    GeometryElement instElem = geomInst.GetInstanceGeometry();

                    foreach (GeometryObject instObj in instElem)
                    {
                        if (instObj is Solid solid)
                        {
                            if (solid.Volume > 0)
                                wallSolid = solid;
                        }
                    }
                }
            }
            var faces = wallSolid.Faces; // получаем поверхности стены
            double ar = 0;
            foreach (Face face in faces)
            {
                var faceArea = face.Area; // складываем площади
                ar += faceArea; // поверхностей 
            }
            string name = comboBox1.Text.Substring(0, 1);
            if (name == "К")
            {
                return ar / 55;
            }
            else
            {
                return ar / 55; // 55 - универсальное число для преобразования объема, который получается с помощью Revit API, в действительный (примерно);
            }
            
        }

        private bool SelectWall() // выбор стены
        {
            Selection selection = uiApplication.ActiveUIDocument.Selection;
            UIDocument uidoc = uiApplication.ActiveUIDocument;
            Document doc = uidoc.Document;
            try
            {
                Reference pickedRef = null;
                pickedRef = selection.PickObject(ObjectType.Element,
                        "Выберите стену");
                Element elem = doc.GetElement(pickedRef);
                if (elem is Wall) // элемент - стена?
                {
                    textBox1.Text = "";
                    wall = (Wall)elem;
                    return true;
                }
                else
                {
                    TaskDialog.Show("Предупреждение", "Выберите стену. Вы выбрали элемент с именем  " + elem.Name);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return false;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == comboBox4.Items.IndexOf("Кирпич")) 
            {
                textBox2.Show();
                label6.Show();
                label8.Show();
                textBox3.Show();
                label7.Show();
                comboBox2.Show();
                label6.Text = "Название кирпича";
                label7.Text = "Тип кирпича";
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(typesOfBricks);
                comboBox2.SelectedIndex = comboBox2.Items.IndexOf("Одинарный");
            }
            else if (comboBox4.SelectedIndex == comboBox4.Items.IndexOf("Бетонный блок"))
            {
                textBox2.Show();
                label6.Show();
                label7.Show();
                comboBox2.Show();
                label8.Hide();
                textBox3.Hide();
                label6.Text = "Название бетонного блока";
                label7.Text = "Тип бетонного блока";
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(typesOfСoncreteBlocks);
                comboBox2.SelectedIndex = comboBox2.Items.IndexOf("Для несущих");
            }
            else
            {
                label7.Hide();
                comboBox2.Hide();
                label8.Hide();
                textBox3.Hide();
                textBox2.Hide();
                label6.Hide();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
                try
                {
                if (comboBox4.Text == "Кирпич")
                {
                    if (textBox2.Text.Replace(" ", string.Empty) != "" && textBox3.Text.Replace(" ", string.Empty) != "" && comboBox4.Text != "<Нет>")
                    {
                        MainBrick newMat = new MainBrick();
                        newMat.Name = textBox2.Text;
                        newMat.Type = comboBox2.Text;
                        newMat.InPack = double.Parse(textBox3.Text);
                        foreach (var material in bricks)
                        {
                            if (newMat.Name == material.Name && newMat.InPack == material.InPack)
                            {
                                MessageBox.Show("Идентичный этому тип кирпича с одинаковым количеством кирпича на м2 уже существует!");
                                return;
                            }
                        }

                        bricksFromToJSON.Add(newMat);
                        bricks.Add(new Brick(newMat.Name, newMat.Type, newMat.InPack));
                        comboBox1.Items.Add("Кирпич " + newMat.Name + " - " + newMat.Type);

                        using (StreamWriter file =
                            File.CreateText(@"C:\Users\Sergey\AppData\Roaming\Autodesk\Revit\Addins\2020\Bricks.json"))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(file, bricksFromToJSON);
                        }

                        MessageBox.Show("Успешное добавление типа кирпича!");
                    }
                    else
                    {
                        TaskDialog.Show("Предупреждение", "Все поля должны быть заполнены!");
                        this.Activate();
                    }
                }
                else
                {
                    if (textBox2.Text.Replace(" ", string.Empty) != "" && comboBox4.Text != "<Нет>")
                    {
                        ConcreteBlock newMat = new ConcreteBlock();
                        newMat.Name = textBox2.Text;
                        newMat.Type = comboBox2.Text;
                        foreach (var material in cBlocks)
                        {
                            if (newMat.Name == material.Name)
                            {
                                MessageBox.Show("Идентичный этому тип бетонного блока уже существует!");
                                return;
                            }
                        }

                        cBlocks.Add(newMat);
                        comboBox1.Items.Add("Бетонный блок " + newMat.Name + " - " + newMat.Type);

                        using (StreamWriter file =
                            File.CreateText(@"C:\Users\Sergey\AppData\Roaming\Autodesk\Revit\Addins\2020\ConcreteBlock.json"))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(file, cBlocks);
                        }

                        MessageBox.Show("Успешное добавление типа бетонного блока!");
                    }
                    else
                    {
                        TaskDialog.Show("Предупреждение", "Все поля должны быть заполнены!");
                        this.Activate();
                    }
                }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
                    throw;
                }
                this.Activate();
        }

        private void computebtn_Click_1(object sender, EventArgs e)
        {
            string name = comboBox1.Text.Substring(0, 1);
            if (comboBox1.Text != "Нет" && label4.Text == "Стена выбрана")
            {
                try
                {
                    if (name == "К")
                    {
                        string bricksName = comboBox1.Text;
                        double amountOfBricks = 0, amountOfPacks = 0;
                        double area = AreaCount();
                        foreach (var brick in bricks)
                        {
                            if ("Кирпич " + brick.Name + " - " + brick.Type == bricksName)
                            {
                                amountOfPacks = Math.Ceiling(area / brick.InPack);
                                switch (comboBox3.Text)
                                {
                                    case "В 1 кирпич":
                                        amountOfBricks = brick.countAmountInM2("В 1 кирпич");
                                        break;
                                    case "В 1.5 кирпича":
                                        amountOfBricks = brick.countAmountInM2("В 1.5 кирпича");
                                        break;
                                    case "В 2 кирпича":
                                        amountOfBricks = brick.countAmountInM2("В 2 кирпича");
                                        break;
                                    case "В 2.5 кирпича":
                                        amountOfBricks = brick.countAmountInM2("В 2.5 кирпича");
                                        break;
                                }
                                textBox1.Text = "На стену вам понадобится " + amountOfPacks + " упаковок данного кирпича, в каждой по " + (amountOfBricks) + " кирпичей (" + (amountOfBricks * amountOfPacks).ToString() + " штук всего).";
                            }

                        }
                    }
                    else if (name == "Б")
                    {
                        string cBlockName = comboBox1.Text;
                        double area = AreaCount();
                        foreach (var block in cBlocks)
                        {
                            if ("Бетонный блок " + block.Name + " - " + block.Type == cBlockName)
                            {
                                textBox1.Text = "На стену вам понадобится " + Math.Ceiling(area / block.Volume) + " штук данных бетонных блоков.";
                            }
                        }
                    }

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
                    throw;
                }
            }
            else if (comboBox1.Text == "Нет")
            {
                TaskDialog.Show("Предупреждение", "Пожалуйста, выберите тип материала!");
            }
            else if (label4.Text != "Стена выбрана")
            {
                TaskDialog.Show("Предупреждение", "Пожалуйста, выберите стену!");
            }
            this.Activate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (SelectWall())
            {
                label4.Text = "Стена выбрана";
            }
            this.Activate();
        }

        private void cnlbtn_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = "";
            string name = comboBox1.Text.Substring(0, 1);
            if (name == "К")
            {
                label9.Show();
                comboBox3.Show();
            }
            else
            {
                label9.Hide();
                comboBox3.Hide();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
