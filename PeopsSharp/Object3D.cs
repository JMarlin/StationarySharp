using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationarySharp {

    //TODO: Modify this in the future to use proper transformation matrices and matrix concatenation
    //      This should also have more of a heirarchical scene-graph type structure to it
    class Object3D {

        public List<Polygon> Polygons = new List<Polygon>();
        public float x = 0;
        public float y = 0;
        public float z = 0;

        public void AddPolygon(Vertex v0, TextureCoordinate uv0, Vertex v1, TextureCoordinate uv1, Vertex v2, TextureCoordinate uv2, Color24 c) {

            this.AddPolygon(new Polygon(v0, uv0, v1, uv1, v2, uv2, c));
        }

        public void AddPolygon(Vertex v0, TextureCoordinate uv0, Vertex v1, TextureCoordinate uv1, Vertex v2, TextureCoordinate uv2, Vertex v3, TextureCoordinate uv3, Color24 c) {

            this.AddPolygon(new Polygon(v0, uv0, v1, uv1, v2, uv2, v3, uv3, c));
        }

        public void AddPolygon(Polygon polygon) {

            this.Polygons.Add(polygon);
        }

        public void AddPolygons(IEnumerable<Polygon> poly_collection) {

            this.Polygons.AddRange(poly_collection);
        }

        public Object3D CreateSubdivided() {

            Object3D return_object = new Object3D();

            return_object.x = this.x;
            return_object.y = this.y;
            return_object.z = this.z;

            foreach (Polygon poly in this.Polygons)
                return_object.AddPolygons(poly.CreateSubdivided());

            return return_object;
        }

        public void Translate(float x, float y, float z) {

            this.x += x;
            this.y += y;
            this.z += z;

            //Reset double-transform guard in vertexes
            foreach (Polygon poly in this.Polygons)
                foreach (Vertex v in poly.v)
                    v.has_been_transformed = false;

            foreach (Polygon polygon in this.Polygons) 
                polygon.Translate(x, y, z);
        }

        public void RotateXGlobal(float angle) {

            //Reset double-transform guard in vertexes
            foreach (Polygon poly in this.Polygons)
                foreach (Vertex v in poly.v)
                    v.has_been_transformed = false;

            foreach (Polygon polygon in this.Polygons)
                polygon.RotateXGlobal(angle);
        }

        public void RotateYGlobal(float angle) {

            //Reset double-transform guard in vertexes
            foreach (Polygon poly in this.Polygons)
                foreach (Vertex v in poly.v)
                    v.has_been_transformed = false;

            foreach (Polygon polygon in this.Polygons)
                polygon.RotateYGlobal(angle);
        }
        public void RotateZGlobal(float angle) {

            //Reset double-transform guard in vertexes
            foreach (Polygon poly in this.Polygons)
                foreach (Vertex v in poly.v)
                    v.has_been_transformed = false;

            foreach (Polygon polygon in this.Polygons)
                polygon.RotateZGlobal(angle);
        }

        public void RotateXLocal(float angle) {

            float oldx = this.x;
            float oldy = this.y;
            float oldz = this.z;

            this.Translate(-oldx, -oldy, -oldz);
            this.RotateXGlobal(angle);
            this.Translate(oldx, oldy, oldz);
        }

        public void RotateYLocal(float angle) {

            float oldx = this.x;
            float oldy = this.y;
            float oldz = this.z;

            this.Translate(-oldx, -oldy, -oldz);
            this.RotateYGlobal(angle);
            this.Translate(oldx, oldy, oldz);
        }

        public void RotateZLocal(float angle) {

            float oldx = this.x;
            float oldy = this.y;
            float oldz = this.z;

            this.Translate(-oldx, -oldy, -oldz);
            this.RotateZGlobal(angle);
            this.Translate(oldx, oldy, oldz);
        }
    }
}
