using System;

namespace world_generation
{
    class Program
    {
        static void Main(string[] args)
        {
            var wg = new GameWorld(50, 50);
            Int32 seed = (Int32) (new Random()).Next();
            wg.generate_world(seed);
            wg._place_rewards_on_stages();
            for (int i = 0; i < wg.stages.Count; i++) {
                Console.ReadLine();
                Console.Clear();
                GameWorld.print_world_matrix_to_console(wg.stages[i]);
            }
        }
    }
}
