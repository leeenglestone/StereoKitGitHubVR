using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;

namespace StereoKitApp
{
    public class App
    {
        public SKSettings Settings => new SKSettings
        {
            appName = "GitHub Contribution VR Example",
            assetsFolder = "Assets",
            displayPreference = DisplayMode.MixedReality
        };

        TextStyle headingTextStyle;
        Vec2 textSize = new Vec2(0.7f, 0.7f);

        // Floor
        Matrix floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
        Material floorMaterial;

        // Materials
        Material level1Material;
        Material level2Material;
        Material level3Material;
        Material level4Material;
        Material level5Material;
        Material lowerBoxMaterial;

        // Models
        Model level1Model;
        Model level2Model;
        Model level3Model;
        Model level4Model;
        Model level5Model;
        Model lowerBoxModel;

        Vec3 lowerBoxDimensions;

        // Collections
        static List<Pose> cubePoses = new List<Pose>();
        static List<int> dayLevels = new List<int>();

        // Contribution information
        UserContributionCollection userContributionCollection;
        ContributionDay[] contributionDays;

        Pose lowerBoxPose;
        Pose labelPose;

        bool dataRetrievalComplete;

        public void Init()
        {
            // Init Materials
            level1Material = Material.Default.Copy();
            level2Material = Material.Default.Copy();
            level3Material = Material.Default.Copy();
            level4Material = Material.Default.Copy();
            level5Material = Material.Default.Copy();

            level1Material[MatParamName.ColorTint] = StereoKit.Color.HSV(215f / 360f, 2f / 100f, 1f);
            level2Material[MatParamName.ColorTint] = StereoKit.Color.HSV(130f / 360f, 33f / 100f, 0.91f);
            level3Material[MatParamName.ColorTint] = StereoKit.Color.HSV(135f / 360f, 67f / 100f, 0.76f);
            level4Material[MatParamName.ColorTint] = StereoKit.Color.HSV(135f / 360f, 70f / 100f, 0.63f);
            level5Material[MatParamName.ColorTint] = StereoKit.Color.HSV(138f / 360f, 70f / 100f, 0.5f);

            lowerBoxMaterial = Material.Default.Copy();
            lowerBoxMaterial[MatParamName.ColorTint] = StereoKit.Color.Hex(0x66666600);

            floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;

            // Models
            level1Model = Model.FromMesh(Mesh.GenerateRoundedCube(new Vec3(0.08f, 1 * 0.02f, 0.08f), 0.01f), level1Material);
            level2Model = Model.FromMesh(Mesh.GenerateRoundedCube(new Vec3(0.08f, 2 * 0.08f, 0.08f), 0.01f), level2Material);
            level3Model = Model.FromMesh(Mesh.GenerateRoundedCube(new Vec3(0.08f, 3 * 0.08f, 0.08f), 0.01f), level3Material);
            level4Model = Model.FromMesh(Mesh.GenerateRoundedCube(new Vec3(0.08f, 4 * 0.08f, 0.08f), 0.01f), level4Material);
            level5Model = Model.FromMesh(Mesh.GenerateRoundedCube(new Vec3(0.08f, 5 * 0.08f, 0.08f), 0.01f), level5Material);

            // Lower box
            lowerBoxPose = new Pose(new Vec3(2.6f, -0.12f, 0.4f), Quat.Identity);
            lowerBoxDimensions = new Vec3(76 * 0.08f, 0.13f, 0.13f * 7);
            lowerBoxModel = Model.FromMesh(Mesh.GenerateRoundedCube(lowerBoxDimensions, 0.01f), lowerBoxMaterial);

            labelPose = new Pose(new Vec3(1.2f, 0.5f, 0.5f), Quat.LookDir(new Vec3(0, 0, -1)));

            headingTextStyle = Text.MakeStyle(
                Default.Font,
                100 * U.cm,
                StereoKit.Color.White);

            Task.Run(async () =>
            {
                //userContributionCollection = await ContributionDayItem.GetContributionData();
                userContributionCollection = await ContributionDayItem.GetFakeContributionData();

                contributionDays = userContributionCollection.user.contributionsCollection.contributionCalendar
                        .weeks.SelectMany(x => x.contributionDays).ToArray();

                // Populate contribution DayLevels here
                for (int d = 0; d < contributionDays.Length; d++)
                {
                    var contributionCount = contributionDays[d].contributionCount;

                    if (contributionCount <= 0)
                    {
                        dayLevels.Add(1);
                    }
                    else if (contributionCount > 0 && contributionCount < 14)
                    {
                        dayLevels.Add(2);
                    }
                    else if (contributionCount >= 14 && contributionCount < 28)
                    {
                        dayLevels.Add(3);
                    }
                    else if (contributionCount >= 28 && contributionCount < 46)
                    {
                        dayLevels.Add(4);
                    }
                    else if (contributionCount >= 46)
                    {
                        dayLevels.Add(5);
                    }
                }

                int column = 0;
                int row = 0;

                for (int dayNumber = 1; dayNumber <= contributionDays.Length; dayNumber++)
                {
                    if (row == 7)
                    {
                        row = 0;
                        column++;
                    }

                    row++;

                    int level = dayLevels[dayNumber - 1];
                    var height = level * 0.08f;

                    if (level == 1)
                        height = 0.02f;

                    float y = (height) / 2;
                    float x = column * 0.1f;
                    float z = row * 0.1f;
                    cubePoses.Add(new Pose(x, y, z, Quat.Identity));

                    if (dayNumber == contributionDays.Length)
                    {
                        dataRetrievalComplete = true;
                        break;
                    }
                }

                dataRetrievalComplete = true;
            });
        }


        public void Step()
        {
            if (!dataRetrievalComplete)
                return;

            if (SK.System.displayType == Display.Opaque)
                Default.MeshCube.Draw(floorMaterial, floorTransform);

            for (int dayNumber = 0; dayNumber < cubePoses.Count; dayNumber++)
            {
                Pose pose = cubePoses[dayNumber];
                int level = dayLevels[dayNumber];
                Model cubeModel = level1Model;

                if (level == 1)
                    cubeModel = level1Model;
                else if (level == 2)
                    cubeModel = level2Model;
                else if (level == 3)
                    cubeModel = level3Model;
                else if (level == 4)
                    cubeModel = level4Model;
                else if (level == 5)
                    cubeModel = level5Model;

                UI.Handle($"Cube{dayNumber}", ref pose, cubeModel.Bounds);
                cubeModel.Draw(pose.ToMatrix());

                cubePoses[dayNumber] = pose;
            }

            // Draw Black box
            lowerBoxModel.Draw(lowerBoxPose.ToMatrix());

            var headingPose = new Vec3(-1f, -0.93f, 0.37f);
            var headingTextFit = TextFit.Squeeze;
            var headingAlign = TextAlign.XCenter;

            // Draw Labels
            UI.PushSurface(labelPose);
            {
                Text.Add("@LeeEnglestone",
                   Matrix.TR(headingPose, Quat.LookDir(0, 0, 1)),
                   textSize,
                   headingTextFit,
                   headingTextStyle,
                   TextAlign.Center | TextAlign.XCenter,
                   headingAlign);
            }
            UI.PopSurface();

            // Add Mon, Wed, Fri at side
            Text.Add("Mon",
                Matrix.TR(new Vec3(-0.2f, 0, 2 * 0.09f), Quat.LookDir(0, 1, 1)), new Vec2(0.1f, 0.1f),
                TextFit.Squeeze, headingTextStyle, TextAlign.Center, headingAlign);

            Text.Add("Wed",
                Matrix.TR(new Vec3(-0.2f, 0, 4 * 0.095f), Quat.LookDir(0, 1, 1)), new Vec2(0.1f, 0.1f),
                TextFit.Squeeze, headingTextStyle, TextAlign.Center, headingAlign);

            Text.Add("Fri ",
                Matrix.TR(new Vec3(-0.2f, 0, 6 * 0.095f), Quat.LookDir(0, 1, 1)), new Vec2(0.1f, 0.1f),
                TextFit.Squeeze, headingTextStyle, TextAlign.Center, headingAlign);

            // todo: Add Months along top

            // todo: Hand Menu (Unfinished)
            // Position the menu relative to the side of the hand

            Handed handed = Handed.Right;
            Hand hand = Input.Hand(handed);

            if (hand.IsPinched)
            {
                // Decide the size and offset of the menu
                Vec2 size = new Vec2(4, 16);
                float offset = handed == Handed.Left ? -2 - size.x : 2 + size.x;

                Vec3 at = hand[FingerId.Little, JointId.KnuckleMajor].position;
                Vec3 down = hand[FingerId.Little, JointId.Root].position;
                Vec3 across = hand[FingerId.Index, JointId.KnuckleMajor].position;

                Pose menuPose = new Pose(
                    at,
                    Quat.LookAt(at, across, at - down) * Quat.FromAngles(0, handed == Handed.Left ? 90 : -90, 0));
                menuPose.position += menuPose.Right * offset * U.cm;
                menuPose.position += menuPose.Up * (size.y / 2) * U.cm;

                // And make a menu!
                UI.WindowBegin("HandMenu", ref menuPose, size * U.cm, UIWin.Body);

                // todo: Make dynamic, not static
                UI.Label("Date: 01/01/2023");
                UI.Label("Contributions: 50");

                UI.WindowEnd();
            }
        }
    }

    public class ContributionDayItem
    {
        public int DayNumber { get; }
        public DateTime DateTime { get; }
        public int Level { get; }
        public int ContributionCount { get; }

        public ContributionDayItem(int dayNumber, DateTime dateTime, int level, int contributionCount)
        {
            DayNumber = dayNumber;
            DateTime = dateTime;
            Level = level;
            ContributionCount = contributionCount;
        }

        public static async Task<UserContributionCollection> GetFakeContributionData()
        {
            var userContributionCollection = new UserContributionCollection() { };
            var tempDate = new DateTime(2022, 01, 01);
            var random = new Random();
            var weeks = new List<Week>();

            for (int w = 0; w < 52; w++)
            {
                var week = new Week();
                var days = new List<ContributionDay>();

                for (int d = 0; d < 7; d++)
                {
                    var contributionDay = new ContributionDay();
                    contributionDay.date = tempDate.ToString("yyyy-MM-dd");
                    contributionDay.weekday = contributionDay.weekday = (int)tempDate.DayOfWeek;
                    contributionDay.contributionCount = random.Next(-30, 60);
                    days.Add(contributionDay);

                    tempDate = tempDate.AddDays(1);
                }

                week.contributionDays = days.ToArray();

                weeks.Add(week);
            }

            userContributionCollection
                .user
                .contributionsCollection
                .contributionCalendar
                .weeks = weeks.ToArray();

            return userContributionCollection;
        }

        public static async Task<UserContributionCollection> GetContributionData()
        {
            var graphQLHttpClientOptions = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("https://api.github.com/graphql")
            };

            #region Hide ApiToken
            string apiToken = GitHubCredentials.ApiKey;
            #endregion

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MyConsoleApp");

            string basicValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"leeenglestone:{apiToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicValue);

            var graphQLClient = new GraphQLHttpClient(graphQLHttpClientOptions, new NewtonsoftJsonSerializer(), httpClient);

            // You may want to provide your own GitHub username once you have your own API Key!
            var request = new GraphQLRequest
            {
                Query =
                @"{
      user(login: ""leeenglestone"") {
        name
        contributionsCollection {
          contributionCalendar {
            colors
            totalContributions
            weeks {
              contributionDays {
                color
                contributionCount
                date
                weekday
              }
              firstDay
            }
          }
        }
      }
    }"
            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<UserContributionCollection>(request);
            return graphQLResponse.Data;
        }
    }

    #region GitHub GraphQL Types

    public class UserContributionCollection
    {
        public User user { get; set; } = new User();
    }

    public class User
    {
        public string name { get; set; }
        public ContributionsCollection contributionsCollection { get; set; } = new ContributionsCollection();
    }

    public class ContributionsCollection
    {
        public ContributionCalendar contributionCalendar { get; set; } = new ContributionCalendar();
    }

    public class ContributionCalendar
    {
        public string[] colors { get; set; }
        public int totalContributions { get; set; }
        public Week[] weeks { get; set; }
    }

    public class Week
    {
        public ContributionDay[] contributionDays { get; set; }
        public string firstDay { get; set; }
    }

    public class ContributionDay
    {
        public string color { get; set; }
        public int contributionCount { get; set; }
        public string date { get; set; }
        public int weekday { get; set; }
    }

    #endregion
}