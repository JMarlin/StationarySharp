using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationarySharp {

    class TextureCoordinate {

        public byte u = 0;
        public byte v = 0;

        public TextureCoordinate Clone() {

            TextureCoordinate return_coord = new TextureCoordinate();

            return_coord.u = this.u;
            return_coord.v = this.v;

            return return_coord;
        }
    }
}
