using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;







namespace OTTER
{
    
    /// <summary>
    /// -
    /// </summary>
    public partial class BGL : Form
    {
        /* ------------------- */
        #region Environment Variables

        List<Func<int>> GreenFlagScripts = new List<Func<int>>();

        /// <summary>
        /// Uvjet izvršavanja igre. Ako je <c>START == true</c> igra će se izvršavati.
        /// </summary>
        /// <example><c>START</c> se često koristi za beskonačnu petlju. Primjer metode/skripte:
        /// <code>
        /// private int MojaMetoda()
        /// {
        ///     while(START)
        ///     {
        ///       //ovdje ide kod
        ///     }
        ///     return 0;
        /// }</code>
        /// </example>
        public static bool START = true;

        //sprites
        /// <summary>
        /// Broj likova.
        /// </summary>
        public static int spriteCount = 0, soundCount = 0;

        /// <summary>
        /// Lista svih likova.
        /// </summary>
        //public static List<Sprite> allSprites = new List<Sprite>();
        public static SpriteList<Sprite> allSprites = new SpriteList<Sprite>();

        //sensing
        int mouseX, mouseY;
        Sensing sensing = new Sensing();

        //background
        List<string> backgroundImages = new List<string>();
        int backgroundImageIndex = 0;
        string ISPIS = "";
        string KONACNI = "";
        SoundPlayer[] sounds = new SoundPlayer[1000];
        TextReader[] readFiles = new StreamReader[1000];
        TextWriter[] writeFiles = new StreamWriter[1000];
        bool showSync = false;
        int loopcount;
        DateTime dt = new DateTime();
        String time;
        double lastTime, thisTime, diff;

        #endregion
        /* ------------------- */
        #region Events

        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {
                foreach (Sprite sprite in allSprites)
                {
                    if (sprite != null)
                        if (sprite.Show == true)
                        {
                            g.DrawImage(sprite.CurrentCostume, new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Heigth));
                        }
                    if (allSprites.Change)
                        break;
                }
                if (allSprites.Change)
                    allSprites.Change = false;
            }
            catch
            {
                //ako se doda sprite dok crta onda se mijenja allSprites
                MessageBox.Show("Greška!");
            }
        }

        private void startTimer(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Start();
            Init();
        }

        private void updateFrameRate(object sender, EventArgs e)
        {
            updateSyncRate();
        }

        /// <summary>
        /// Crta tekst po pozornici.
        /// </summary>
        /// <param name="sender">-</param>
        /// <param name="e">-</param>
        public void DrawTextOnScreen(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var brush = new SolidBrush(Color.WhiteSmoke);
            string text = ISPIS;
            string text2 = KONACNI;

            SizeF stringSize = new SizeF();
            Font stringFont = new Font("Arial", 14);
            stringSize = e.Graphics.MeasureString(text, stringFont);

            using (Font font1 = stringFont)
            {
                RectangleF rectF1 = new RectangleF(0, 0, stringSize.Width, stringSize.Height);
                RectangleF rectF2 = new RectangleF(350, 270, stringSize.Width, stringSize.Height);
                
                e.Graphics.FillRectangle(brush, Rectangle.Round(rectF1));
                
                e.Graphics.DrawString(text, font1, Brushes.Black, rectF1);
                
                
            }
        }

        private void mouseClicked(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = false;
            sensing.MouseDown = false;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            //sensing.MouseX = e.X;
            //sensing.MouseY = e.Y;
            //Sensing.Mouse.x = e.X;
            //Sensing.Mouse.y = e.Y;
            sensing.Mouse.X = e.X;
            sensing.Mouse.Y = e.Y;

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            sensing.Key = e.KeyCode.ToString();
            sensing.KeyPressedTest = true;
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            sensing.Key = "";
            sensing.KeyPressedTest = false;
        }

        private void Update(object sender, EventArgs e)
        {
            if (sensing.KeyPressed(Keys.Escape))
            {
                START = false;
            }

            if (START)
            {
                this.Refresh();
            }
        }

        #endregion
        /* ------------------- */
        #region Start of Game Methods

        //my
        #region my

        //private void StartScriptAndWait(Func<int> scriptName)
        //{
        //    Task t = Task.Factory.StartNew(scriptName);
        //    t.Wait();
        //}

        //private void StartScript(Func<int> scriptName)
        //{
        //    Task t;
        //    t = Task.Factory.StartNew(scriptName);
        //}

        private int AnimateBackground(int intervalMS)
        {
            while (START)
            {
                setBackgroundPicture(backgroundImages[backgroundImageIndex]);
                Game.WaitMS(intervalMS);
                backgroundImageIndex++;
                if (backgroundImageIndex == 3)
                    backgroundImageIndex = 0;
            }
            return 0;
        }

        private void KlikNaZastavicu()
        {
            foreach (Func<int> f in GreenFlagScripts)
            {
                Task.Factory.StartNew(f);
            }
        }

        #endregion

        /// <summary>
        /// BGL
        /// </summary>
        public BGL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pričekaj (pauza) u sekundama.
        /// </summary>
        /// <example>Pričekaj pola sekunde: <code>Wait(0.5);</code></example>
        /// <param name="sekunde">Realan broj.</param>
        public void Wait(double sekunde)
        {
            int ms = (int)(sekunde * 1000);
            Thread.Sleep(ms);
        }

        //private int SlucajanBroj(int min, int max)
        //{
        //    Random r = new Random();
        //    int br = r.Next(min, max + 1);
        //    return br;
        //}

        /// <summary>
        /// -
        /// </summary>
        public void Init()
        {
            if (dt == null) time = dt.TimeOfDay.ToString();
            loopcount++;
            //Load resources and level here
            this.Paint += new PaintEventHandler(DrawTextOnScreen);
            SetupGame();
        }

        /// <summary>
        /// -
        /// </summary>
        /// <param name="val">-</param>
        public void showSyncRate(bool val)
        {
            showSync = val;
            if (val == true) syncRate.Show();
            if (val == false) syncRate.Hide();
        }

        /// <summary>
        /// -
        /// </summary>
        public void updateSyncRate()
        {
            if (showSync == true)
            {
                thisTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                diff = thisTime - lastTime;
                lastTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                double fr = (1000 / diff) / 1000;

                int fr2 = Convert.ToInt32(fr);

                syncRate.Text = fr2.ToString();
            }

        }

        //stage
        #region Stage

        /// <summary>
        /// Postavi naslov pozornice.
        /// </summary>
        /// <param name="title">tekst koji će se ispisati na vrhu (naslovnoj traci).</param>
        public void SetStageTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Postavi boju pozadine.
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        public void setBackgroundColor(int r, int g, int b)
        {
            this.BackColor = Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Postavi boju pozornice. <c>Color</c> je ugrađeni tip.
        /// </summary>
        /// <param name="color"></param>
        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        /// <summary>
        /// Postavi sliku pozornice.
        /// </summary>
        /// <param name="backgroundImage">Naziv (putanja) slike.</param>
        public void setBackgroundPicture(string backgroundImage)
        {
            this.BackgroundImage = new Bitmap(backgroundImage);
        }

        /// <summary>
        /// Izgled slike.
        /// </summary>
        /// <param name="layout">none, tile, stretch, center, zoom</param>
        public void setPictureLayout(string layout)
        {
            if (layout.ToLower() == "none") this.BackgroundImageLayout = ImageLayout.None;
            if (layout.ToLower() == "tile") this.BackgroundImageLayout = ImageLayout.Tile;
            if (layout.ToLower() == "stretch") this.BackgroundImageLayout = ImageLayout.Stretch;
            if (layout.ToLower() == "center") this.BackgroundImageLayout = ImageLayout.Center;
            if (layout.ToLower() == "zoom") this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        #endregion

        //sound
        #region sound methods

        /// <summary>
        /// Učitaj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        /// <param name="file">-</param>
        public void loadSound(int soundNum, string file)
        {
            soundCount++;
            sounds[soundNum] = new SoundPlayer(file);
        }

        /// <summary>
        /// Sviraj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        public void playSound(int soundNum)
        {
            sounds[soundNum].Play();
        }

        /// <summary>
        /// loopSound
        /// </summary>
        /// <param name="soundNum">-</param>
        public void loopSound(int soundNum)
        {
            sounds[soundNum].PlayLooping();
        }

        /// <summary>
        /// Zaustavi zvuk.
        /// </summary>
        /// <param name="soundNum">broj</param>
        public void stopSound(int soundNum)
        {
            sounds[soundNum].Stop();
        }

        #endregion

        //file
        #region file methods

        /// <summary>
        /// Otvori datoteku za čitanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToRead(string fileName, int fileNum)
        {
            readFiles[fileNum] = new StreamReader(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToRead(int fileNum)
        {
            readFiles[fileNum].Close();
        }

        /// <summary>
        /// Otvori datoteku za pisanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToWrite(string fileName, int fileNum)
        {
            writeFiles[fileNum] = new StreamWriter(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToWrite(int fileNum)
        {
            writeFiles[fileNum].Close();
        }

        /// <summary>
        /// Zapiši liniju u datoteku.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <param name="line">linija</param>
        public void writeLine(int fileNum, string line)
        {
            writeFiles[fileNum].WriteLine(line);
        }

        /// <summary>
        /// Pročitaj liniju iz datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća pročitanu liniju</returns>
        public string readLine(int fileNum)
        {
            return readFiles[fileNum].ReadLine();
        }

        /// <summary>
        /// Čita sadržaj datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća sadržaj</returns>
        public string readFile(int fileNum)
        {
            return readFiles[fileNum].ReadToEnd();
        }

        #endregion

        //mouse & keys
        #region mouse methods

        /// <summary>
        /// Sakrij strelicu miša.
        /// </summary>
        public void hideMouse()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Pokaži strelicu miša.
        /// </summary>
        public void showMouse()
        {
            Cursor.Show();
        }

        /// <summary>
        /// Provjerava je li miš pritisnut.
        /// </summary>
        /// <returns>true/false</returns>
        public bool isMousePressed()
        {
            //return sensing.MouseDown;
            return sensing.MouseDown;
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">naziv tipke</param>
        /// <returns></returns>
        public bool isKeyPressed(string key)
        {
            if (sensing.Key == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">tipka</param>
        /// <returns>true/false</returns>
        public bool isKeyPressed(Keys key)
        {
            if (sensing.Key == key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
        /* ------------------- */

        /* ------------ GAME CODE START ------------ */

        /* Game variables */
        Lik Pika;
        Prepreka p1;
        Prepreka p2;
        Prepreka p3;
        Prepreka p4;
        Pokelopta pokeball;


        /* Initialization */
        



        private void SetupGame()
        {
            //1. setup stage
            SetStageTitle("PMF");
            setBackgroundColor(Color.WhiteSmoke);
            setBackgroundPicture("backgrounds\\pattern.jpg");
            //none, tile, stretch, center, zoom
            setPictureLayout("stretch");

            //2. add sprites
            Pika = new Lik("sprites\\pikapika.png", 50, GameOptions.DownEdge - 100);
           
            Pika.SetSize(35);
            Pika.Y = GameOptions.DownEdge - Pika.Heigth;
            // Random sluzi za X
            Random g = new Random();

            int r = g.Next(GameOptions.LeftEdge + 10, GameOptions.RightEdge - 50);
            p1 = new Prepreka("sprites\\prepreka.jpg", r, GameOptions.DownEdge - Pika.Heigth - 50, "plava");
            int sirina = p1.Width;
            

            r = g.Next(GameOptions.LeftEdge + sirina, GameOptions.RightEdge - sirina);
            p2 = new Prepreka("sprites\\prepreka.jpg", r, p1.Y - 140, "plava");
            r = g.Next(GameOptions.LeftEdge + sirina, GameOptions.RightEdge - sirina);
            p3 = new Prepreka("sprites\\prepreka.jpg", r, p2.Y - 140, "plava");
            r = g.Next(GameOptions.LeftEdge + sirina, GameOptions.RightEdge - sirina);
            p4 = new Prepreka("sprites\\prepreka.jpg", r, p3.Y - 140, "plava");
            
            int najdonji = Pika.Y;

            pokeball = new Pokelopta("sprites\\pokeball.png", 0, GameOptions.UpEdge);
            
            pokeball.SetSize(10);
            Game.AddSprite(pokeball);
            Game.AddSprite(Pika);
            pokeball.SetVisible(false);

            Game.AddSprite(p1);
            Game.AddSprite(p2);
            Game.AddSprite(p3);
            Game.AddSprite(p4);




            //3. scripts that start
            Game.StartScript(Jump);
            Game.StartScript(KretnjaPrepreke);
            Game.StartScript(FallingPokeball);
            Game.StartScript(Time);
            Game.StartScript(GameOver);
            

        }

        //možemo slati i parametre kod poziva događaja




        /* Event handlers - metode*/


        /* Scripts */
        private int KretnjaPrepreke()
        {
            bool right = true;
            bool right2 = false;
            while (START)
            {
                
                    if (p3.X + p3.Width > GameOptions.RightEdge)
                    {
                        right = false;
                    }
                    if (p3.X < GameOptions.LeftEdge)
                    {
                        right = true;
                    }
                    if (right)
                    {

                    p3.X += GameOptions.Speed;
                    
                    }
                    else
                    {
                        p3.X -= GameOptions.Speed;
                    
                    }

                if (p4.X + p4.Width > GameOptions.RightEdge)
                {
                    right2 = false;
                }
                if(p3.X<GameOptions.LeftEdge)
                    right2 = true;

                if (right2)
                {
                    p4.X += GameOptions.Speed;
                }
                else
                    p4.X -= GameOptions.Speed;


                Wait(0.01);
                    if((Pika.TouchingSprite(p3)&&Pika.Y<=p3.Y-p3.Heigth*2))
                {
                    if (right)
                        Pika.X += GameOptions.Speed;
                    else
                        Pika.X -= GameOptions.Speed;
                }

                


            }
            return 0;
        }


        
        int bodovi = 0;
        

        private void Pad()
        {
            int dno = GameOptions.DownEdge;
                int o = p2.Y;
                while(o<dno)
                {
                    p1.Y += 5;
                    if (p1.Y < dno)
                        p1.Y = p4.Y - 140;
                    p2.Y += 5;
                    Pika.Y += 5;
                    p3.Y += 5;
            
                    p4.Y += 5;
                if (p2.Y < dno)
                    p2.Y = p1.Y - 140;

                o += 5;
                    Wait(0.01);
                }
        }
        
        // Podesavanja granica za Spriteove izvan skripte kako bi bile dohvative
        //Pocetne su granice zadane u GameOptionu(okvir ekrana)
        int granicaL = GameOptions.LeftEdge;
        int granicaD = GameOptions.RightEdge;
        int granicaG = GameOptions.DownEdge;
        // x sluzi kasnije za podešavanje na kojoj se prepreci nalazimo te kako bi lakse razlikovali prepreke
        int x = 0;
        // za kretanje lijevo i desno
        Bitmap desniSkin = new Bitmap("sprites\\pikadesno.png");
        Bitmap lijeviSkin = new Bitmap("sprites\\pikapika.png");
        private int Jump()
        {
            
            
            
            while (START)
            {
                

                //Kretnje lijevo desno
                if (sensing.KeyPressed(Keys.Right))
                {
                    
                    Pika.CurrentCostume = desniSkin;
                    
                    Pika.X += Pika.Kretnja;
                    

                }
                
                else if (sensing.KeyPressed(Keys.Left))
                {
                    
                    Pika.CurrentCostume = lijeviSkin;
                    Pika.X -= Pika.Kretnja;
                    

                }
                
                //Ogranicenje za spriteove(prepreke)
                if ((Pika.X< granicaL || Pika.X > granicaD)&&(!Pika.TouchingSprite(p1)||!Pika.TouchingSprite(p2)||!Pika.TouchingSprite(p3))&&Pika.Y<granicaG)
                {
                    //Lik pada dok ne dotakne dno
                    //Ako je na p2, ako dotakne p1 ostaje na p1, ne pada skroz do dna
                    while (Pika.Y < GameOptions.DownEdge-1)
                    {
                        //Kretnje dok pada
                        if (sensing.KeyPressed(Keys.Right))
                        {
                            Pika.X += Pika.Kretnja;
                            Pika.CurrentCostume = desniSkin;

                        }
                        else if (sensing.KeyPressed(Keys.Left))
                        {
                            Pika.CurrentCostume = lijeviSkin;
                            Pika.X -= Pika.Kretnja;

                        }
                        
                        Pika.Y += 4;
                        // AKO U Padu dodirne prvu prepreku ostaje na njoj
                        // Gubi bod jer je tako podeseno u igrici kako bi se smanjio broj pokušaja skakanja na više prepreke
                        // (npr. samo pritišće space jer će kad tada skočiti na prepreku)
                        if(Pika.TouchingSprite(p1)&&Pika.Y-1<=p1.Y-p1.Heigth)
                        {
                            if(bodovi>0)
                                bodovi -= 1;
                            granicaL = p1.X - Pika.Width;
                            granicaD = p1.X + p1.Width - 10;
                            granicaG = p1.Y + p1.Heigth;
                            x = 1;
                            break;
                        }
                        // Ako dotakne drugu prepreku
                        else if(Pika.TouchingSprite(p2) && Pika.Y - 1 <= p2.Y - p2.Heigth)
                        {
                            if(bodovi>0)
                                bodovi -= 1;
                            granicaL = p2.X - Pika.Width;
                            granicaD = p2.X + p2.Width - 10;
                            x = 2;
                            break;

                        }
                        
                        
                        Wait(0.01);
                        
                    }
                
                    
                    
                    

                }
                

                // visina oznacava koliko visoko moze lik skociti, bool vrijednosti l i r sluze za pokretanje lika u zraku da izgleda bolje
                int visina = 0;
                bool l = false;
                bool r = false;
                // bool dod(iri) kako se ne bi mogao napraviti multijump
                bool dod = true;
                bool dod2 = true;
                bool dod3 = true;

                // skok
                if (sensing.KeyPressed(Keys.Space))
                {
                    // kad je lik na 1.prepreci x je jednak 1 itd.
                    if (Pika.TouchingSprite(p1))
                        x = 1;
                    else if (Pika.TouchingSprite(p2))
                        x = 2;
                    else
                        x = 0;

                    //visina(jump) raste dok ne dode do 160.. Fiksirano je na 160 zbog razlike izmedu Y koordinata dvaju prepreka
                        while (visina < 160)
                        {

                            if (sensing.KeyPressed(Keys.Right))
                            {
                                r = true;
                                l = false;
                            }

                            else if (sensing.KeyPressed(Keys.Left))
                            {
                                r = false;
                                l = true;
                            }

                        if (r)
                        {
                            Pika.X += Pika.Kretnja;
                            Pika.CurrentCostume = desniSkin;
                        }
                        else if (l)
                        {
                            Pika.X -= Pika.Kretnja;
                            Pika.CurrentCostume = lijeviSkin;
                        }
                            Pika.Y -= 4;
                            visina += 4;
                            Wait(0.01);
                        }

                    /*X OZNACAVA RAZINU 
                     * - 0-> POCETNA KOJA JE SE IZVRSAVA SAMO KOD PRVOG SKOKA
                     * - 1 -> PRVA PREPREKA ITD.
                     */
                    
                    if (x == 0)
                    {
                        l = false;
                        r = false;
                        
                        while (dod)
                        {
                            if (sensing.KeyPressed(Keys.Right))
                            {
                                r = true;
                                l = false;
                            }

                            else if (sensing.KeyPressed(Keys.Left))
                            {
                                r = false;
                                l = true;
                            }

                            if (r)
                            {
                                Pika.X += Pika.Kretnja;
                                Pika.CurrentCostume = desniSkin;
                            }
                            else if (l)
                            {
                                Pika.CurrentCostume = lijeviSkin;
                                Pika.X -= Pika.Kretnja;
                            }
                                
                            Pika.Y += 4;


                            if (Pika.TouchingSprite(p1) && Pika.Y - 1 < p1.Y - p1.Heigth)
                            {
                                bodovi += 1;
                                dod = false;
                                Pika.Y = p1.Y - p1.Heigth * 2;
                                granicaL = p1.X - Pika.Width;
                                granicaD = p1.X + p1.Width - 10;
                                granicaG = p1.Y - p1.Heigth;
                                x = 1;
                            }
                            if (Pika.Y + Pika.Heigth >= GameOptions.DownEdge)
                            {
                                x = 0;
                                dod = false;
                                Pika.Y = GameOptions.DownEdge-Pika.Heigth;
                                granicaL = GameOptions.LeftEdge;
                                granicaD = GameOptions.RightEdge;
                                granicaG = GameOptions.DownEdge;
                                break;
                            }
                            Wait(0.01);
                        }
                        Wait(0.01);
                    }
                    // x = 1, nalazi se na prvoj prepreci i skace na drugu. ako je dotakne x ide u 2
                    else if(x==1)
                    {
                        dod2 = true;
                        l = false;
                        r = false;
                        while (dod2)
                        {
                            if (sensing.KeyPressed(Keys.Right))
                            {
                                r = true;
                                l = false;
                            }

                            else if (sensing.KeyPressed(Keys.Left))
                            {
                                r = false;
                                l = true;
                            }

                            if (r)
                                Pika.X += Pika.Kretnja;
                            else if (l)
                                Pika.X -= Pika.Kretnja;
                            Pika.Y += 4;
                            

                            if (Pika.TouchingSprite(p2) && Pika.Y - 1 < p2.Y - p2.Heigth)
                            {
                                bodovi += 1;
                                
                                x = 2;
                                Pika.Y = p2.Y - p2.Heigth * 2;
                                granicaL = p2.X - Pika.Width + 5;
                                granicaD = p2.X + p2.Width - 10;
                                granicaG = p2.Y - p2.Heigth;
                                break;
                            }
                            else if (Pika.TouchingSprite(p1) && Pika.Y <= p1.Y - p1.Heigth)
                            {
                                x = 1;
                                if (bodovi > 0)
                                    bodovi -= 1;
                                Pika.Y = p1.Y - p1.Heigth * 2;
                                granicaL = p1.X - Pika.Width;
                                granicaD = p1.X + p1.Width - 10;
                                granicaG = p1.Y - p1.Heigth;
                                break;

                            }
                            Wait(0.01);
                        }
                        Wait(0.01);
                        
                    }
                    // X SE NALAZI NA DRUGOJ PREPRECI. AKO DOTAKNE TRECU X ODLAZI U 2
                    else if(x==2)
                    {
                        l = false;
                        r = false;
                        dod3 = true;
                        while (dod3)
                        {
                            if (sensing.KeyPressed(Keys.Right))
                            {
                                r = true;
                                l = false;
                            }

                            else if (sensing.KeyPressed(Keys.Left))
                            {
                                r = false;
                                l = true;
                            }

                            if (r)
                                Pika.X += Pika.Kretnja;
                            else if (l)
                                Pika.X -= Pika.Kretnja;
                            Pika.Y += 4;


                            if (Pika.TouchingSprite(p3) && Pika.Y - 1 < p3.Y - p3.Heigth)
                            {

                                bodovi += 1;
                                Random kl = new Random();
                                p4.X = kl.Next(GameOptions.LeftEdge + p4.Width, GameOptions.RightEdge - p4.Width);
                                Pad();
                                Wait(0.01);
                                granicaL = p3.X - Pika.Width + 5;
                                granicaD = p3.X + p3.Width - 10;
                                granicaG = p3.Y - p3.Heigth;
                                Prepreka zamj = p1;
                                p1 = p2;
                                p2 = p3;
                                p3 = p4;
                                p4 = zamj;
                                
                                Wait(0.01);

                                
                               
                                break;
                            }
                            else if(Pika.TouchingSprite(p2)&&Pika.Y<=p2.Y-p2.Heigth)
                            {
                                if(bodovi>0)
                                    bodovi -= 1;
                                x = 2;
                                Pika.Y = p2.Y - p2.Heigth * 2;
                                granicaL = p2.X - Pika.Width + 5;
                                granicaD = p2.X + p2.Width - 10;
                                granicaG = p2.Y - p2.Heigth;
                                break;
                            }
                            else if(Pika.TouchingSprite(p1)&&Pika.Y<=p1.Y-p1.Heigth)
                            {
                                if(bodovi>0)
                                    bodovi -= 1;
                                Pika.Y = p1.Y - p1.Heigth * 2;
                                granicaL = p1.X - Pika.Width;
                                granicaD = p1.X + p1.Width - 10;
                                granicaG = p1.Y - p1.Heigth;
                                x = 1;
                                break;
                            }
                            
                            Wait(0.01);
                        }
                        Wait(0.01);
                    }
                    


                }


                if (bodovi == 20)
                    Pika.Kretnja = 8;

                else if (bodovi == 20)
                    GameOptions.Speed = 10;
                else if (bodovi == 50)
                    GameOptions.Speed = 12;
                else if (bodovi == 100)
                    GameOptions.Speed = 15;
                
                

            }
            return 0;
        }
        
        private int FallingPokeball()
        {
            while(START)
            {
                if(bodovi%5==0 && bodovi>5)
                {
                    Random RAND = new Random();

                    pokeball.X = RAND.Next(GameOptions.LeftEdge + 5, GameOptions.RightEdge - 5);
                    pokeball.SetVisible(true);
                    while(pokeball.Y<GameOptions.DownEdge)
                    {
                        pokeball.Y += pokeball.Brzina;
                        //if(pokeball.TouchingSprite(Pika))
                        //{
                            
                        //    Pika.SetVisible(false);
                        //    p1.SetVisible(false);
                        //    p2.SetVisible(false);
                        //    p3.SetVisible(false);
                        //    p4.SetVisible(false);
                        //    pokeball.SetVisible(false);
                            



                        //    ISPIS = "GAME OVER!!!\nKonacni bodovi:" + bodovi;
                        //    Wait(0.01);
                        //    setBackgroundColor(0,0,128);
                        //    setBackgroundPicture("backgrounds\\caughtpikachu.png");
                        //    setPictureLayout("stretch");
                            
                        //    START = false;
                        //    break;
                            

                            
                        //}
                        Wait(0.01);
                    }
                    
                }
                if(bodovi==25)
                {
                    pokeball.Brzina = 8;
                }
                pokeball.Y = 0;
                pokeball.SetVisible(false);
            }
            return 0;
        }

        // sw inicijaliziramo kako bi ograničili korisnika 
        // ograničenje je stavljeno kako igra ne bi bila prelagana te kako bi natjeralo igrača da
        // uvijek radi nešto u igrici te da ne čeka stalno spriteove da se pomiču
        Stopwatch sw = new Stopwatch();
        private int Time()
        {
            while(START)
            {
                sw.Start();
                //Timer se pokrece uvijek, a prekida ukoliko korisnik ne pritisne neku od glavnih tipki(Arrowse Lijevo i desno te Space)
                if (sensing.KeyPressed(Keys.Left) || sensing.KeyPressed(Keys.Right) ||sensing.KeyPressed(Keys.Space))
                {
                    sw.Reset();
                }
                if(Pika.Y<GameOptions.DownEdge)
                    ISPIS = "Score:" + bodovi + "\nTime:" + (5-sw.ElapsedMilliseconds / 1000);
                if (sw.ElapsedMilliseconds>5001)
                {
                    sw.Stop();

                    // Nakon 5 sekundi bez dodira dolazi GameOver jer ce Y pasti ispod granica koje smo postavili za GameOver
                    // U svakom slučaju lik se ne može nikako spasiti
                    while(Pika.Y-Pika.Heigth<GameOptions.DownEdge)
                    {
                        Pika.Y += 5;
                        p1.Y += 5;
                        p2.Y += 5;
                        p3.Y += 5;
                        p4.Y += 5;
                        
                        Wait(0.01);
                    }
                }

            }
            return 0;
        }
        private void Pokusaj(Prepreka prva, Prepreka druga, Prepreka treca, Prepreka cetvrta)
        {
            Prepreka zamjenska;
            zamjenska = prva;
            prva = druga;
            druga = treca;
            treca = cetvrta;
            cetvrta = prva;
            
        }

        private int GameOver()
        {
            while(START)
            {
                if(Pika.Y>=GameOptions.DownEdge || Pika.TouchingSprite(pokeball))
                {
                    Wait(0.05);
                    ISPIS = "Bodovi konacni:" + bodovi+"\nUkoliko zelite opet igrati pritisnite \"Retry\"";
                    Wait(0.05);
                    p2.SetVisible(false);
                    p3.SetVisible(false);
                    p1.SetVisible(false);
                    p4.SetVisible(false);
                    pokeball.SetVisible(false);
                    Pika.SetVisible(false);
                    setBackgroundPicture("backgrounds\\fallenPikachu.jpg");
                    setPictureLayout("stretch");
                    Wait(0.01);

                    MessageBoxButtons buttons = MessageBoxButtons.RetryCancel;
                    DialogResult dr = MessageBox.Show("Wanna try again?", "Try", buttons);
                    
                    
                    if (dr == DialogResult.Retry)
                    {
                        Application.Restart();
                    }


                    else
                    {
                        START = false;
                        Application.Exit();
                    }

                    

                    Wait(2);





                }
                
                Wait(0.01);
            }
            return 0;
        }

        

        /* ------------ GAME CODE END ------------ */


    }
}
