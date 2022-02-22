﻿using System;
using Xunit;
using System.Linq;

namespace world_generation
{
    class Program
    {
        static void Main(string[] args)
        {
            test_arrow_placement();
            return;
            test_reward_placement();
            return;
            var wg = new GameWorld(50, 50);
            wg.generate_world();
            wg._place_rewards_on_stages();
            for (int i = 0; i < wg.stages.Count; i++) {
                Console.ReadLine();
                Console.Clear();
                GameWorld.print_world_matrix_to_console(wg.stages[i]);
            }
        }

        private static void test_reward_placement(Int32 seed=10, int nr_tests=200) {
            Random rand_gen = new Random(seed);
            Random tmp_rand = new Random(seed + 1);
            for (int test=0; test < nr_tests; test++) {
                Int32 world_seed = rand_gen.Next();
                var wg = new GameWorld();
                wg.generate_world(world_seed);
                foreach (var stage in wg.stages) {
                    String s_rewards = "";
                    String s_random = "";
                    foreach (var reward in wg.rewards) {
                        Assert.True(stage[reward.x, reward.y] == GameWorld.CELL_MARK_PASSABLE);
                        s_rewards += stage[reward.x, reward.y].ToString();
                        s_random +=stage[tmp_rand.Next() % wg.width, tmp_rand.Next() % wg.height];
                    }
                    Console.WriteLine(s_rewards);
                    Console.WriteLine(s_random);
                    foreach (var x in wg.rewards) {
                        Console.Write("-");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine(String.Format("Test {0}/{1}, random seed {2}. Success!", test + 1, nr_tests, world_seed));
            }
        }

        private static void test_arrow_placement(Int32 seed=10) {
            var rand_gen = new Random(seed);
            var wg = new GameWorld();
            wg.generate_world();
            wg._generate_rewards(4, rand_gen);

            IntCoordinates player_position = new IntCoordinates(wg.width / 2, wg.height / 2);
            var arrow = GameWorldUtils.generate_arrow(wg.stages.Last(), player_position, wg.rewards);
            Console.WriteLine($"Arrow at ({arrow.origin.x}, {arrow.origin.y}) in the direction ({arrow.direction.x}, {arrow.direction.y})");
            Console.WriteLine($"Player position ({player_position.x}, {player_position.y})");
            Console.WriteLine("Rewards:");
            foreach (var r in wg.rewards) {
                Console.WriteLine($"({r.x}, {r.y})");
            }
        }
    }
}
