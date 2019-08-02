using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StationarySharp
{
    public partial class Form1 : Form
    {
        private Renderer renderer; 
        
         ~Form1() {

            renderer.Exit();
         }

        private void form_closing(object sender, FormClosingEventArgs e) {

            renderer.Exit();
            Application.Exit();
        }

        private IntPtr _WndprocWrap(IntPtr hwnd, int message, IntPtr wparam, IntPtr lparam) {

            Message m = new Message();
            m.Msg = message;
            m.HWnd = hwnd; 
            m.WParam = wparam;
            m.LParam = lparam;

            this.WndProc(ref m);

            return m.Result;
        }

        private float motion = 0;
        private float rotation = 0;

        protected override void OnKeyDown(KeyEventArgs e) {

            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Up)
                motion++;

            if (e.KeyCode == Keys.Down)
                motion--;

            if (e.KeyCode == Keys.Right)
                rotation++;

            if (e.KeyCode == Keys.Left)
                rotation--;

            if (motion > 1)
                motion = 1;

            if (motion < -1)
                motion = -1;

            if (rotation > 1)
                rotation = 1;

            if (rotation < -1)
                rotation = -1;
        }

        protected override void OnKeyUp(KeyEventArgs e) {

            base.OnKeyDown(e);
            
            if (e.KeyCode == Keys.Up)
                motion--;

            if (e.KeyCode == Keys.Down)
                motion++;

            if (e.KeyCode == Keys.Right)
                rotation--;

            if (e.KeyCode == Keys.Left)
                rotation++;

            if (motion > 1)
                motion = 1;

            if (motion < -1)
                motion = -1;

            if (rotation > 1)
                rotation = 1;

            if (rotation < -1)
                rotation = -1;
        }

        public Form1() {

            InitializeComponent();

            Cube cube = new Cube(1, Color24.FromRGB(255, 255, 255));
            Object3D cube2 = cube.CreateSubdivided();

            cube.Translate(-1.0f, -0.7f, 2.0f);
            cube2.Translate(1.0f, -0.7f, 2.0f);

            renderer = new Renderer(this, this._WndprocWrap, () => {

                if (motion != 0) {
                    foreach (Object3D obj in renderer.Objects)
                        obj.Translate(0, 0, -0.2f * motion);
                }

                if (rotation != 0)
                    foreach (Object3D obj in renderer.Objects)
                        obj.RotateYGlobal(-3.0f*rotation);

                //cube.RotateXLocal(1);
                cube.RotateYLocal(1);
                //cube.RotateZLocal(3);

                //cube2.RotateXLocal(-1);
                cube2.RotateYLocal(-1);
                //cube2.RotateZLocal(-3);

                return true;
            });

            renderer.Objects.Add(cube);
            renderer.Objects.Add(cube2);

            this.FormClosing += new FormClosingEventHandler(this.form_closing);
        }
    }
}
