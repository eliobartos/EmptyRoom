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
            test_reward_placement();
            return;
            test_objects_placement();
            return;
            test_world_generation();
            return;
            test_arrow_placement();
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

        private static void test_reward_placement(Int32 seed=10, int nr_tests=200, int display_first_n=5) {
            Random rand_gen = new Random(seed);
            Random tmp_rand = new Random(seed + 1);
            var nr_displayed = 0;
            for (int test=0; test < nr_tests; test++) {
                Int32 world_seed = rand_gen.Next();
                var wg = new GameWorld();
                wg.generate_world(world_seed);
                foreach (var stage in wg.stages) {
                    foreach (var reward in wg.rewards) {
                        Assert.True(stage[reward.x, reward.y] == GameWorld.CELL_MARK_PASSABLE);
                    }
                }
                if (nr_displayed < display_first_n) {
                    GameWorld._print(wg.stages.Last(), wg.rewards);
                    nr_displayed += 1;
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

        private static Dictionary<String, double> place_objects_on_world_stage(int [, ] game_world, int nr_objects, int minimal_distance_between_objects, Int32 seed) {
            var result_dict = new Dictionary<String, double> ();

            var nr_passable_cells = GameWorldUtils.get_passable_cells_as_list(game_world).Count;
            var non_interacting_objects = GameWorldUtils.find_space_for_n_objects(
                game_world, nr_objects, min_distance_between_objects: minimal_distance_between_objects, seed: seed);
            Assert.True(non_interacting_objects.Count <= nr_passable_cells);

            int nr_non_interacting_well_places_pairs = 0;
            for (int first_index = 0; first_index < non_interacting_objects.Count; first_index ++) {
                var first_object = non_interacting_objects[first_index];
                Assert.True(game_world[first_object.x, first_object.y] == GameWorld.CELL_MARK_PASSABLE);

                for (int second_index = first_index + 1; second_index < non_interacting_objects.Count; second_index++) {
                    var distance = first_object.distance_to_other(non_interacting_objects[second_index]);
                    Assert.True(distance > 0.001);
                    if (distance >= minimal_distance_between_objects) {
                        nr_non_interacting_well_places_pairs += 1;
                    }
                }
            }
            result_dict.Add(
                "non_interacting_well_placed_fraction",
                 1.0 * nr_non_interacting_well_places_pairs / (non_interacting_objects.Count * (non_interacting_objects.Count - 1) / 2));
            result_dict.Add("non_interacting_not_placed_fraction", 1.0 * (nr_objects - non_interacting_objects.Count) / nr_objects);

            var interacting_objects = GameWorldUtils.find_space_for_n_objects(game_world, nr_objects, keep_world_connected: true, seed: seed);
            Assert.True(interacting_objects.Count <= nr_passable_cells);
            var original_cluster_size = GameWorldUtils.mark_cluster_and_return_size(
                GameWorldUtils.copy_as_matrix_of_occupied_or_passable(game_world),
                GameWorldUtils.find_one_with_mark(game_world, GameWorld.CELL_MARK_PASSABLE),
                GameWorld.CELL_MARK_UNUSED_AFTER_THIS + 221);
            var matrix = GameWorldUtils.copy_as_matrix_of_occupied_or_passable(game_world);
            foreach (var o in interacting_objects) {
                matrix[o.x, o.y] = GameWorld.CELL_MARK_OCCUPIED;
            }
            if (interacting_objects.Count < original_cluster_size) {
                var new_cluster_size = GameWorldUtils.mark_cluster_and_return_size(
                    matrix, GameWorldUtils.find_one_with_mark(matrix, GameWorld.CELL_MARK_PASSABLE), GameWorld.CELL_MARK_UNUSED_AFTER_THIS + 1124);
                Assert.True(original_cluster_size == (new_cluster_size + interacting_objects.Count));
            }

            for (var first_index = 0; first_index < interacting_objects.Count; first_index++) {
                var first_object = interacting_objects[first_index];
                for (var second_index = first_index + 1; second_index < interacting_objects.Count; second_index++) {
                    var second_object = interacting_objects[second_index];
                    Assert.True((first_object.x != second_object.x) || (first_object.y != second_object.y));
                }
            }

            result_dict.Add("interacting_not_placed_fraction", 1.0 * (nr_objects - interacting_objects.Count) / nr_objects);
            return result_dict;
        }
        private static void test_objects_placement(Int32 seed=31) {
            var min_distance_list = new List<int> () {5};
            var nr_objects_to_place_list = new List<int> () {50, 100, 500};
            var rand_gen = new Random(seed);
            int nr_rounds = 1;
            foreach (int nr_to_place in nr_objects_to_place_list) {
                foreach (int min_distance in min_distance_list) {
                    var collected_results = new Dictionary<String, List<double>> ();
                    for (int round = 0; round < nr_rounds; round++) {
                        var wg = new GameWorld();
                        wg.generate_world(rand_gen.Next());
                        foreach (var stage in wg.stages) {
                            var result_dict = place_objects_on_world_stage(stage, nr_to_place, min_distance, seed);

                            var _non_int_wpf = result_dict["non_interacting_well_placed_fraction"];
                            var _non_int_npf = result_dict["non_interacting_not_placed_fraction"];
                            var _int_npf = result_dict["interacting_not_placed_fraction"];
                            var message = $"Placing {nr_to_place} objects at min distance {min_distance}...\n";
                            message += $"Failed to place {_non_int_npf:N2} non-interacting objects. Of those placed, {_non_int_wpf:N2} are placed well.\n";
                            message += $"Failed to place {_int_npf:N2} interacting objects.";
                            if ((_non_int_npf > 0.2)  || (_non_int_wpf < 0.8) || (_int_npf > 0.3)) {
                                Console.WriteLine(message);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Sucessfully placed interacting and non interacting objects!");
        }
    }
}
