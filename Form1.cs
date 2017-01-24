using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;





namespace Lines
{


    public partial class Form1 : Form
    {

        // Declare costants
        private const int MAX_SHAPES = 1000;
        private const int MAX_POINTS = 1000;

        // Declare variables
        private PictureBox pic_box;
        private EState state = EState.EST_FREE;
        private List<Shape> shapes_list = new List<Shape>();
        private int selected_shape = -1;
        private int mouse_on_shape_n = -1;
        private string current_filename = "noname";
        private int tolerance;




        // Enumeration EState used to keep track of the application's state
        enum EState
        {
            EST_FREE,
            EST_SHAPE_SELECTED,
            EST_ADDING_POINT,
            EST_POINT_SELECTED,
            EST_MOVING_POINT
        };




        // Class Constructor
        public Form1()
        {
            InitializeComponent();

            // Initialise variables
            state = EState.EST_FREE;
            shapes_list = new List<Shape>();
            selected_shape = -1;
            mouse_on_shape_n = -1;
            current_filename = "noname";
            tolerance = 4;

            // Set Form events callback
            this.KeyDown += Form_KeyDownEvent;

            // Set up Menu
            MainMenu main_menu = new MainMenu();

            MenuItem menu_file = main_menu.MenuItems.Add("&File");
            MenuItem menu_new_file = menu_file.MenuItems.Add("&New");
            MenuItem menu_open_file = menu_file.MenuItems.Add("&Open");
            MenuItem menu_save_file = menu_file.MenuItems.Add("&Save");
            MenuItem menu_save_as_file = menu_file.MenuItems.Add("&Save as");

            MenuItem menu_help = main_menu.MenuItems.Add("&Help");

            this.Menu = main_menu;

            // Set Menu events callback
            menu_new_file.Click += MenuNewFileClick;
            menu_open_file.Click += MenuOpenFileClick;
            menu_save_file.Click += MenuSaveFileClick;
            menu_save_as_file.Click += MenuSaveAsFileClick;
            menu_help.Click += MenuHelpClick;

            // Add PictureBox
            pic_box = new PictureBox();
            pic_box.Width = this.ClientSize.Width;
            pic_box.Height = this.ClientSize.Height;
            Controls.Add(pic_box);

            // Set PictureBox events callback
            pic_box.Paint += PictureBox_Paint;
            pic_box.MouseClick += PictureBox_MouseClick;
            pic_box.MouseDown += PictureBox_MouseDown;
            pic_box.MouseMove += PictureBox_MouseMove;
            pic_box.MouseUp += PictureBox_MouseUp;
        }




        // Form keydown event callback
        private void Form_KeyDownEvent(object sender, KeyEventArgs e)
        {
            // Delete the previous point
            if (e.KeyCode == Keys.Space && state == EState.EST_ADDING_POINT)
            {
                shapes_list[selected_shape].DeleteLastPoint();
                pic_box.Refresh();
            }

            // Delete selected shape
            else if (e.KeyCode == Keys.Delete && state == EState.EST_SHAPE_SELECTED)
            {
                shapes_list.RemoveAt(selected_shape);
                selected_shape = -1;
                state = EState.EST_FREE;
                pic_box.Refresh();
            }

            // Delete selected point
            else if (e.KeyCode == Keys.Delete && state == EState.EST_POINT_SELECTED)
            {
                shapes_list[selected_shape].DeleteSelectedPoint();
                state = EState.EST_SHAPE_SELECTED;
                pic_box.Refresh();
            }

            // Turn in adding point mode
            else if (e.KeyCode == Keys.A &&
                     (state == EState.EST_SHAPE_SELECTED || state == EState.EST_POINT_SELECTED))
            {
                // Turn straight in adding point mode if the shape is open
                if (shapes_list[selected_shape].closed_shape == false)
                {
                    state = EState.EST_ADDING_POINT;
                    shapes_list[selected_shape].tmp_point =
                        new Point(Cursor.Position.X - PointToScreen(pic_box.Location).X,
                                  Cursor.Position.Y - PointToScreen(pic_box.Location).Y);
                    pic_box.Refresh();
                }

                // If the shape is closed ask the user if wish to open it
                else
                {
                    DialogResult result = MessageBox.Show("Cannot add points in a close shape.\n" +
                                                          "Do you wish to open the shape?",
                                                          "Error",
                                                          MessageBoxButtons.YesNo,
                                                          MessageBoxIcon.Stop);
                    if(result == DialogResult.Yes)
                    {
                        shapes_list[selected_shape].closed_shape = false;

                        state = EState.EST_ADDING_POINT;
                        shapes_list[selected_shape].tmp_point =
                            new Point(Cursor.Position.X - PointToScreen(pic_box.Location).X,
                                      Cursor.Position.Y - PointToScreen(pic_box.Location).Y);
                        pic_box.Refresh();
                    }
                }
            }

            // Close selcted shape
            else if (e.KeyCode == Keys.C &&
                     (state == EState.EST_SHAPE_SELECTED || state == EState.EST_POINT_SELECTED ||
                      state == EState.EST_MOVING_POINT || state == EState.EST_ADDING_POINT))
            {
                if(shapes_list[selected_shape].closed_shape == false)
                {
                    shapes_list[selected_shape].closed_shape = true;
                    shapes_list[selected_shape].tmp_point = new Point(-1, -1);

                    if (state == EState.EST_ADDING_POINT)
                    {
                        state = EState.EST_SHAPE_SELECTED;
                    }

                    pic_box.Refresh();
                }
            }

            // Open selcted shape
            else if (e.KeyCode == Keys.O &&
                     (state == EState.EST_SHAPE_SELECTED || state == EState.EST_POINT_SELECTED ||
                      state == EState.EST_MOVING_POINT))
            {
                if(shapes_list[selected_shape].closed_shape == true)
                {
                    shapes_list[selected_shape].closed_shape = false;
                    pic_box.Refresh();
                }
            }

        }




        // Menu new file event callback
        private void MenuNewFileClick(object sender, System.EventArgs e)
        {
            shapes_list.Clear();
            selected_shape = -1;
            pic_box.Refresh();
        }




        // Menu open file event callback
        private void MenuOpenFileClick(object sender, System.EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Binary File (*.bin)|*.bin";
            dlg.Title = "Open File";
            dlg.ShowDialog();

            LoadFile(dlg.FileName);
            pic_box.Refresh();
        }




        // Menu save file event callback
        private void MenuSaveFileClick(object sender, System.EventArgs e)
        {
            if (current_filename == "noname")
            {
                string f_name = GetFilenameByDialog();
                if (WriteFile(f_name) == true)
                {
                    current_filename = f_name;
                }
            }
            else
            {
                WriteFile(current_filename);
            }
        }




        // Menu save file as event callback
        private void MenuSaveAsFileClick(object sender, System.EventArgs e)
        {
            string f_name = GetFilenameByDialog();
            if (WriteFile(f_name) == true)
            {
                current_filename = f_name;
            }
        }




        // Menu help event callback
        private void MenuHelpClick(object sender, System.EventArgs e)
        {
            MessageBox.Show("-Left click to add a new shape or point\n" +
                            "-Spacebar when adding points to remove the previous point\n" +
                            "-Right click to end adding point mode\n" +
                            "-A when a shape is selected to get in adding point mode\n" +
                            "-C to close the selected shape\n" +
                            "-O to open the selected shape\n" +
                            "-Delete to delete the selected shape or point\n",
                            "Guide", MessageBoxButtons.OK, MessageBoxIcon.None);
        }




        // Picture box paint event callback
        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            pic_box.BackColor = Color.DarkGray;

            for(int i = 0; i < shapes_list.Count; i++)
            {
                shapes_list[i].Draw(e, i == selected_shape);
            }
        }




        // Picture box mouse click event callback
        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                
                // Add a point to the shape
                if (state == EState.EST_ADDING_POINT)
                {
                    if(shapes_list[selected_shape].points_list.Count < MAX_POINTS)
                    {
                        // If the mouse is above the first point close the shape
                        if(shapes_list[selected_shape].MouseOnFirstPoint(e.X, e.Y, tolerance) == true)
                        {
                            shapes_list[selected_shape].closed_shape = true;
                            shapes_list[selected_shape].tmp_point = new Point(-1, -1);
                            state = EState.EST_SHAPE_SELECTED;
                            pic_box.Refresh();
                        }
                        // The mouse is not above the first point so add a new point to the shape
                        else
                        {
                            shapes_list[selected_shape].AddPoint(e.X, e.Y);
                            pic_box.Refresh();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Limit of points reached for this shape", "Warning",
                                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                
                else if(state == EState.EST_FREE)
                {
                    // Initialise a new shape and add the first point
                    if (MouseOnAShape(e.X, e.Y) == false)
                    {
                        if(shapes_list.Count < MAX_SHAPES)
                        {
                            selected_shape = shapes_list.Count;
                            shapes_list.Add(new Shape());
                            shapes_list[selected_shape].AddPoint(e.X, e.Y);

                            state = EState.EST_ADDING_POINT;
                            pic_box.Refresh();
                        }
                        else
                        {
                            MessageBox.Show("Limit of shapes reached", "Warning",
                                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                    }
                }
                
                else if(state == EState.EST_SHAPE_SELECTED)
                {
                    // Select the shape point pointed by the mouse
                    if (shapes_list[selected_shape].MouseClickOnAPoint(e.X, e.Y, tolerance) == true)
                    {
                        state = EState.EST_POINT_SELECTED;
                        pic_box.Refresh();

                    }
                    
                    else if (MouseOnAShape(e.X, e.Y) == true)
                    {
                        // Add an intermediate point
                        if (mouse_on_shape_n == selected_shape)
                        {
                            shapes_list[selected_shape].AddIntermediatePoint(e.X, e.Y);
                            pic_box.Refresh();
                        }

                        // Select another shape
                        else
                        {
                            selected_shape = mouse_on_shape_n;
                            pic_box.Refresh();
                        }
                    }

                    // Deselect the shape on selection
                    else
                    {
                        state = EState.EST_FREE;
                        selected_shape = -1;
                        pic_box.Refresh();
                    }

                }

                else if(state == EState.EST_POINT_SELECTED)
                {
                    if (MouseOnAShape(e.X, e.Y) == true &&
                        shapes_list[selected_shape].MouseClickOnAPoint(e.X, e.Y, tolerance) == false)
                    {
                        // Add an intermediate point
                        if (mouse_on_shape_n == selected_shape)
                        {
                            shapes_list[selected_shape].AddIntermediatePoint(e.X, e.Y);
                            shapes_list[selected_shape].selected_point = -1;
                            state = EState.EST_SHAPE_SELECTED;
                            pic_box.Refresh();
                        }

                        // Select another shape
                        else
                        {
                            selected_shape = mouse_on_shape_n;
                            pic_box.Refresh();
                        }
                    }
                    
                    // Deselect the selected point
                    else if (shapes_list[selected_shape].MouseClickOnAPoint(e.X, e.Y, tolerance) == 
                                                                                                false)
                    {
                        state = EState.EST_SHAPE_SELECTED;
                        pic_box.Refresh();
                    }

                    // Select another point
                    else if (shapes_list[selected_shape].MouseClickOnAPoint(e.X, e.Y, tolerance) ==
                                                                                               true)
                    {
                        state = EState.EST_POINT_SELECTED;
                        pic_box.Refresh();
                    }
                }

                // Select the shape pointed by the mouse
                if (state != EState.EST_MOVING_POINT && state != EState.EST_ADDING_POINT &&
                    state != EState.EST_POINT_SELECTED && MouseOnAShape(e.X, e.Y) == true)
                {
                    state = EState.EST_SHAPE_SELECTED;
                    selected_shape = mouse_on_shape_n;
                    pic_box.Refresh();
                }

            }

            else if (e.Button == MouseButtons.Right)
            {
                // End adding points to the shape
                if (state == EState.EST_ADDING_POINT)
                {
                    state = EState.EST_SHAPE_SELECTED;
                    shapes_list[selected_shape].tmp_point = new Point(-1, -1);
                    pic_box.Refresh();
                }
            }

        }




        // Picture box mouse down event callback
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Star moving the selected point
            if (e.Button == MouseButtons.Left && state == EState.EST_POINT_SELECTED &&
                shapes_list[selected_shape].MouseOnSelectedPoint(e.X, e.Y, tolerance) == true)
            {
                state = EState.EST_MOVING_POINT;
                Cursor.Current = Cursors.Hand;
            }
        }




        // Picture box mouse move event callback
        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Move the temporary new point
            if (state == EState.EST_ADDING_POINT && selected_shape != -1)
            {
                shapes_list[selected_shape].tmp_point = new Point(e.X, e.Y);
                pic_box.Refresh();
            }

            // Move the selected point
            else if (state == EState.EST_MOVING_POINT)
            {
                shapes_list[selected_shape].MoveSelectedPoint(new Point(e.X, e.Y));
                pic_box.Refresh();
            }
        }




        // Picture box mouse up event callback
        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            // End moving the selected point
            if (e.Button == MouseButtons.Left && state == EState.EST_MOVING_POINT)
            {
                state = EState.EST_POINT_SELECTED;
                Cursor.Current = Cursors.Default;

                if(shapes_list[selected_shape].LastPointOnFirstPoint(tolerance) == true)
                {
                    shapes_list[selected_shape].DeleteSelectedPoint();
                    state = EState.EST_SHAPE_SELECTED;
                    shapes_list[selected_shape].closed_shape = true;
                    pic_box.Refresh();
                }
            }
        }




        // Check if the mouse is above one of the shapes and if the state is in EST_SHAPE_SELECTED
        //   or EST_POINT_SELECTED add a point at the mouse position
        // Param mx: mouse position x coordinate
        // Param my: mouse position y coordinate
        // Return: true if the mouse is above one of the shapes false otherwise
        private bool MouseOnAShape(int mx, int my)
        {
            for (int i = 0; i < shapes_list.Count; i++)
            {
                if (shapes_list[i].MouseOnTheShape(mx, my, tolerance ) == true)
                {
                    mouse_on_shape_n = i;
                    return true;
                }
            }
            return false;
        }




        // Ask the user to choose a file path using the standard windows dialog
        // Return: file path
        private string GetFilenameByDialog()
        {
            string filename = "";

            while(filename == "")
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Binary File (*.bin)|*.bin";
                dlg.Title = "Save File";
                dlg.ShowDialog();

                filename = dlg.FileName;
                if(filename == "")
                {
                    MessageBox.Show("Select a valid filename", "Error", MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                }
            }

            return filename;
        }




        // Write a file
        // Param filename: file path
        // Return: true if success false otherwise
        private bool WriteFile(string filename)
        {
            BinaryWriter bw;
            try
            {
                bw = new BinaryWriter(new FileStream(filename, FileMode.Create));
            }
            catch(IOException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            // Write header
            string header = "File custom lines type";
            bw.Write(header);

            // Write the number of shapes in the file
            Int16 shapes_count = (Int16)shapes_list.Count;
            bw.Write(shapes_count);

            for (int i = 0; i < shapes_count; i++)
            {
                // Write the number of points in the shape
                Int16 points_count = (Int16)shapes_list[i].points_list.Count;
                bw.Write(points_count);

                // Write shape points
                for (int j = 0; j < points_count; j++)
                {
                    Int16 x = (Int16)shapes_list[i].points_list[j].X;
                    bw.Write(x);

                    Int16 y = (Int16)shapes_list[i].points_list[j].Y;
                    bw.Write(y);
                }
            }

            bw.Close();
            return true;
        }




        // Open a file
        // Param filename: file path
        // Return: true if success false otherwise
        private bool LoadFile(string filename)
        {
            shapes_list.Clear();
            selected_shape = -1;

            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(filename, FileMode.Open));
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Read header and if is an unknow file format return false
            string header = br.ReadString();
            if(header != "File custom lines type")
            {
                MessageBox.Show("Unknown file format", "Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return false;
            }

            // Read the number of shapes in the file
            Int16 shapes_count = br.ReadInt16();
            for(int i = 0; i < shapes_count; i++)
            {
                shapes_list.Add(new Shape());

                // Read the number of points in the shape
                Int16 points_count = br.ReadInt16();
                for(int j = 0; j < points_count; j++)
                {
                    // Load shape points
                    Int16 x = br.ReadInt16();
                    Int16 y = br.ReadInt16();
                    shapes_list[i].AddPoint(x, y);
                }
            }

            br.Close();
            return true;
        }

    }
}
