using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 内存管理
{
    class MemoryBlock
    {
        private ToolTip blockInfo;
        private Label block;
        private int num;
        private int height = 80;
        private int width;
        private Point loc;
        private GroupBox parent;
        public MemoryBlock(int num, int width, int height, int pos_x, int pos_y, GroupBox parent) 
        {
            this.parent = parent;
            this.num = num;
            this.width = width;
            this.height = height;
            loc = new Point(pos_x, pos_y);

            block = new Label
            {
                Name = parent.Text + num.ToString(),
                Text = num.ToString(),
                Size = new Size(width, height),
                Location = loc,
                Parent = parent,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(234)))), ((int)(((byte)(255))))),
            };

            blockInfo = new ToolTip
            {
                IsBalloon = true
            };
            updateInfo();
        }
        void updateInfo()
        {
            blockInfo.SetToolTip(this.block, "任务编号为: " + this.num.ToString() + ", 大小为: " + this.width.ToString());
        }
        public void BringToFront()
        {
            block.BringToFront();
        }
        public void remove()
        {
            this.parent.Controls.Remove(block);
        }
        public int getNum()
        {
            return this.num;
        }
        public int getWidth()
        {
            return this.width;
        }
        public int getPosX()
        {
            return loc.X - 12;
        }
    }
}
