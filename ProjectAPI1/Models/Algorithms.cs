using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace ProjectAPI1.Classes
{
    public class AlgorithmFactory
    {
        public Algorithm GetAlgorithm(Profile profile)
        {
            if (profile == null)
            {
                throw new Exception("Profile is null");
            }
            switch (profile.Algorithm)
            {
                case "Unordered":
                    return new GeneralUnorderedAlgorithm(profile);
                case "Ordered":
                    return new GeneralOrderedAlgorithm(profile);
                default:
                    throw new Exception("Algorithm not found");
            }
        }
    }

    public abstract class Algorithm
    {
        protected Profile Profile;
        protected List<Box> Boxes;
        protected Dimensions ContainerDimensions;
        protected string? ExtraPreferences;
        protected int ProblemId;
        public static int Attempts;
        public static int BoxFitted;
        public static bool isInterrupted;
        public float finalScore;

        protected Classes.Solution Solution;


        public Algorithm(Profile profile)
        {
            finalScore = float.MinValue;
            Boxes = null;
            ContainerDimensions = null;
            ExtraPreferences = null;
            Solution = null;
            Profile = profile;
            Attempts = 0;
            BoxFitted = 0;
            isInterrupted = false;
        }

        protected bool CheckVolume(Classes.Problem problem)
        {
            Dimensions containerDimensions = problem.ContainerDimensions;
            List<Box> boxes = problem.Boxes;
            float containerVolume = containerDimensions.Length * containerDimensions.Width * containerDimensions.Height;
            float boxesVolume = 0;
            foreach (Box box in boxes)
            {
                boxesVolume += box.Dimensions.Length * box.Dimensions.Width * box.Dimensions.Height;
            }
            return boxesVolume <= containerVolume;
        }

        protected bool CheckUniform(List<Box> Boxes)
        {
            if (Boxes.Count == 0)
            {
                return false;
            }
            Dimensions firstBoxDimensions = Boxes[0].Dimensions;
            foreach (Box box in Boxes)
            {
                if (box.Dimensions.Height != firstBoxDimensions.Height || box.Dimensions.Width != firstBoxDimensions.Width || box.Dimensions.Length != firstBoxDimensions.Length)
                {
                    return false;
                }
                // check for no restrictions
                if(box.Restrictions != null)
                {
                    return false;
                }
            }
           


            return true;
        }

        protected bool Fits(List<BoxPlacement> boxPlacements, BoxPlacement boxPlacement)
        {
            foreach (BoxPlacement placedBox in boxPlacements)
            {
                float x1Min = placedBox.Position.x;
                float x1Max = placedBox.Position.x + placedBox.Box.Dimensions.Width;
                float y1Min = placedBox.Position.y;
                float y1Max = placedBox.Position.y + placedBox.Box.Dimensions.Height;
                float z1Min = placedBox.Position.z;
                float z1Max = placedBox.Position.z + placedBox.Box.Dimensions.Length;

                float x2Min = boxPlacement.Position.x;
                float x2Max = boxPlacement.Position.x + boxPlacement.Box.Dimensions.Width;
                float y2Min = boxPlacement.Position.y;
                float y2Max = boxPlacement.Position.y + boxPlacement.Box.Dimensions.Height;
                float z2Min = boxPlacement.Position.z;
                float z2Max = boxPlacement.Position.z + boxPlacement.Box.Dimensions.Length;

                // if intersecting the container return false
                if (x2Min < 0 || x2Max > ContainerDimensions.Width || y2Min < 0 || y2Max > ContainerDimensions.Height || z2Min < 0 || z2Max > ContainerDimensions.Length)
                {
                    return false;
                }
                // if not intersecting then coninue 
                if (x1Max <= x2Min || x2Max <= x1Min || y1Max <= y2Min || y2Max <= y1Min || z1Max <= z2Min || z2Max <= z1Min)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        protected Box GetRandomOrientation(Box box, float heat)
        {   if (box.Restrictions != null)
            {
                Box newBox2 = new Box(box.Description, box.Dimensions, box.Restrictions, box.OrderInList, box.Preferences);
                return newBox2;
            }
            Box newBox = new Box(box.Description, box.Dimensions, box.Restrictions, box.OrderInList, box.Preferences);
            // get number between 0 and 1
            System.Random randomGenerator = new System.Random();
            float random = (float)randomGenerator.NextDouble();
            // if random is more than heat then return the box
            if (random >= heat)
            {
                return newBox;
            }
            // get random orientation
            int orientation = randomGenerator.Next(0, 5);
            switch (orientation)
            {
                case 0:
                    newBox.Dimensions = box.Dimensions;
                    break;
                case 1:
                    newBox.Dimensions = new Dimensions(box.Dimensions.Length, box.Dimensions.Width, box.Dimensions.Height);
                    break;
                case 2:
                    newBox.Dimensions = new Dimensions(box.Dimensions.Height, box.Dimensions.Length, box.Dimensions.Width);
                    break;
                case 3:
                    newBox.Dimensions = new Dimensions(box.Dimensions.Width, box.Dimensions.Height, box.Dimensions.Length);
                    break;
                case 4:
                    newBox.Dimensions = new Dimensions(box.Dimensions.Height, box.Dimensions.Width, box.Dimensions.Length);
                    break;
                case 5:
                    newBox.Dimensions = new Dimensions(box.Dimensions.Length, box.Dimensions.Height, box.Dimensions.Width);
                    break;
            }
            return newBox;
        }

        protected List<Box> SwapRandomBoxes(List<Box> boxes, float heat)
        {
            System.Random randomGenerator = new System.Random();
            int numberToSwap = (int)(boxes.Count * heat);
            for (int i = 0; i < numberToSwap; i++)
            {
                int index1 = randomGenerator.Next(0, boxes.Count);
                int index2 = randomGenerator.Next(0, boxes.Count);
                Box temp = boxes[index1];
                boxes[index1] = boxes[index2];
                boxes[index2] = temp;
            }
            return boxes;
        }
        protected static float NextFloat(float mean, float stdDev)
        {
            System.Random random = new System.Random();
            double u1 = 1.0 - random.NextDouble(); // uniform random variable
            double u2 = 1.0 - random.NextDouble(); // uniform random variable
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2); // random normal variable
            float randNormal = (float)(mean + stdDev * randStdNormal);
            return randNormal;
        }

        protected static float GetNormalHeat()
        {
            float heat = NextFloat(0, 0.1f);
            if (heat < 0)
            {
                heat = heat * -1;
            }
            return heat;
        }

        protected List<List<Box>> GetRandomBoxOrientations(List<Box> currentOrientation, int numToGenerate)
        {
            List<List<Box>> boxOrientations = new List<List<Box>>();
            // always have the default one
            boxOrientations.Add(Boxes);

            // add random ones
            for (int i = 0; i < numToGenerate; i++)
            {
                List<Box> newOrientation = new List<Box>();
                foreach (Box box in currentOrientation)
                {
                    newOrientation.Add(GetRandomOrientation(box, GetNormalHeat()));
                }
                newOrientation = SwapRandomBoxes(newOrientation, GetNormalHeat());
                boxOrientations.Add(newOrientation);
            }

            // add a completely random one
            List<Box> boxes = new List<Box>();
            foreach (Box box in currentOrientation)
            {
                boxes.Add(GetRandomOrientation(box, 1f));
            }
            boxes = SwapRandomBoxes(boxes, 1f);
            boxOrientations.Add(boxes);

            return boxOrientations;
        }

        protected BoxPlacement? getFirstAvailableBoxPlacement(List<BoxPlacement> boxPlacements, Box box)
        {
            // first check if it is restricted to a specific positions
            if (box.Restrictions != null)
            {
                string[] split =box.Restrictions.Split(':');
                Position position2 = new Position();
                position2.x = float.Parse(split[0]);
                position2.y = float.Parse(split[1]);
                position2.z = float.Parse(split[2]);
                BoxPlacement boxPlacement = new BoxPlacement();
                boxPlacement.Box = box;
                boxPlacement.Position = position2;
                if (Fits(boxPlacements, boxPlacement))
                {
                    return boxPlacement;
                } else
                {
                    return null;
                }
            }
            Position position = new Position();
            position.x = 0;
            position.y = 0;
            position.z = 0;
            List<float> allPossibleX = new List<float>();
            List<float> allPossibleY = new List<float>();
            List<float> allPossibleZ = new List<float>();
            foreach (BoxPlacement boxPlacement in boxPlacements)
            {
                allPossibleX.Add(boxPlacement.Position.x + boxPlacement.Box.Dimensions.Width);
                allPossibleY.Add(boxPlacement.Position.y + boxPlacement.Box.Dimensions.Height);
                allPossibleZ.Add(boxPlacement.Position.z + boxPlacement.Box.Dimensions.Length);
            }
            // add 0,0,0
            allPossibleX.Add(0);
            allPossibleY.Add(0);
            allPossibleZ.Add(0);

            // remove duplicates
            allPossibleX = allPossibleX.Distinct().ToList();
            allPossibleY = allPossibleY.Distinct().ToList();
            allPossibleZ = allPossibleZ.Distinct().ToList();
            // sort so that the smallest values are first
            allPossibleX.Sort();
            allPossibleY.Sort();
            allPossibleZ.Sort();


            // find the first available position
            foreach (float x in allPossibleX)
            {
                foreach (float y in allPossibleY)
                {
                    foreach (float z in allPossibleZ)
                    {
                        position = new Position();
                        position.x = x;
                        position.y = y;
                        position.z = z;
                        BoxPlacement boxPlacement = new BoxPlacement();
                        boxPlacement.Box = box;
                        boxPlacement.Position = position;
                        if (Fits(boxPlacements, boxPlacement))
                        {
                            return boxPlacement;
                        }
                    }
                }
            }
            return null;
        }

        public abstract Classes.Solution Solve(Classes.Problem problem);

        protected List<Dimensions> GetRotations(Dimensions dimensions)
        {
            List<Dimensions> rotations = new List<Dimensions>();
            rotations.Add(dimensions);
            rotations.Add(new Dimensions(dimensions.Height, dimensions.Width, dimensions.Length));
            rotations.Add(new Dimensions(dimensions.Length, dimensions.Height, dimensions.Width));
            rotations.Add(new Dimensions(dimensions.Width, dimensions.Length, dimensions.Height));
            rotations.Add(new Dimensions(dimensions.Length, dimensions.Width, dimensions.Height));
            rotations.Add(new Dimensions(dimensions.Height, dimensions.Length, dimensions.Width));
            // remove duplicates
            rotations = rotations.Distinct().ToList();
            return rotations;
        }

        public Classes.Solution GetSolution(Classes.Problem problem)
        {
            Attempts = 0;
            Boxes = problem.Boxes;
            ContainerDimensions = problem.ContainerDimensions;
            ExtraPreferences = problem.ExtraPreferences;
            if (!CheckVolume(problem))
            {
                throw new Exception("Container volume is too small");
            }
            if (Boxes == null || ContainerDimensions == null)
            {
                throw new Exception("Boxes or ContainerDimensions not set");
            }
            else
            {
                if (Solution == null)
                {
                    Solution = Solve(problem);
                }
                return Solution;
            }
        }
    }

    public class UniformAlgorithm : Algorithm
    {
        public UniformAlgorithm(Profile profile) : base(profile) { }


        public override Classes.Solution Solve(Classes.Problem Problem)
        {
            Boxes = Problem.Boxes;
            ContainerDimensions = Problem.ContainerDimensions;
            ExtraPreferences = Problem.ExtraPreferences;
            ProblemId = Problem.Id;
            if (!CheckUniform(Boxes))
            {
                throw new Exception("Boxes are not uniform");
            }
            // try fit boxes in container without rotation
            Dimensions firstBoxDimensions = Boxes[0].Dimensions;
            List<Dimensions> rotations = GetRotations(firstBoxDimensions);
            foreach (Dimensions rotation in rotations)
            {
                if (OniformFits(rotation))
                {
                    finalScore = float.MaxValue;
                    return ArrangeBoxes(rotation);
                }
            }
            finalScore = float.MinValue;
            return null;
        }

        private Classes.Solution ArrangeBoxes(Dimensions boxDimensions)
        {
            int x = Convert.ToInt32(ContainerDimensions.Width / boxDimensions.Width);
            int y = Convert.ToInt32(ContainerDimensions.Height / boxDimensions.Height);
            int z = Convert.ToInt32(ContainerDimensions.Length / boxDimensions.Length);
            int boxCount = Boxes.Count;
            List<BoxPlacement> boxPlacements = new List<BoxPlacement>();
            for (int i = 0; i < boxCount; i++)
            {
                int xIndex = i % x;
                int yIndex = (i / x) % y;
                int zIndex = i / (x * y);
                Position position = new Position();
                position.x = xIndex * boxDimensions.Width;
                position.y = yIndex * boxDimensions.Height;
                position.z = zIndex * boxDimensions.Length;
                BoxPlacement boxPlacement = new BoxPlacement();
                Box box = Boxes[i];
                box.Dimensions = new Dimensions(boxDimensions.Width, boxDimensions.Height, boxDimensions.Length);
                boxPlacement.Box = box;
                boxPlacement.Position = position;
                boxPlacements.Add(boxPlacement);
            }
            Classes.Solution solution = new Classes.Solution();
            solution.Placements = boxPlacements;
            solution.ProblemId = ProblemId;
            solution.ProfileId = Profile.Id;
            return solution;
        }
        private bool OniformFits(Dimensions boxDimensions)
        {
            int x = Convert.ToInt32(ContainerDimensions.Width / boxDimensions.Width);
            int y = Convert.ToInt32(ContainerDimensions.Height / boxDimensions.Height);
            int z = Convert.ToInt32(ContainerDimensions.Length / boxDimensions.Length);
            int maxBoxes = x * y * z;
            int boxCount = Boxes.Count;
            return maxBoxes >= boxCount;
        }
    }

    public class UniformOrderedAlgorithm : Algorithm
    {
        public UniformOrderedAlgorithm(Profile profile) : base(profile) { }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            Algorithm algorithm = new UniformAlgorithm(Profile);
            // order boxes based on ORderInList, where the biggest numbers will be first
            problem.Boxes.Sort((x, y) => y.OrderInList.CompareTo(x.OrderInList));
            Classes.Solution solution = algorithm.Solve(problem);
            finalScore = float.MaxValue;
            return solution;
        }
    }

    public class GreedyAlgorithm : Algorithm
    {
        private List<BoxPlacement> boxPlacements;
        private int mostBoxesPlaced = 0;
        private List<Box> bestOrientation;
        public GreedyAlgorithm(Profile profile) : base(profile)
        {
            boxPlacements = new List<BoxPlacement>();
        }

        private Classes.Solution SolveForOrientation(Classes.Problem problem, List<Box> boxes)
        {
            // our current position in the container
            int counter = 0;
            foreach (Box box in boxes)
            {
                BoxPlacement boxPlacement = getFirstAvailableBoxPlacement(boxPlacements, box);
                if (boxPlacement == null)
                {
                    boxPlacements = new List<BoxPlacement>();
                    if (counter >= mostBoxesPlaced)
                    {
                        mostBoxesPlaced = counter;
                        bestOrientation = boxes;
                        BoxFitted = counter;
                    }
                    finalScore = float.MinValue;
                    return null;
                }
                boxPlacements.Add(boxPlacement);
                counter++;
            }
            Classes.Solution solution = new Classes.Solution();
            solution.Placements = boxPlacements;
            solution.ProblemId = ProblemId;
            solution.ProfileId = Profile.Id;
            finalScore = float.MaxValue;
            return solution;
        }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            Boxes = problem.Boxes;
            ContainerDimensions = problem.ContainerDimensions;
            ExtraPreferences = problem.ExtraPreferences;
            ProblemId = problem.Id;

            bestOrientation = Boxes;
            while (true)
            {
                List<List<Box>> boxOrientations = GetRandomBoxOrientations(bestOrientation, 50);
                foreach (List<Box> boxOrientation in boxOrientations)
                {
                    if (isInterrupted)
                    {
                        return null;
                    }
                    Attempts++;
                    Classes.Solution solution = SolveForOrientation(problem, boxOrientation);
                    if (solution != null)
                    {
                        return solution;
                    }
                }
            }
            // if we get here, we have not found a solution and we have been interrupted
            return null;
        }
    }

    public class GreedyOrderedAlgorithm : Algorithm
    {
        public GreedyOrderedAlgorithm(Profile profile) : base(profile) { }

        private List<Dimensions> GetAllBoxDimensions(Classes.Solution solution)
        {
            List<Box> Boxes = solution.Placements.Select(x => x.Box).ToList();
            List<Dimensions> boxDimensions = new List<Dimensions>();
            foreach (Box box in Boxes)
            {
                boxDimensions.Add(box.Dimensions);
            }
            // remove duplicates
            boxDimensions = boxDimensions.Distinct().ToList();
            return boxDimensions;
        }

        // gets all the boxes that were placed in the solution and have the same dimension 
        private List<BoxPlacement> GetAllPlacementsForDimension(Dimensions dimensions, Classes.Solution sol)
        {
            List<BoxPlacement> placements = new List<BoxPlacement>();
            foreach (BoxPlacement boxPlacement in sol.Placements)
            {
                if (boxPlacement.Box.Dimensions.Height == dimensions.Height &&
                    boxPlacement.Box.Dimensions.Width == dimensions.Width &&
                    boxPlacement.Box.Dimensions.Length == dimensions.Length &&
                    boxPlacement.Box.Restrictions == null)
                {
                    placements.Add(boxPlacement);
                }
            }
            return placements;
        }
        private List<Dimensions> GetAllDimentionRotations(Dimensions dimension)
        {
            List<Dimensions> dimensions = new List<Dimensions>();
            dimensions.Add(dimension);
            dimensions.Add(new Dimensions(dimension.Height, dimension.Length, dimension.Width));
            dimensions.Add(new Dimensions(dimension.Length, dimension.Width, dimension.Height));
            dimensions.Add(new Dimensions(dimension.Length, dimension.Height, dimension.Width));
            dimensions.Add(new Dimensions(dimension.Height, dimension.Width, dimension.Length));
            dimensions.Add(new Dimensions(dimension.Width, dimension.Length, dimension.Height));
            dimensions = dimensions.Distinct().ToList();
            return dimensions;
        }

        private void SortSimilar(Dimensions dimensions, Classes.Solution solution)
        {
            // get all placements for this dimension and sort them where first is the highest z value

            List<Dimensions> allRotatedDims = GetAllDimentionRotations(dimensions);


            List<BoxPlacement> placements = new List<BoxPlacement>();
            foreach (Dimensions dim in allRotatedDims)
            {
                placements.AddRange(GetAllPlacementsForDimension(dim, solution));
            }

            placements.Sort((x, y) => y.Position.z.CompareTo(x.Position.z));

            // get all the boxes that were placed in the solution and have the same dimension and sort them by order in list - first is lowest
            List<Box> boxes = new List<Box>();
            foreach (BoxPlacement placement in placements)
            {
                Box newBox = new Box(placement.Box.Description, placement.Box.Dimensions, placement.Box.Restrictions, placement.Box.OrderInList, placement.Box.Preferences);
                boxes.Add(newBox);
            }
            boxes.Sort((x, y) => x.OrderInList.CompareTo(y.OrderInList));
            //Debug.Log("boxes count: " + boxes.Count + " OF dimension: " + dimensions.Height + " " + dimensions.Width + " " + dimensions.Length);

            // update placements
            for (int i = 0; i < placements.Count; i++)
            {
                Box temp = placements[i].Box;
                placements[i].Box = new Box(boxes[i].Description, temp.Dimensions, boxes[i].Restrictions, boxes[i].OrderInList, boxes[i].Preferences);
            }

        }

        private List<Classes.Solution> GetRotatedSolutions(Classes.Solution sol, Dimensions containerDimensions)
        {
            List<Classes.Solution> solutions = new List<Classes.Solution>();
            // add with no rotations
            solutions.Add(sol);
            // if one of the boxes has restrictions -> return
            List<BoxPlacement> placements = sol.Placements;



        // add with 180 rotation -> all items in the back are now in front
            Classes.Solution rotated180 = new Classes.Solution();
            rotated180.ProblemId = sol.ProblemId;
            rotated180.ProfileId = sol.ProfileId;
            rotated180.Placements = new List<BoxPlacement>();
            foreach (BoxPlacement placement in sol.Placements)
            {
                // return with no rotation if restrictions apply
                if (placement.Box.Restrictions != null)
                {
                    return solutions;
                }
                BoxPlacement rotatedPlacement = new BoxPlacement();
                rotatedPlacement.Box = placement.Box;
                rotatedPlacement.Position = new Position();
                rotatedPlacement.Position.x = placement.Position.x;
                rotatedPlacement.Position.y = placement.Position.y;
                rotatedPlacement.Position.z = containerDimensions.Length - placement.Position.z - placement.Box.Dimensions.Length;
                rotated180.Placements.Add(rotatedPlacement);
            }

            solutions.Add(rotated180);

            return solutions;
        }

        private float CalcScore(Classes.Solution sol)
        {
            float score = 0;
            List<BoxPlacement> placements = sol.Placements;

            int max_order = placements.Max(x => x.Box.OrderInList);
            float mean_z = placements.Average(x => x.Position.z);

            // reward for placing boxes in order
            foreach (BoxPlacement placement in placements)
            {
                score += (placement.Position.z - mean_z) * (max_order - placement.Box.OrderInList);
            }
            score = score / placements.Count;
            return score;
        }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            Algorithm algorithm = new GreedyAlgorithm(Profile);
            Classes.Solution solution = algorithm.Solve(problem);
            if (solution == null)
            {
                return null;
            }
            Dimensions containerDimensions = problem.ContainerDimensions;
            List<Classes.Solution> solutions = GetRotatedSolutions(solution, containerDimensions);

            float best_score = float.MinValue;
            Classes.Solution best_solution = null;

            foreach (Classes.Solution sol in solutions)
            {
                if (isInterrupted)
                {
                    return null;
                }
                List<Dimensions> boxDimensions = GetAllBoxDimensions(sol);
                foreach (Dimensions dimensions in boxDimensions)
                {
                    SortSimilar(dimensions, sol);
                }
                float score = CalcScore(sol);
                if (score < best_score)
                {
                    best_score = score;
                    best_solution = sol;
                }
            }
            finalScore = best_score;
            return solution;
        }

    }

    public class GeneralUnorderedAlgorithm : Algorithm
    {
        public GeneralUnorderedAlgorithm(Profile profile) : base(profile) { }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            if (CheckUniform(problem.Boxes))
            {
                Attempts += 1;
                Algorithm alg1 = new UniformAlgorithm(Profile);
                Classes.Solution sol1 = alg1.Solve(problem);
                if (sol1 != null)
                {
                    finalScore = float.MaxValue;
                    return sol1;
                }
            }

            Algorithm alg2 = new GreedyAlgorithm(Profile);
            Classes.Solution sol2 = alg2.Solve(problem);
            if (sol2 == null)
            {
                //Debug.Log("No solution found");
            }
            finalScore = float.MaxValue;
            return sol2;
        }
    }

    public class GeneralOrderedAlgorithm : Algorithm
    {
        public GeneralOrderedAlgorithm(Profile profile) : base(profile) { }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            if (CheckUniform(problem.Boxes))
            {
                Attempts += 1;
                Algorithm alg1 = new UniformOrderedAlgorithm(Profile);
                Classes.Solution sol1 = alg1.Solve(problem);
                if (sol1 != null)
                {
                    finalScore = alg1.finalScore;
                    return sol1;
                }
            }

            Algorithm alg2 = new GreedyOrderedAlgorithm(Profile);
            Classes.Solution sol2 = alg2.Solve(problem);
            if (sol2 == null)
            {
                //Debug.Log("No solution found");
            }
            finalScore = alg2.finalScore;
            return sol2;
        }
    }
}
