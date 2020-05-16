using System;

public class Util
{
    public static void PrintMap(GameState state, int cX, int cY, int endX, int endY)
    {
        Console.Clear();
        for (int y =0; y < state.MapHeight; y++)
        {
            for (int x = 0; x < state.MapWidth; x++)
            {
                if (x == cX && y == cY)
                {
                    Console.Write("O");
                }
                else if (x == endX && y == endY)
                {
                    Console.Write("X");
                }
                else
                {
                    // Console.Write(state.Map[x, y] == MapItem.Empty ? " " : "#");
                }
                
            }
            Console.WriteLine();
        }
    }
}