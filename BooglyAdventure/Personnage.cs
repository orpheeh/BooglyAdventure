using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace BooglyAdventure
{
    class Personnage : Transformable, Drawable
    {
        public Sprite AspectPhysique;
        protected Texture Image;
        public Vector2f PositionAbsolue { get; set; }
        public Vector2f TaillePerso;
        public Vector2i AnimationDeplacement;
        public Vector2f VecteurDeplacement;
        public int VitesseDeplacement;
        public Niveau level;
        public bool MettreAJourAnimation;
        public int ConstanteGravitationnelle;
        
        //Pour l' animation du déplacement
        public enum Direction { Down, Left, Right, Up };

        //Attaque Mana
        public BouleDeFeu Mana;

        //Barre de vie du joueur
        public int NombreDeVie;
        public readonly int NOMBREVIEMAX = 5;
        public BarreDeVie Stamina;

        public Personnage(Texture texture, IntRect AspectInitial, Vector2f TaillePerso, Vector2f positionInitial, Niveau level,int speed = 2,int viemax = 5)
        {

            AspectPhysique = new Sprite(Image = texture, AspectInitial);
            AspectPhysique.Position = positionInitial;
            this.TaillePerso = TaillePerso;
            PositionAbsolue = positionInitial;
            this.level = level;
            VitesseDeplacement = speed;
            MettreAJourAnimation = false;
            

            //Mana Pour Attaquer
            Mana = new BouleDeFeu(BouleDeFeu.TypeDeCyble.CEnnemie, positionInitial, new Color(1, 4, 135), new Color(77, 215, 225), 3);

            NombreDeVie = 1;
            NOMBREVIEMAX = viemax;
            Stamina = new BarreDeVie(NombreDeVie, NOMBREVIEMAX);
        }
        public virtual void Draw(RenderTarget target, RenderStates state)
        {
            if (Mana.SeDeplace == false)
                Mana.CercleDeFeu.Position = AspectPhysique.Position;

            if (Mana.SeDeplace && Mana.TimerDeplacement.ElapsedTime.AsSeconds() >= 0.01)
            {
                if (Mana.direction == Direction.Left)
                    Mana.MovePositionAbsolue(new Vector2f(-Mana.Vitesse, 0),level);
                //Mana.CercleDeFeu.Position = new Vector2f(Mana.CercleDeFeu.Position.X - Mana.Vitesse, Mana.CercleDeFeu.Position.Y);
                else
                    Mana.MovePositionAbsolue(new Vector2f(Mana.Vitesse, 0), level);
                    //Mana.CercleDeFeu.Position = new Vector2f(Mana.CercleDeFeu.Position.X + Mana.Vitesse, Mana.CercleDeFeu.Position.Y);

                if (Mana.CercleDeFeu.Position.X < 0 ||
                    Mana.CercleDeFeu.Position.X + Mana.CercleDeFeu.Radius > level.TailleFenetre.X)
                    Mana.SeDeplace = false;

                Mana.TimerDeplacement.Restart();
            }
            Stamina.RectVie.Position = new Vector2f(AspectPhysique.Position.X + TaillePerso.X / 2 - Stamina.TailleBarre.X / 2, AspectPhysique.Position.Y - 5 - Stamina.TailleBarre.Y);
            target.Draw(AspectPhysique);
            if (Mana.SeDeplace)
                target.Draw(Mana.CercleDeFeu);
            target.Draw(Stamina);
        }
        public bool ChuteLibre()
        {
            Vector2f test = PositionAbsolue;
            test.Y += VitesseDeplacement;

            return !CollisionDecorFixe(test);
        }
        public bool ChuteLibre(Vector2f PositionAbsolue)
        {
            Vector2f test = PositionAbsolue;
            test.Y += VitesseDeplacement;

            return !CollisionDecorFixe(test);
        }
        public void Graviter()
        {
            if (ChuteLibre())
            {
                VecteurDeplacement = new Vector2f(0, VitesseDeplacement);
                DeplacerPersonnage();
                VecteurDeplacement.Y += ConstanteGravitationnelle;
            }
        }
        public bool CollisionDecorFixe(Vector2f position)
        {
            if (position.X + TaillePerso.X >= level.Monde.TileMap.GetLength(0) * level.Monde.TailleTuile.X ||
                position.Y + TaillePerso.Y >= level.Monde.TileMap.GetLength(1) * level.Monde.TailleTuile.Y ||
                position.X < 0 || position.Y < 0) 
            {
                if (position.Y + TaillePerso.Y >= level.Monde.TileMap.GetLength(1) * level.Monde.TailleTuile.Y)
                {
                    Toucher();
                    return false;
                }
                return true;
            }

            int PremiereCaseX = (int)(position.X / level.Monde.TailleTuile.X);
            int DerniereCaseX = (int)((position.X + TaillePerso.X) / level.Monde.TailleTuile.X);

            int PremiereCaseY = (int)(position.Y / level.Monde.TailleTuile.Y);
            int DerniereCaseY = (int)((position.Y + TaillePerso.Y) / level.Monde.TailleTuile.Y);

            for(int i = PremiereCaseX; i <= DerniereCaseX; i++)
                for(int j = PremiereCaseY; j <= DerniereCaseY; j++)
                {
                    if(level.Monde.ProprieteDesTuiles[level.Monde.TileMap[i,j]].Plein)
                    {
                        return true;
                    }
                }

            return false;
        }
        public virtual bool CollisionAutrePersonnage(Personnage Autrui)
        {
            return (AspectPhysique.GetGlobalBounds().Intersects(Autrui.AspectPhysique.GetGlobalBounds())) ;
        }
        public bool EssaiDeplacement()
        {
            Vector2f test = PositionAbsolue;
            test.X += VecteurDeplacement.X;
            test.Y += VecteurDeplacement.Y;

            if(CollisionDecorFixe(test) == false)
            {
                PositionAbsolue = test;
                AspectPhysique.Position = new Vector2f(PositionAbsolue.X - (level.Monde.ScrollingSFML.Center.X - level.Monde.ScrollingSFML.Size.X / 2), PositionAbsolue.Y - (level.Monde.ScrollingSFML.Center.Y - level.Monde.ScrollingSFML.Size.Y / 2));
                return true;
            }
            return false;
        }
        protected void Affine()
        {
            for (int i = 0; i < Math.Abs(VecteurDeplacement.X); i++)
            {
                VecteurDeplacement = new Vector2f((VecteurDeplacement.X == 0) ? 0 : (VecteurDeplacement.X > 0) ? 1 : -1, 0);
                if (EssaiDeplacement() == false)
                    break;
            }
            for (int i = 0; i < Math.Abs(VecteurDeplacement.Y); i++)
            {
                VecteurDeplacement = new Vector2f(0,(VecteurDeplacement.Y == 0) ? 0 : (VecteurDeplacement.Y > 0) ? 1 : -1);
                if (EssaiDeplacement() == false)
                    break;
            }
        }
        public bool DeplacerPersonnage()
        {
            if (EssaiDeplacement())
                return true;

            Affine();
            return false;
        }

        public static bool CollisionDecorFixe(Vector2f TaillePerso,Vector2f position, Niveau level)
        {
            if (position.X + TaillePerso.X >= level.Monde.TileMap.GetLength(0) * level.Monde.TailleTuile.X ||
                position.Y + TaillePerso.Y >= level.Monde.TileMap.GetLength(1) * level.Monde.TailleTuile.Y ||
                position.X < 0 || position.Y < 0)
                return true;

            int PremiereCaseX = (int)(position.X / level.Monde.TailleTuile.X);
            int DerniereCaseX = (int)((position.X + TaillePerso.X) / level.Monde.TailleTuile.X);

            int PremiereCaseY = (int)(position.Y / level.Monde.TailleTuile.Y);
            int DerniereCaseY = (int)((position.Y + TaillePerso.Y) / level.Monde.TailleTuile.Y);

            for (int i = PremiereCaseX; i <= DerniereCaseX; i++)
                for (int j = PremiereCaseY; j <= DerniereCaseY; j++)
                {
                    if (level.Monde.ProprieteDesTuiles[level.Monde.TileMap[i, j]].Plein)
                    {
                        return true;
                    }
                }

            return false;
        }
        public void Toucher()
        {
            NombreDeVie--;
            if (NombreDeVie < 0)
                NombreDeVie = 0;
            Stamina.MettreAjourBarre(NombreDeVie);
        }
        public void Revie()
        {
            NombreDeVie++;
            if (NombreDeVie > NOMBREVIEMAX)
                NombreDeVie = NOMBREVIEMAX;
            Stamina.MettreAjourBarre(NombreDeVie);
        }
    }


    struct BouleDeFeu
    {
        public CircleShape CercleDeFeu;
        public TypeDeCyble Cyble;
        public int Vitesse;
        public Clock TimerDeplacement;
        public bool SeDeplace;
        public Personnage.Direction direction;
        public Vector2f PositionAbsolue;

        public BouleDeFeu(TypeDeCyble type, Vector2f pos, Color Couleur, Color Bordure,int Speed)
        {
            CercleDeFeu = new CircleShape(5);
            CercleDeFeu.OutlineThickness = 2;
            CercleDeFeu.OutlineColor = Bordure;
            CercleDeFeu.FillColor = Couleur;
            Cyble = type;
            TimerDeplacement = new Clock();
            Vitesse = Speed;
            SeDeplace = false;
            direction = Personnage.Direction.Right;
            CercleDeFeu.Position = pos;
            PositionAbsolue = new Vector2f(0, 0);
        }

        public void MovePositionAbsolue(Vector2f dl,Niveau level)
        {
            PositionAbsolue += dl;
            CercleDeFeu.Position = new Vector2f(PositionAbsolue.X - (level.Monde.ScrollingSFML.Center.X - level.Monde.ScrollingSFML.Size.X / 2), PositionAbsolue.Y - (level.Monde.ScrollingSFML.Center.Y - level.Monde.ScrollingSFML.Size.Y / 2));
        }
        public enum TypeDeCyble { CHeros, CEnnemie };
    }

    class BarreDeVie : Transformable, Drawable
    {
        public int NombreDeVie, NombreDeVieMax;
        public RectangleShape RectVie, RectMort;
        public Vector2f TailleBarre;

        public BarreDeVie(int NombreDeVie, int NombreDeVieMax)
        {
            this.NombreDeVie = NombreDeVie;
            this.NombreDeVieMax = NombreDeVieMax;

            TailleBarre = new Vector2f(NombreDeVieMax * 5, 5);

            RectVie = new RectangleShape(new Vector2f(NombreDeVie * 5, 5));
            RectVie.FillColor = Color.Green;
            RectMort = new RectangleShape(new Vector2f((NombreDeVieMax - NombreDeVie) * 5, 5));
            RectMort.FillColor = Color.Red;

            RectMort.Position = new Vector2f(RectVie.Position.X + RectVie.Size.X, RectVie.Position.Y);
        }

        public void Draw(RenderTarget target, RenderStates state)
        {
            RectMort.Position = new Vector2f(RectVie.Position.X + RectVie.Size.X, RectVie.Position.Y);

            target.Draw(RectVie);
            if (NombreDeVie != NombreDeVieMax)
                target.Draw(RectMort);
        }

        public void MettreAjourBarre(int vie)
        {
            NombreDeVie = vie;
            RectVie = new RectangleShape(new Vector2f(vie * 5, 5));
            RectVie.FillColor = Color.Green;
            RectMort = new RectangleShape(new Vector2f((NombreDeVieMax - vie) * 5, 5));
            RectMort.FillColor = Color.Red;

        }
    }
}
