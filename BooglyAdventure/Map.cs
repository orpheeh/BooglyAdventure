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

    //Propriété des tuiles
    struct TileProp
    {
        public bool Plein;
    }

    // Affiche les éléments fixe du décor
    class Map : Transformable, Drawable
    {
        public int[,] TileMap { get; private set; }
        public TileProp[] ProprieteDesTuiles { get; private set; }
        public Vector2f TailleTuile;
        public View ScrollingSFML;
        public int TuileVide;

        public Map(TileProp[] Prop, int[,] map, Vector2u TailleFenetre, string Tileset = "null",int tuilevide = 8)
        {
            ProprieteDesTuiles = Prop;
            TileMap = map;
            TailleTuile = new Vector2f(32,32);
            this.TailleFenetre = TailleFenetre;
            this.Tileset = new Texture(Tileset);
            ScrollingSFML = new View(new FloatRect(32, 32, TailleFenetre.X, TailleFenetre.Y));
            TuileVide = tuilevide;

            ChargerDecor();
        }

        public void ChargerDecor()
        {
            Decor = new VertexArray(PrimitiveType.Quads);

            for(int i = 0; i < TileMap.GetLength(0); i++)
                for(int j = 0; j < TileMap.GetLength(1); j++)
                {
                    if (TileMap[i,j] != TuileVide)
                    {
                        int tu = TileMap[i, j] % Niveau.NombreDeTuileTileset.X;
                        int tv = TileMap[i, j] / Niveau.NombreDeTuileTileset.X;

                        Decor.Append(new Vertex(new Vector2f(i * TailleTuile.X, j * TailleTuile.Y), new Vector2f(tu * TailleTuile.X, tv * TailleTuile.Y)));
                        Decor.Append(new Vertex(new Vector2f((i + 1) * TailleTuile.X, j * TailleTuile.Y), new Vector2f((tu + 1) * TailleTuile.X, tv * TailleTuile.Y)));
                        Decor.Append(new Vertex(new Vector2f((i + 1) * TailleTuile.X, (j + 1) * TailleTuile.Y), new Vector2f((tu + 1) * TailleTuile.X, (tv + 1) * TailleTuile.Y)));
                        Decor.Append(new Vertex(new Vector2f(i * TailleTuile.X, (j + 1) * TailleTuile.Y), new Vector2f(tu * TailleTuile.X, (tv + 1) * TailleTuile.Y)));
                    }
                }
        }

        public void Draw(RenderTarget target, RenderStates state)
        {
            state.Transform *= Transform;
            state.Texture = Tileset;
            target.SetView(ScrollingSFML);
            target.Draw(Decor,state);
            target.SetView(target.DefaultView);
        }

        private VertexArray Decor;
        private Texture Tileset;
        private Vector2u TailleFenetre;
        
    }
}
