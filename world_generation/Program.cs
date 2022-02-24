using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace world_generation
{
    class Program
    {
        static void Main(string[] args)
        {
            test_place_objects();
            return;
            test_world_generation();
            return;
            test_arrow_placement();
            return;
            test_reward_placement();
            return;
        }

        private static void test_world_generation() {
            var wg = new GameWorld(50, 50);
            wg.generate_world();
            wg._place_rewards_on_stages();
            for (int i = 0; i < wg.stages.Count; i++) {
                Console.ReadLine();
                Console.Clear();
                GameWorld.print_world_matrix_to_console(wg.stages[i]);
            }
            List<double> percolation_probs = new List<double>() {1.0, 0.9, 0.8, 0.7};
            wg = new GameWorld(percolation_probs);
            wg.generate_world();
            wg._place_rewards_on_stages();
            GameWorld.print_world_matrix_to_console(wg.stages.Last());
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
        private static void test_place_objects(Int32 seed=31) {
            var _min_distance_between_objects = 3;
            var rand_gen = new Random(seed);
            for (int round = 0; round < 10; round++) {
                var wg = new GameWorld();
                wg.generate_world(rand_gen.Next());
                foreach (var stage in wg.stages) {
                    var placed_objects = GameWorldUtils.find_space_for_n_objects(stage, 5, min_distance_between_objects: _min_distance_between_objects);
                    for (int first_index = 0; first_index < placed_objects.Count; first_index ++) {
                        var first_object = placed_objects[first_index];
                        Assert.True(stage[first_object.x, first_object.y] == GameWorld.CELL_MARK_PASSABLE);
                        for (int second_index = first_index + 1; second_index < placed_objects.Count; second_index++) {
                            Assert.True(first_object.distance_to_other(placed_objects[second_index]) >= _min_distance_between_objects);
                        }
                    }
                }
            }
            Console.WriteLine("Sucessfully placed objects!");
        }
    }
}
