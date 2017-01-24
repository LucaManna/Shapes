using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;




namespace Lines
{

    class Shape
    {

        // Declare variables
        public List<Point> points_list;
        public Point tmp_point;
        public bool closed_shape;
        public int selected_point;



        // Class Constructor
        public Shape()
        {
            // Initialise variables
            points_list = new List<Point>();
            tmp_point = new Point(-1, -1);
            closed_shape = false;
            selected_point = -1;
        }




        // Add a new point to the shape
        // Param x: x coordinate of the point
        // Param y: y coordinate of the point
        // No return
        public void AddPoint(int x, int y)
        {
            points_list.Add(new Point(x, y));
        }




        // Add a new intermediate point point to the shape
        // Param x: x coordinate of the point
        // Param y: y coordinate of the point
        // No return
        public void AddIntermediatePoint(int x, int y)
        {
            // Check if the coordinates are on the shape by examinating the shape's lines one by one
            for (int i = 1; i < points_list.Count; i++)
            {
                if (MouseOnTheLine(x, y, points_list[i - 1].X, points_list[i - 1].Y,
                                  points_list[i].X, points_list[i].Y, 6) == true)
                {
                    points_list.Insert(i, new Point(x, y));
                    return;
                }
            }

            // If the shape is closed check the line between the last point and the first one
            if (closed_shape == true)
            {
                if (MouseOnTheLine(x, y,
                                   points_list[points_list.Count - 1].X,
                                   points_list[points_list.Count - 1].Y,
                                   points_list[0].X, points_list[0].Y, 6) == true)
                {
                    AddPoint(x, y);
                    return;
                }
            }
        }




        // Remove the last point added to the shape
        // No return
        public void DeleteLastPoint()
        {
            if (points_list.Count > 1)
            {
                points_list.RemoveAt(points_list.Count - 1);
            }
        }




        // Check if the mouse is above the shape (on a shape's line) with a certain tolerance
        // Param mx: mouse x coordinate
        // Param my: mouse y coordinate
        // Param tolerance: tolerance in pixel
        // Return: true if the mouse is above the shape false otherwise
        public bool MouseOnTheShape(int mx, int my, int tolerance)
        {
            // Check if the mouse is on the shape by examinating the shape's lines one by one
            for (int i = 1; i < points_list.Count; i++)
            {
                if(MouseOnTheLine(mx, my, points_list[i - 1].X, points_list[i - 1].Y,
                                  points_list[i].X, points_list[i].Y, tolerance) == true)
                {
                    return true;
                }
            }

            // If the shape is closed check the line between the last point and the first one
            if(closed_shape == true)
            {
                if (MouseOnTheLine(mx, my,
                                   points_list[points_list.Count -1].X,
                                   points_list[points_list.Count - 1].Y,
                                   points_list[0].X, points_list[0].Y, tolerance) == true)
                {
                    return true;
                }
            }

            return false;
        }




        // Check if the mouse is above a line
        // Param mx: mouse x coordinate
        // Param my: mouse y coordinate
        // Param x1: x coordinate of the first line's point
        // Param y1: y coordinate of the first line's point
        // Param x2: x coordinate of the second line's point
        // Param y2: y coordinate of the second line's point
        // Param tolerance: tolerance in pixel
        // Return: true if the mouse is above the line false otherwise
        private bool MouseOnTheLine(int mx, int my, int x1, int y1, int x2, int y2, int tolerance)
        {
            double v = Math.Abs(((x2 - x1) * (y1 - my)) - ((x1 - mx) * (y2 - y1)));
            double d = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            // Distance of the mouse position from the first line's point
            double d1 = Math.Sqrt(((mx - x1) * (mx - x1)) + ((my - y1) * (my - y1)));
            // Distance of the mouse position from the second line's point
            double d2 = Math.Sqrt(((mx - x2) * (mx - x2)) + ((my - y2) * (my - y2)));

            if ((v / d) <= tolerance && d + tolerance >= d1 && d + tolerance >= d2)
            {
                return true;
            }

            return false;
        }
        
        
        
        
        // Check if the mouse is above one of the shape's points with a certain tolerance
        // Param mx: mouse x coordinate
        // Param my: mouse y coordinate
        // Param tolerance: tolerance in pixel
        // Return: true if the mouse is above one of the points false otherwise
        public bool MouseClickOnAPoint(int mx, int my, int tolerance)
        {
            // Examine each shape's points one by one
            for (int i = 0; i < points_list.Count; i++)
            {
                double px = points_list[i].X;
                double py = points_list[i].Y;
                // Distance between mouse position and the point
                double d = Math.Sqrt(((mx - px) * (mx - px)) + ((my - py) * (my - py)));
                if(d <= tolerance)
                {
                    selected_point = i;
                    return true;
                }
            }

            selected_point = -1;
            return false;
        }




        // Check if the mouse is above the selected points with a certain tolerance
        // Param mx: mouse x coordinate
        // Param my: mouse y coordinate
        // Param tolerance: tolerance in pixel
        // Return: true if the mouse is above the selected point false otherwise
        public bool MouseOnSelectedPoint(int mx, int my, int tolerance)
        {
            // Check that a point was selected if not return false
            if(selected_point == -1)
            {
                return false;
            }

            double px = points_list[selected_point].X;
            double py = points_list[selected_point].Y;
            // Distance between mouse position and the point
            double d = Math.Sqrt(((mx - px) * (mx - px)) + ((my - py) * (my - py)));
            if (d <= tolerance)
            {
                return true;
            }

            return false;
        }




        // Check if the mouse is above the first point to determine if the shape
        //    must be closed
        // Param mx: mouse x coordinate
        // Param my: mouse y coordinate
        // Param tolerance: tolerance in pixel
        // Return: true if the mouse is above the first point false otherwise
        public bool MouseOnFirstPoint(int mx, int my, int tolerance)
        {
            // First point coordinates
            double fp_x = points_list[0].X;
            double fp_y = points_list[0].Y;

            // Distance between mouse position and the last point
            double d = Math.Sqrt(((mx - fp_x) * (mx - fp_x)) + ((my - fp_y) * (my - fp_y)));
            if (d <= tolerance)
            {
                return true;
            }

            return false;
        }




        // Check if the last point is above the first point to determine if the shape
        //    must be closed
        // Param tolerance: tolerance in pixel
        // Return: true if the last point is above the first point false otherwise
        public bool LastPointOnFirstPoint(int tolerance)
        {
            // Check that a point was selected if not return false
            if (selected_point == -1)
            {
                return false;
            }

            // Selected point coordinates
            double lp_x = points_list[points_list.Count - 1].X;
            double lp_y = points_list[points_list.Count - 1].Y;
            // First point coordinates
            double fp_x = points_list[0].X;
            double fp_y = points_list[0].Y;

            // Distance between first and last point
            double d = Math.Sqrt(((lp_x - fp_x) * (lp_x - fp_x)) + ((lp_y - fp_y) * (lp_y - fp_y)));
            if (d <= tolerance)
            {
                return true;
            }

            return false;
        }




        // Move the selected point
        // Param new_position: coordinates of the new point position
        // No return
        public void MoveSelectedPoint(Point new_position)
        {
            // Check that a point was selected
            if (selected_point != -1)
            {
                points_list[selected_point] = new_position;
            }
        }




        // Delete the selected point
        // No return
        public void DeleteSelectedPoint()
        {
            // Check that a point was selected
            if (selected_point != -1)
            {
                points_list.RemoveAt(selected_point);
                selected_point = -1;
            }
        }




        // Draw the shape
        // Param e: PaintEventArgs to draw with
        // Param is_selected: true if the shape is selected false if it is not
        // No return
        public void Draw(PaintEventArgs e, bool is_selected)
        {
            Pen pen = new Pen(Color.Black, 1f);

            // If the shape is selected draw the points
            if (is_selected == true)
            {
                pen.Color = Color.Red;

                for (int i = 0; i < points_list.Count; i++)
                {
                    // If the current point is selected draw it yellow otherwise red
                    if (selected_point == i)
                    {
                        pen.Color = Color.Yellow;
                        e.Graphics.DrawEllipse(pen, points_list[i].X - 4, points_list[i].Y - 4, 8, 8);
                        pen.Color = Color.Red;
                    }
                    else
                    {
                        e.Graphics.DrawEllipse(pen, points_list[i].X - 2, points_list[i].Y - 2, 4, 4);
                    }
                }

                // If in adding point mode draw the temporary line
                if(tmp_point.X != -1 && points_list.Count > 0)
                {
                    pen.Color = Color.Yellow;
                    e.Graphics.DrawLine(pen, points_list[points_list.Count -1], tmp_point);
                    pen.Color = Color.Red;
                }
            }

            // Draw shape's lines
            for(int i = 1; i < points_list.Count; i++)
            {
                e.Graphics.DrawLine(pen, points_list[i -1], points_list[i]);
            }

            // If the shape is closed draw a line between the last and the first point
            if(closed_shape == true)
            {
                e.Graphics.DrawLine(pen, points_list[points_list.Count - 1], points_list[0]);
            }
        }

    }
}
