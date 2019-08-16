using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iRacingDash.Helpers
{
    public class FormManipulator
    {
        private Form1 dashForm;

        public FormManipulator(Form1 form)
        {
            this.dashForm = form;
        }

        public Panel CreateSettingWindow(Point location, Color color, Size size, bool visible)
        {
            Panel panel = new Panel();
            panel.Location = location;
            panel.BackColor = color;
            panel.Size = size;
            panel.Visible = visible;

            if (!dashForm.Controls.Contains(panel))
                dashForm.Controls.Add(panel);

            return panel;
            //settingsPanel.BringToFront();
        }

        public Label CreateLabel(string name, string text, Size size, Point location, Color foreColor, Color backColor,
            Font font,
            bool visible)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = location;
            label.Visible = visible;
            label.ForeColor = foreColor;
            label.BackColor = backColor;
            label.Font = font;
            label.Name = name;
            label.Size = size;

            dashForm.Controls.Add(label);

            return label;
        }
    }
}