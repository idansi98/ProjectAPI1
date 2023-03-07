using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Linq;
using MoreLinq;


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
        protected List<Box>? Boxes;
        protected Dimensions? ContainerDimensions;
        protected string? ExtraPreferences;
        protected int ProblemId;
        public static int BoxFitted;
        public static bool isInterrupted;
        public float finalScore;

        protected Classes.Solution? Solution;


        public Algorithm(Profile profile)
        {
            Boxes = null;
            ContainerDimensions = null;
            ExtraPreferences = null;
            Solution = null;
            Profile = profile;
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

        protected bool HasRestrictions(List<Box> Boxes)
        {
            foreach (Box box in Boxes)
            {
                if (box.Restrictions != null)
                {
                    return false;
                }
            }
            return true;
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
            }
            if (HasRestrictions(Boxes))
            {
                return false;
            } else
            {
                return true;
            }
        }

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

        public abstract Classes.Solution Solve(Classes.Problem problem);

        public Classes.Solution GetSolution(Classes.Problem problem)
        {
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

    public class GeneticAlgorithm : Algorithm
    {
        private bool isOrdered;
        private int maxTime;
        private float orderLambda;
        private int maxRoundsWithoutImprovement;
        private int genesPerGeneration;
        private List<Dimensions> allBoxDimensions;
        public GeneticAlgorithm(Profile profile) : base(profile)
        {
            isOrdered = true;
            maxTime = 30;
            orderLambda = 0.2f;
            maxRoundsWithoutImprovement = 30;
            genesPerGeneration = 50;
            allBoxDimensions = new();
        }
        public void SetIsOrdered(bool isOrdered)
        {
            this.isOrdered = isOrdered;
        }

        public void SetMaxTime(int maxTime)
        {
            this.maxTime = maxTime;
        }

        public void SetOrderLambda(float orderLambda)
        {
            this.orderLambda = orderLambda;
        }

        public void SetMaxRoundsWithoutImprovement(int maxRoundsWithoutImprovement)
        {
            this.maxRoundsWithoutImprovement= maxRoundsWithoutImprovement;
        }

        public void SetGenesPerGeneration(int genesPerGeneration)
        {
            this.genesPerGeneration = genesPerGeneration;
        }

        protected List<List<Box>> Mutate(List<Box> gene, int populationSize)
        {
            List<List<Box>> genes = new();
            List<Box> newGene;

            // add random ones
            for (int i = 0; i < populationSize; i++)
            {
                newGene = new List<Box>();
                foreach (Box box in gene)
                {
                    newGene.Add(GetRandomOrientation(box, GetRandomMutationStrength()));
                }
                newGene = SwapRandomBoxes(newGene, GetRandomMutationStrength());
                genes.Add(newGene);
            }

            // add a completely random one
            newGene = new List<Box>();
            foreach (Box box in newGene)
            {
                newGene.Add(GetRandomOrientation(box, 1f));
            }
            newGene = SwapRandomBoxes(newGene, 1f);
            genes.Add(newGene);

            return genes;
        }
        private List<Dimensions> GetAllBoxDimensions(List<Box> boxes)
        {
            List<Dimensions> boxDimensions = new List<Dimensions>();
            foreach (Box box in boxes)
            {
                List<float> dims = new();
                dims.Add(box.Dimensions.Width);
                dims.Add(box.Dimensions.Height);
                dims.Add(box.Dimensions.Width);
                // sort
                dims = dims.OrderBy(x => x).ToList();
                Dimensions dim = new Dimensions(dims[0], dims[1], dims[2]);
                boxDimensions.Add(dim);
            }
            // remove duplicates
            boxDimensions = boxDimensions.Distinct().ToList();
            return boxDimensions;
        }

        // gets all the boxes that were placed in the solution and have the same dimension 
        private List<BoxPlacement> GetAllPlacementsForDimension(Dimensions dimensions, List<BoxPlacement> placedBoxes)
        {
            List<BoxPlacement> placements = new List<BoxPlacement>();
            foreach (BoxPlacement boxPlacement in placedBoxes)
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
            List<Dimensions> dimensions = new()
            {
                dimension,
                new Dimensions(dimension.Height, dimension.Length, dimension.Width),
                new Dimensions(dimension.Length, dimension.Width, dimension.Height),
                new Dimensions(dimension.Length, dimension.Height, dimension.Width),
                new Dimensions(dimension.Height, dimension.Width, dimension.Length),
                new Dimensions(dimension.Width, dimension.Length, dimension.Height)
            };
            dimensions = dimensions.Distinct().ToList();
            return dimensions;
        }

        private void SortSimilar(Dimensions dimensions, List<BoxPlacement> placedBoxes)
        {
            // Get all placement of same box size

            List<Dimensions> allRotatedDims = GetAllDimentionRotations(dimensions);
            List<BoxPlacement> placements = new ();
            foreach (Dimensions dim in allRotatedDims)
            {
                placements.AddRange(GetAllPlacementsForDimension(dim, placedBoxes));
            }

            // sort them by Z index

            placements.Sort((x, y) => y.Position.z.CompareTo(x.Position.z));

            // get all the boxes that were placed in the solution and have the same dimension and sort them by order in list - first is lowest
            List<Box> boxes = new ();
            foreach (BoxPlacement placement in placements)
            {
                Box newBox = new(placement.Box.Description, placement.Box.Dimensions, placement.Box.Restrictions, placement.Box.OrderInList, placement.Box.Preferences);
                boxes.Add(newBox);
            }
            boxes.Sort((x, y) => x.OrderInList.CompareTo(y.OrderInList));

            // update placements
            for (int i = 0; i < placements.Count; i++)
            {
                Box temp = placements[i].Box;
                placements[i].Box = new Box(boxes[i].Description, temp.Dimensions, boxes[i].Restrictions, boxes[i].OrderInList, boxes[i].Preferences);
            }
        }

        private BoxPlacement? getFirstAvailableBoxPlacement(List<BoxPlacement> boxPlacements, Box box)
        {
            // first check if it is restricted to a specific positions
            if (box.Restrictions != null)
            {
                string[] split = box.Restrictions.Split(':');
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
                }
                else
                {
                    return null;
                }
            }
            // for unrestricted boxes
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

        private List<BoxPlacement> getPlacementFromGene(List<Box> gene)
        {
            List<BoxPlacement> placedBoxes = new();
            foreach (Box box in gene)
            {
                BoxPlacement? boxPlacement = getFirstAvailableBoxPlacement(placedBoxes, box);
                if (boxPlacement == null)
                    break;
                placedBoxes.Add(boxPlacement);
            }
            foreach (Dimensions dimension in allBoxDimensions)
            {
                SortSimilar(dimension, placedBoxes);
            }
            return placedBoxes;
        }

        private Solution getSolutionFromGene(List<Box> gene)
        {
            List<BoxPlacement> placement = getPlacementFromGene(gene);


            Classes.Solution solution = new Classes.Solution();
            solution.Placements = placement;
            solution.ProblemId = ProblemId;
            solution.ProfileId = Profile.Id;
            return solution;
        }

        protected Box GetRandomOrientation(Box box, float heat)
        {
            if (box.Restrictions != null)
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

        protected static float GetRandomMutationStrength()
        {
            System.Random random = new();
            double u1 = 1.0 - random.NextDouble(); // uniform random variable
            double u2 = 1.0 - random.NextDouble(); // uniform random variable
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2); // random normal variable
            return Math.Abs((float)(0.1 * randStdNormal));
        }

        private float Fitness(List<Box> gene)
        {
            float score = 0;
            List<BoxPlacement> placedBoxes = getPlacementFromGene(gene);
            HeuristicSwapping(placedBoxes);

            // give a score of 1 for each plcaed box
            score += placedBoxes.Count;

            if (isOrdered)
            {
                // order placedBoxes based on OrderInList
                placedBoxes = placedBoxes.OrderBy(x => x.Box.OrderInList).ToList();

                int boxCount = placedBoxes.Count;

                float median_z;
                List<BoxPlacement> placedBoxesOrderedByZ = placedBoxes.OrderBy(x => x.Position.z).ToList();
                if (placedBoxes.Count % 2 == 0)
                {
                    median_z = (placedBoxesOrderedByZ[(placedBoxes.Count / 2) - 1].Position.z
                        + placedBoxesOrderedByZ[placedBoxes.Count / 2].Position.z) / 2;
                }
                else
                {
                    median_z = placedBoxes[(placedBoxes.Count - 1) / 2].Position.z;
                }
                float biggest_z = placedBoxes[placedBoxes.Count - 1].Position.z;
                float smallest_z = placedBoxes[0].Position.z;



                float orderScore = 0;
                for (int i = 0; i < boxCount; i++)
                {
                    orderScore += (placedBoxes[i].Position.z - median_z) * (boxCount - i);
                }

                // normalize order score
                if (biggest_z != smallest_z)
                {
                    orderScore = orderScore / (biggest_z - smallest_z);
                }
                orderScore = (orderLambda * orderScore) / (median_z * boxCount);

                score += orderScore;
            }
            return score;
        }

        private void HeuristicSwapping(List<BoxPlacement> gene)
        {
            foreach (Dimensions dim in allBoxDimensions)
            {
                SortSimilar(dim, gene);
            }
        }

        public override Solution Solve(Problem problem)
        {
            long finishingTime = DateTime.Now.Ticks + DateTime.Now.AddMinutes(maxTime).Ticks;


            List<Box> bestGene = problem.Boxes.ToList();
            this.allBoxDimensions = GetAllBoxDimensions(bestGene);
            float bestScore = Fitness(bestGene);

            // random restarts
            while (DateTime.Now.Ticks < finishingTime)
            {
                int roundsWithoutImprovement = 0;
                List<Box> tempBestGene = problem.Boxes.ToList();
                float tempBestScore = Fitness(tempBestGene);

                // This is a few generation cycles until we hit time limit or see no improvement
                while (DateTime.Now.Ticks < finishingTime ||
                    roundsWithoutImprovement >= maxRoundsWithoutImprovement ||
                    (isOrdered == false && bestScore == problem.Boxes.Count))
                {
                    List<List<Box>> mutated = Mutate(tempBestGene, genesPerGeneration);
                    foreach (List<Box> gene in mutated)
                    {
                        float score = Fitness(gene);
                        if (score >= tempBestScore)
                        {
                            tempBestScore = score;
                            tempBestGene = gene;
                            roundsWithoutImprovement = -1;
                        }
                    }
                    roundsWithoutImprovement++;
                }
                if(tempBestScore > bestScore)
                {
                    bestScore = tempBestScore;
                    bestGene = tempBestGene;
                }
            }
            return getSolutionFromGene(bestGene);
        }
    }

    public class GeneralUnorderedAlgorithm : Algorithm
    {
        public GeneralUnorderedAlgorithm(Profile profile) : base(profile) { }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            if (CheckUniform(problem.Boxes))
            {
                Algorithm alg1 = new UniformAlgorithm(Profile);
                Classes.Solution sol1 = alg1.GetSolution(problem);
                if (sol1 != null)
                {
                    finalScore = float.MaxValue;
                    return sol1;
                }
            }

            GeneticAlgorithm alg2 = new GeneticAlgorithm(Profile);
            alg2.SetMaxTime(10);
            alg2.SetIsOrdered(false);
            alg2.SetMaxRoundsWithoutImprovement(8);
            alg2.SetGenesPerGeneration(50);
            return alg2.GetSolution(problem);
        }
    }

    public class GeneralOrderedAlgorithm : Algorithm
    {
        public GeneralOrderedAlgorithm(Profile profile) : base(profile) { }

        public override Classes.Solution Solve(Classes.Problem problem)
        {
            if (CheckUniform(problem.Boxes))
            {
                Algorithm alg1 = new UniformOrderedAlgorithm(Profile);
                Classes.Solution sol1 = alg1.GetSolution(problem);
                if (sol1 != null)
                {
                    finalScore = alg1.finalScore;
                    return sol1;
                }
            }

            GeneticAlgorithm alg2 = new GeneticAlgorithm(Profile);
            alg2.SetMaxTime(10);
            alg2.SetIsOrdered(true);
            alg2.SetMaxRoundsWithoutImprovement(8);
            alg2.SetGenesPerGeneration(50);
            return alg2.GetSolution(problem);
        }
    }
}
