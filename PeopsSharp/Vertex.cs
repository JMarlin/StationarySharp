using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationarySharp {

    class Vertex {

        public float x = 0;
        public float y = 0;
        public float z = 0;
        public Color24 c = Color24.FromRGB(0, 0, 0);
        public bool has_been_transformed = false;
        public Polygon parent_poly = null;

        public Vertex Clone() {

            Vertex return_vert = new Vertex();

            return_vert.x = this.x;
            return_vert.y = this.y;
            return_vert.z = this.z;
            return_vert.c = this.c.Clone();

            return return_vert;
        }

        public void Translate(float x, float y, float z) {

            if (this.has_been_transformed)
                return;

            this.has_been_transformed = true;

            this.x += x;
            this.y += y;
            this.z += z;
        }

        public void RotateXGlobal(float angle) {

            float rad_angle = Renderer.DegreeToRad(angle);
            float temp_y, temp_z;

            if (this.has_been_transformed)
                return;

            this.has_been_transformed = true;

            temp_y = this.y;
            temp_z = this.z;
            this.y = (temp_y * (float)Math.Cos(rad_angle)) - (temp_z * (float)Math.Sin(rad_angle));
            this.z = (temp_y * (float)Math.Sin(rad_angle)) + (temp_z * (float)Math.Cos(rad_angle));
        }

        public void RotateYGlobal(float angle) {

            float rad_angle = Renderer.DegreeToRad(angle);
            float temp_x, temp_z;

            if (this.has_been_transformed)
                return;

            this.has_been_transformed = true;

            temp_x = this.x;
            temp_z = this.z;
            this.x = (temp_x * (float)Math.Cos(rad_angle)) + (temp_z * (float)Math.Sin(rad_angle));
            this.z = (temp_z * (float)Math.Cos(rad_angle)) - (temp_x * (float)Math.Sin(rad_angle));
        }
        public void RotateZGlobal(float angle) {

            float rad_angle = Renderer.DegreeToRad(angle);
            float temp_x, temp_y;

            if (this.has_been_transformed)
                return;

            this.has_been_transformed = true;

            temp_x = this.x;
            temp_y = this.y;
            this.x = (temp_x * (float)Math.Cos(rad_angle)) - (temp_y * (float)Math.Sin(rad_angle));
            this.y = (temp_x * (float)Math.Sin(rad_angle)) + (temp_y * (float)Math.Cos(rad_angle));
        }
    }
}
