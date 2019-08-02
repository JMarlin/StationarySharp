using System.Collections.Generic;
using System;

namespace StationarySharp {

    class Polygon {

        public Vertex[] v;
        public TextureCoordinate[] uv;
        public Color24 c;
        public float z;

        public Polygon(Vertex v0, TextureCoordinate uv0, Vertex v1, TextureCoordinate uv1, Vertex v2, TextureCoordinate uv2, Color24 c) {

            this.v = new Vertex[3];
            this.uv = new TextureCoordinate[3];
            this.v[0] = v0;
            this.v[1] = v1;
            this.v[2] = v2;
            this.uv[0] = uv0;
            this.uv[1] = uv1;
            this.uv[2] = uv2;
            this.c = c;
            this.z = 0.0f;

            foreach (Vertex vert in this.v) {

                if (vert.parent_poly != null)
                    Console.Write("ALREADY HAD A PARENT");

                vert.parent_poly = this;
            }
        }

        public Polygon(Vertex v0, TextureCoordinate uv0, Vertex v1, TextureCoordinate uv1, Vertex v2, TextureCoordinate uv2, Vertex v3, TextureCoordinate uv3, Color24 c) {

            this.v = new Vertex[4];
            this.uv = new TextureCoordinate[4];
            this.v[0] = v0;
            this.v[1] = v1;
            this.v[2] = v2;
            this.v[3] = v3;
            this.uv[0] = uv0;
            this.uv[1] = uv1;
            this.uv[2] = uv2;
            this.uv[3] = uv3;
            this.c = c;
            this.z = 0.0f;

            foreach (Vertex vert in this.v)
                vert.parent_poly = this;
        }

        public void Translate(float x, float y, float z) {

            foreach (Vertex vertex in this.v)
                vertex.Translate(x, y, z);
        }

        public IEnumerable<Polygon> CreateSubdivided() {

            List<Polygon> return_polys = new List<Polygon>();
            var vert_list = new Vertex[this.v.Length * 2 + (this.v.Length == 4 ? 1 : 0)];
            var uv_list = new TextureCoordinate[this.v.Length * 2 + (this.v.Length == 4 ? 1 : 0)];
            var col = this.c.Clone(); 

            for (int i = 0; i < this.v.Length; i++) {

                vert_list[i] = this.v[i].Clone();
                uv_list[i] = this.uv[i].Clone();
            }

            for (int i = this.v.Length; i < vert_list.Length; i++) {

                vert_list[i] = new Vertex();
                uv_list[i] = new TextureCoordinate();

                if(i == 8) {

                    vert_list[i].x = (vert_list[0].x + vert_list[2].x) / 2;
                    vert_list[i].y = (vert_list[0].y + vert_list[2].y) / 2;
                    vert_list[i].z = (vert_list[0].z + vert_list[2].z) / 2;
                    uv_list[i].u = (byte)((uv_list[0].u + uv_list[2].u) >> 1);
                    uv_list[i].v = (byte)((uv_list[0].v + uv_list[2].v) >> 1);

                    continue;
                }

                vert_list[i].x = (vert_list[i - this.v.Length].x + vert_list[(i + 1 - this.v.Length) % this.v.Length].x) / 2;
                vert_list[i].y = (vert_list[i - this.v.Length].y + vert_list[(i + 1 - this.v.Length) % this.v.Length].y) / 2;
                vert_list[i].z = (vert_list[i - this.v.Length].z + vert_list[(i + 1 - this.v.Length) % this.v.Length].z) / 2;
                uv_list[i].u = (byte)((uv_list[i - this.v.Length].u + uv_list[(i + 1 - this.v.Length) % this.v.Length].u) >> 1);
                uv_list[i].v = (byte)((uv_list[i - this.v.Length].v + uv_list[(i + 1 - this.v.Length) % this.v.Length].v) >> 1);
            }

            if(this.v.Length > 3) {

                return_polys.Add(new Polygon(vert_list[4], uv_list[4], vert_list[1], uv_list[1], vert_list[5], uv_list[5], vert_list[8], uv_list[8], col));
                return_polys.Add(new Polygon(vert_list[8], uv_list[8], vert_list[5], uv_list[5], vert_list[2], uv_list[2], vert_list[6], uv_list[6], col));
                return_polys.Add(new Polygon(vert_list[7], uv_list[7], vert_list[8], uv_list[8], vert_list[6], uv_list[6], vert_list[3], uv_list[3], col));
                return_polys.Add(new Polygon(vert_list[0], uv_list[0], vert_list[4], uv_list[4], vert_list[8], uv_list[8], vert_list[7], uv_list[7], col));
            } else {
                
                return_polys.Add(new Polygon(vert_list[0], uv_list[0], vert_list[3], uv_list[3], vert_list[5], uv_list[5], col));
                return_polys.Add(new Polygon(vert_list[3], uv_list[3], vert_list[4], uv_list[4], vert_list[5], uv_list[5], col));
                return_polys.Add(new Polygon(vert_list[3], uv_list[3], vert_list[1], uv_list[1], vert_list[4], uv_list[4], col));
                return_polys.Add(new Polygon(vert_list[5], uv_list[5], vert_list[4], uv_list[4], vert_list[2], uv_list[2], col));
            }

            return return_polys;
        }

        public void RotateXGlobal(float angle) {

            foreach (Vertex vertex in this.v)
                vertex.RotateXGlobal(angle);
        }

        public void RotateYGlobal(float angle) {

            foreach (Vertex vertex in this.v)
                vertex.RotateYGlobal(angle);
        }

        public void RotateZGlobal(float angle) {

            foreach (Vertex vertex in this.v)
                vertex.RotateZGlobal(angle);
        }

        private void AverageZ() {

            float total = 0;

            foreach (Vertex vertex in this.v)
                total += vertex.z;

            this.z = total / this.v.Length;
        }

        private void MinMaxZ() {

            float min_z, max_z;

            min_z = this.v[0].z;

            foreach (Vertex vertex in this.v)
                if (vertex.z < min_z) min_z = vertex.z;

            max_z = this.v[0].z;

            foreach (Vertex vertex in this.v)
                if (vertex.z > max_z) max_z = vertex.z;

            this.z = (min_z + max_z) / 2.0f;
        }

        public void UpdateZ() {

            float total = 0;
            float cog_x = 0, cog_y = 0, cog_z = 0;

            foreach (Vertex vertex in this.v) {

                cog_x += vertex.x;
                cog_y += vertex.y;
                cog_z += vertex.z;
            }

            cog_x = cog_x / this.v.Length;
            cog_y = cog_y / this.v.Length;
            cog_z = cog_z / this.v.Length;

            this.z = (float)System.Math.Sqrt(cog_x * cog_x + cog_y * cog_y + cog_z * cog_z);

            //if (v.Length % 2 == 1)
            //this.MinMaxZ();
            //else
            //    this.AverageZ();
        }
    }
}
