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
    class Ennemi : Personnage
    {
        public bool EstMort;
        public Clock TimerPrincipale;
        public readonly int NOMBREBONBON = 1;
        public Direction DirectionDeplacement;

        //Attaque Mana imprévu
        public int DistanceDattaque;

        public Ennemi(Texture texture, IntRect AspectInitial, Vector2f TaillePerso, Vector2f positionInitial, Niveau level, int speed = 2, int nbBonbon = 3, int viemax = 3,int distanceAttaque = 200) : base(texture, AspectInitial, TaillePerso, positionInitial, level, speed, viemax)
        {
            NOMBREBONBON = nbBonbon;
            TimerPrincipale = new Clock();
            DirectionDeplacement = Direction.Right;
            DistanceDattaque = distanceAttaque;
            Mana.CercleDeFeu.FillColor = new Color(157, 20, 0);
            Mana.CercleDeFeu.OutlineColor = new Color(227, 100, 6);
            Mana.Vitesse = 3;
            EstMort = false;
            Mana.Cyble = BouleDeFeu.TypeDeCyble.CHeros;
            NombreDeVie = NOMBREVIEMAX;
            Stamina = new BarreDeVie(NombreDeVie, NOMBREVIEMAX);

        }
        public override void Draw(RenderTarget target, RenderStates state)
        {

            if(TimerPrincipale.ElapsedTime.AsSeconds() >= 0.1)
            {
                Move();
                Graviter();
                Mort();
                if (MettreAJourAnimation)
                {
                    AnimationDeplacement.X++;
                    if (AnimationDeplacement.X * TaillePerso.X >= Image.Size.X)
                        AnimationDeplacement.X = 0;
                }
                
                TimerPrincipale.Restart();
            }

            //Intelligence pour éviter de tomber
            if (ChuteLibre(PositionAbsolue + VecteurDeplacement))
                ChangerDirection();
            else if (DeplacerPersonnage() == false)
                ChangerDirection();

            AspectPhysique.TextureRect = new IntRect(AnimationDeplacement.X * (int)TaillePerso.X, AnimationDeplacement.Y * (int)TaillePerso.Y, (int)TaillePerso.X, (int)TaillePerso.Y);

            base.Draw(target, state);
        }
        public void DeposerDesBonbons()
        {
            int i;
            RessourceObjet[] tmp = new RessourceObjet[level.Ressources.Length + NOMBREBONBON];
            for (i = 0; i < level.Ressources.Length; i++)
                tmp[i] = level.Ressources[i];

            for (int j = 0; j < NOMBREBONBON; j++)
            {
                tmp[j + i] = new BonbonGARGOU(new Sprite(level.SpriteBonbon));
                tmp[j + i].PositionAbsolue = PositionAbsolue + new Vector2f(j*tmp[j + i].GetBoiteEnglobante().Width + 10,0);
            }

            level.Ressources = tmp;
            Console.WriteLine("Nombre de ressources  : "  + level.Ressources.Length);
            for (int k = 0; k < level.Ressources.Length; k++)
                if (level.Ressources[k] != null) Console.WriteLine("Ok " + level.Ressources[k].PositionAbsolue.X + " " + level.Ressources[k].PositionAbsolue.Y);
        }

        public void Move()
        {
            switch(DirectionDeplacement)
            {
                case Direction.Left:
                    VecteurDeplacement.X = -VitesseDeplacement;
                    VecteurDeplacement.Y = 0;
                    AnimationDeplacement.Y = (int)Direction.Left;
                    MettreAJourAnimation = true;
                    break;
                case Direction.Right:
                    VecteurDeplacement.X = VitesseDeplacement;
                    VecteurDeplacement.Y = 0;
                    AnimationDeplacement.Y = (int)Direction.Right;
                    MettreAJourAnimation = true;
                    break;
            }

        }
        public void ChangerDirection()
        {
            switch(DirectionDeplacement)
            {
                case Direction.Left:
                    DirectionDeplacement = Direction.Right;
                    break;
                case Direction.Right:
                    DirectionDeplacement = Direction.Left;
                    break;
            }
        }
        public void UtiliserMana(Personnage hero)
        {
            if((hero.PositionAbsolue.Y + hero.TaillePerso.Y >= PositionAbsolue.Y && hero.PositionAbsolue.Y <= PositionAbsolue.Y ) ||
              (hero.PositionAbsolue.Y <= PositionAbsolue.Y + TaillePerso.Y && hero.PositionAbsolue.Y >= PositionAbsolue.Y))
            {
                if (Mana.SeDeplace == false && DirectionDeplacement == Direction.Left && (hero.PositionAbsolue.X + hero.TaillePerso.X >= PositionAbsolue.X - DistanceDattaque && hero.PositionAbsolue.X < PositionAbsolue.X))
                {
                    Mana.SeDeplace = true;
                    Mana.direction = Direction.Left;
                    Mana.MovePositionAbsolue(PositionAbsolue - Mana.PositionAbsolue, level);
                    Mana.TimerDeplacement.Restart();

                }
                else if (Mana.SeDeplace == false && DirectionDeplacement == Direction.Right && (hero.PositionAbsolue.X < PositionAbsolue.X + DistanceDattaque && hero.PositionAbsolue.X > PositionAbsolue.X))
                {
                    Mana.SeDeplace = true;
                    Mana.direction = Direction.Right;
                    Mana.MovePositionAbsolue(PositionAbsolue - Mana.PositionAbsolue, level);                    
                    Mana.TimerDeplacement.Restart();

                }
            }
        }
        public void Mort()
        {
            if(NombreDeVie == 0)
            {
                DeposerDesBonbons();
                EstMort = true;
            }
        }
     }


}
