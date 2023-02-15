# Virtual Reality GitHub Contribution Viewer
Code to retrieve call the [GitHub GraphQL API](https://docs.github.com/en/graphql), retrieve  contribution information and display it in VR using C#, .NET and [StereoKit](https://stereokit.net), then view it on a Oculus Meta Quest Headset.

![GitHub contribution chart in VR](images/screenshot1.jpg)

## Walkthrough video
To view a video demonstrating the VR experience, see here:

[https://www.youtube.com/watch?v=cMpiN1irijo](https://www.youtube.com/watch?v=cMpiN1irijo)

## Solution structure

- **StereoKitApp** - You don't run this directly. The other projects depend on the logic in here. This is where you'll be making most of your code changes.

- **StereoKit_DotNet** - Run this one to see the simulator.

- **StereoKit_Android** - Runthis one when targeting the Quest headset to run on the headset.

- **StereoKit_UWP** - Don't worry about this one.

## Using fake/real data
By default, I have made the app use fake/random data when ran and does not call the GitHub API without changing the code as follows..

To call the GitHub API to retrieve and display your *real* contribution information..

1. Log in to the GitHub site then go to `Settings > Developer Settings > Personal access tokens` and create your own key API key.

2. Include the key in the `GitHubCredentials.cs` file. 


3. Change the code to call the real API
```
// Uncomment this section to retrieve real data
userContributionCollection = await ContributionDayItem.GetContributionData();
```
```
// Comment this call to retrieve fake data out
// userContributionCollection = await ContributionDayItem.GetFakeContributionData();
```
4. Change the code to retrieve data from YOUR profile, using YOUR login.
```
/ You may want to provide your own GitHub username once you have your own API Key!
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
```

## Running the code on the simulator
Whether you are using the default fake data or retrieving real data, to run the simulator, run the **StereoKit_DotNet** project.

Note: See the [StereoKit simulator controls](https://stereokit.net/Pages/Guides/Using-The-Simulator.html) on how to use the simulator.

## Running the code on the headset
1. [Setup an "Organisation"](https://developer.oculus.com/manage/organizations/) on the Meta/Oculus Quest Website.
2. Enable USB connections in Developer Settings on the Quest Headset
3. Plug in the cable to the headset, select the headset from the targets
4. un the **Stereokit_Android** project.

## Keep updated!
To see similar projects like this, follow me on:

- [Twitter](https://twitter.com/LeeEnglestone)
- [LinkedIn](https://www.linkedin.com/in/LeeEnglestone/)
- [YouTube](https://www.youtube.com/@LeeEnglestone/videos)