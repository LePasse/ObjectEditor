using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;


namespace OOP2
{

    public partial class Form1 : Form
    {
        List<Control> controls = new List<Control>();
        Object bufObject;
        List<Object> allObj = new List<Object>();
        private void Form1_Load(object sender, EventArgs e)
        {
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Classes").ToArray();
            foreach (Type type in typelist)
            {

                if (type.IsClass)
                {
                    comboBox1.Items.Add(type.Name);
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
            Form1_Load(this,EventArgs.Empty);
        }

        public static Object create_obj(Type type)
        {
            ConstructorInfo[] cons = type.GetConstructors();
            ParameterInfo[] pars = cons[0].GetParameters();
            List<Object> test = new List<Object>();
            if (pars.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                foreach (var para in pars)
                {
                    test.Add(create_obj(para.ParameterType));
                }
            }
            return Activator.CreateInstance(type, test.ToArray());
        }

        public int printF(Object obj,List<Control> controls, int x, int y)
        {
            if (obj != null)
            {
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                controls.Clear();
                
                Type type = obj.GetType();
                var fields = type.GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    Type type2 = Type.GetType(fieldInfo.FieldType.ToString());
                    if (type2.IsEnum)
                    {
                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20), 
                            Location = new Point(0 + x, 10 + y * 30), 
                            Text = fieldInfo.Name
                        });
                        y++;

                        ComboBox buf = new ComboBox()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20), 
                            DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList, 
                            Name = fieldInfo.Name,
                            Location = new Point(0 + x, 10 + y * 30)
                        };

                        FieldInfo[] fieldinfo2 = type2.GetFields(BindingFlags.Public | BindingFlags.Static);
                        foreach (var field2 in fieldinfo2)
                        {
                            buf.Items.Add(field2.Name.ToString());
                        }
                        controls.Add(buf);
                        y++;
                    }
                    else if ((type2.IsClass) && (type2.Name != "String"))
                    {
                        object temp = create_obj(type2);
                        y = printF( temp, controls, x, y);
                    }
                    else
                    {
                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Text = fieldInfo.Name
                        });
                        y++;

                        controls.Add(new TextBox
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Name = fieldInfo.Name
                        });
                        y++;
                    }
                }
            }

            return y;
        }

        public object setV(object obj)
        {
            Type type = obj.GetType();
            var fields = type.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                Type t = Type.GetType(fieldInfo.FieldType.ToString());
                //Type t = Type.GetType(fieldInfo.FieldType.ToString());
                if ((t.IsClass) && (t.Name != "String"))
                {
                    object buff = create_obj(t);
                    buff = setV(buff);
                    fieldInfo.SetValue(obj,buff);
                }
                else if (t.IsEnum)
                {
                    var field2 = t.GetFields();
                    
                    foreach (Control control in controls)
                    {
                        foreach (FieldInfo field in field2)
                        {
                            if (field.Name.ToString() == control.Text)
                            {
                                fieldInfo.SetValue(obj, field.GetValue(obj));
                            }
                        }
                    }
                    
                }
                else
                {
                    Object val = 0;
                    foreach (Control control in controls)
                    {
                        if (control.Name.ToString() == fieldInfo.Name.ToString())
                        {
                            val = control.Text;
                        }
                    }

                    try
                    {
                        val = Convert.ChangeType(val, fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, val);
                    }
                    catch
                    {
                        MessageBox.Show("Неправильно заполнено поле " + fieldInfo.Name);
                        return false;
                    }
                }
            }

            return obj;
        }

        public int printO(Object obj, List<Control> controls, int x, int y)
        {
            if (obj != null)
            {
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                controls.Clear();

                Type type = obj.GetType();
                var fields = type.GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    Type type2 = Type.GetType(fieldInfo.FieldType.ToString());
                    if ((type2.IsClass) && (type2.Name != "String"))
                    {
                        y = printO(fieldInfo.GetValue(obj), controls, x, y);
                    }
                    else
                    {
                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Text = fieldInfo.Name
                        });
                        y++;

                        controls.Add(new TextBox
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Name = fieldInfo.Name,
                            ReadOnly = true,
                            Text = fieldInfo.GetValue(obj).ToString()
                        });
                        y++;
                    }
                }
            }

            return y;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                Object obj = create_obj(Type.GetType("Classes." + comboBox1.Text));
                obj = setV(obj);
                foreach (Control control in controls)
                {
                    this.Controls.Remove(control);
                }
                controls.Clear();
                allObj.Add(obj);
                comboBox2.Items.Add(obj.ToString() + allObj.Count());
                
            }
            else MessageBox.Show("Выберите класс");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object buff = allObj[comboBox2.SelectedIndex];
            if (buff != null)
            {
                printO(buff, controls, 200, 1);
            }
            foreach (Control control in controls)
            {
                this.Controls.Add(control);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Control control in controls)
            {
                Controls.Remove(control);
            }
            object buff = allObj[comboBox2.SelectedIndex];
            printF(buff, controls, 200, 1);
            foreach (Control control in controls)
            {
                Type t = buff.GetType();
                var fields = t.GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    if (fieldInfo.Name.ToString() == control.Name.ToString())
                    {
                        control.Text = fieldInfo.GetValue(buff).ToString();
                    }
                }
            }
            foreach (Control control in controls)
            {
                this.Controls.Add(control);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (Control control in controls)
            {
                Controls.Remove(control);
            }
            controls.Clear();
            object buff = allObj[comboBox2.SelectedIndex];
            comboBox2.Items.Remove(comboBox2.SelectedItem);
            comboBox2.SelectedIndex = -1;
            allObj.Remove(buff);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                Object obj = create_obj(Type.GetType("Classes." + comboBox1.Text));
                printF(obj, this.controls, 200, 1);
            }
            foreach (Control control in controls)
            {
                this.Controls.Add(control);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            object buff = allObj[comboBox2.SelectedIndex];
            allObj[comboBox2.SelectedIndex] = setV(buff);
            foreach (Control control in controls)
            {
                this.Controls.Remove(control);
            }
            controls.Clear();
        }
    }
}

namespace Classes
{
    class Ammo
    {
        public int speed;
        public int range;

        public enum T
        {
            LRN,
            WC,
            FMJ,
        }

        public T type;
    }

    class ShotgunAmmo
    {
        public int speed;
        public int range;

        public enum T
        {
            Buckshot,
            Slug,
        }
        public T type;
    }

    class Weapon
    {
        public int lethality;
        public int weight;
        public int model;
    }

    class Melee : Weapon
    {
        public int length;

        public enum T
        {
            destruction,
            erosion,
        }
        public T impact;
    }

    class Special : Weapon
    {
        public int range;
        public enum T
        {
            SniperRifle,
            FusionRifle,
            Sidearm,
        }
        public T type;
    }

    class Firearm : Weapon
    {
        public int rateOfFire;
        public int magCapacity;

        public enum T
        {
            GAP,
            M24,
        }

        public T barrel;
    }

    class Handgun : Firearm
    {
        public Ammo ammo;

        public enum C
        {
            mm4,
            mm5,
            mm6,
            mm7,
        }

        public enum T
        {
            Colt,
            CZ,
            FN,
        }

        public C caliber;
        public T type;
    }

    class Rifle : Firearm
    {
        public Ammo ammo;

        public enum T
        {
            Hunting,
            Fishing,
            Optics,
        }
        public T type;
    }

    class Shotgun : Firearm
    {
        public ShotgunAmmo ammo;

        public enum C
        {
            Gauge1,
            Gauge7,
            Gauge24,
        }

        public enum T
        {
            Breakaction,
            Pumpaction,
            Semiautomatic,
        }
        public C caliber;
        public T type;
    }
}
