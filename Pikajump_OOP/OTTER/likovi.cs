using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
     public abstract class likovi : Sprite
    {
        protected int kretnja;
        public int Kretnja
        {
            get
            {
                return kretnja;
            }
            set
            {
                kretnja = value;
            }
        }

        public likovi (string s, int x, int y) : base(s, x, y)
        {
            this.kretnja = 5;
        }
        
        
    }

    public class Pokelopta : Sprite
    {
        protected int brzina;
        public int Brzina
        {
            get
            {
                return brzina;
            }
            set
            {
                brzina = value;
            }
        }
        public Pokelopta (string s, int x, int y) : base(s,x,y)
        {
            this.Brzina = 5;
        }
    }
    class Lik : likovi
    {
        public override int X
        {
            get { return x; }
            set
            {
                if (value <= GameOptions.LeftEdge)
                    this.x = GameOptions.LeftEdge;
                else if (value >= (GameOptions.RightEdge - this.Width))
                    this.x = GameOptions.RightEdge - this.Width;
                else
                    this.x = value;
            }
        }

        public Lik(string s, int xcor, int ycor) : base(s,xcor,ycor)
        {
            this.kretnja = 5;
        }
    }

    public class Prepreka : Sprite
    {
        private string boja;
        public string Boja
        {
            get
            {
                return boja;
            }
            set
            {
                boja = value;
            }
        }
        public Prepreka(string s, int x, int y, string b) : base(s, x, y)
        {
            this.Boja = b;
        }
    }

    

}
