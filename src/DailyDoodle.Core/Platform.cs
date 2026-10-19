using System;
using System.Collections.Generic;
using System.Text;

namespace DailyDoodle.Core;

internal class Platform
{

    public int PlatformWidth;
    public int PlatformHeight;

    //coordinates - can we use vectors again?
    public int PlatformLocationX; 
    public int PlatformLocationY; 

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
