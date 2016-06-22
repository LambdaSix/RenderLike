using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLike.Example.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameInfo = new GameInfo()
            {
                Font = "celtic_garamond_10x10_gs_tc.png",
                FontLayout = FontLayout.TCOD,
                FontType = FontType.GreyscaleAA,
                Height = 24,
                Width = 80
            };
            var game = new ExampleGame(gameInfo);

            game.Start();
        }
    }
}
