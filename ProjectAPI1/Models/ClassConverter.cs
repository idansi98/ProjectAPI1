using ProjectAPI1.Classes;
using ProjectAPI1.Models;

namespace ProjectAPI1.Models
{
    public class ClassConvert
    {
        public static Classes.Problem ConvertProblem(Models.Problem problem)
        {
            Classes.Problem problem1 = new Classes.Problem();
            problem1.Id = problem.Id;
            problem1.Name = problem.Name;
            problem1.Boxes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Box>>(problem.Boxes);
            problem1.ContainerDimensions = Newtonsoft.Json.JsonConvert.DeserializeObject<Dimensions>(problem.ContainerDimensions);
            problem1.ExtraPreferences = problem.ExtraPreferences;
            return problem1;
        }
        public static Models.Problem ConvertProblem(Classes.Problem problem)
        {
            Models.Problem problem1 = new Models.Problem();
            problem1.Id = problem.Id;
            problem1.Name = problem.Name;
            problem1.Boxes = Newtonsoft.Json.JsonConvert.SerializeObject(problem.Boxes);
            problem1.ContainerDimensions = Newtonsoft.Json.JsonConvert.SerializeObject(problem.ContainerDimensions);
            problem1.ExtraPreferences = problem.ExtraPreferences;
            return problem1;
        }

        public static Classes.Solution ConvertSolution(Models.Solution solution)
        {
            Classes.Solution solution1 = new Classes.Solution();
            solution1.Id = solution.Id;
            solution1.ProblemId = solution.ProblemId;
            solution1.ProfileId = solution.ProfileId;
            solution1.Placements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BoxPlacement>>(solution.Placements);
            return solution1;
        }
        
        public static Models.Solution ConvertSolution(Classes.Solution solution)
        {
            Models.Solution solution1 = new Models.Solution();
            solution1.Id = solution.Id;
            solution1.ProblemId = solution.ProblemId;
            solution1.ProfileId = solution.ProfileId;
            solution1.Placements = Newtonsoft.Json.JsonConvert.SerializeObject(solution.Placements);
            solution1.Time = DateTime.Now.ToString();
            return solution1;
        }

        public static Classes.Profile ConvertProfile(Models.Profile profile)
        {
            Classes.Profile profile1 = new Classes.Profile();
            profile1.Id = profile.Id;
            profile1.Name = profile.Name;
            profile1.Algorithm = profile.Algorithm;
            profile1.ExtraSettings = profile.ExtraSettings;
            profile1.IsOutdated = profile.IsOutdated;
            profile1.IsDefault = profile.IsDefault;
            return profile1;
        }

        public static Models.Profile ConvertProfile(Classes.Profile profile, String userID)
        {
            Models.Profile profile1 = new Models.Profile();
            profile1.Id = profile.Id;
            profile1.Name = profile.Name;
            profile1.Algorithm = profile.Algorithm;
            profile1.ExtraSettings = profile.ExtraSettings;
            profile1.IsOutdated = profile.IsOutdated;
            profile1.IsDefault = profile.IsDefault;
            profile1.UserId = userID;
            return profile1;
        }

        public static List<Classes.Problem> ConvertProblems(List<Models.Problem> problems)
        {
            List<Classes.Problem> problems1 = new List<Classes.Problem>();
            foreach (Models.Problem problem in problems)
            {
                problems1.Add(ConvertProblem(problem));
            }
            return problems1;
        }

        public static List<Models.Problem> ConvertProblems(List<Classes.Problem> problems)
        {
            List<Models.Problem> problems1 = new List<Models.Problem>();
            foreach (Classes.Problem problem in problems)
            {
                problems1.Add(ConvertProblem(problem));
            }
            return problems1;
        }

        public static List<Classes.Solution> ConvertSolutions(List<Models.Solution> solutions)
        {
            List<Classes.Solution> solutions1 = new List<Classes.Solution>();
            foreach (Models.Solution solution in solutions)
            {
                solutions1.Add(ConvertSolution(solution));
            }
            return solutions1;
        }

        public static List<Models.Solution> ConvertSolutions(List<Classes.Solution> solutions)
        {
            List<Models.Solution> solutions1 = new List<Models.Solution>();
            foreach (Classes.Solution solution in solutions)
            {
                solutions1.Add(ConvertSolution(solution));
            }
            return solutions1;
        }

        public static List<Classes.Profile> ConvertProfiles(List<Models.Profile> profiles)
        {
            List<Classes.Profile> profiles1 = new List<Classes.Profile>();
            foreach (Models.Profile profile in profiles)
            {
                profiles1.Add(ConvertProfile(profile));
            }
            return profiles1;
        }

        public static List<Models.Profile> ConvertProfiles(List<Classes.Profile> profiles, string userID)
        {
            List<Models.Profile> profiles1 = new List<Models.Profile>();
            foreach (Classes.Profile profile in profiles)
            {
                profiles1.Add(ConvertProfile(profile, userID));
            }
            return profiles1;
        }
    }

}
