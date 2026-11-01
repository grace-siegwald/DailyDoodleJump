using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;


namespace DailyDoodle.Core;

internal class Platform
{
    public Vector2 Position { get; set; }

    public int Width { get; set; } = 80;
    public int Height { get; set; } = 20;

    public Platform(Vector2 position)
    {
        Position = position;
    }
    /*
     platforms can:
    bounce, break move

     
     
     */

    public void GeneratePlatform()
    {
        //use random to generate vector positions for the platforms
        //use random to pick which platforms
        //they shoudl ahve set width and height

    }

    public void Bounce()
    {
        // use x and y to move the player move up when they hit this platofrm - this should coordinate with a player function
    }

    public void Break()
    {
        //break ad make the platforms move down then disaapear or just disappear
    }

    public void DisappearPlatform()
    {
        //use when platform breaks?
    }
    public void Move()
    {
        //use x and y coordinates (hopefully vectors) to move the moveable platofrms around

        // Implement bounce logic here
    }

}
