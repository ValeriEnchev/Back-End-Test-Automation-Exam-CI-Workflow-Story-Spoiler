using NUnit.Framework;
using NUnit.Framework.Internal;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using StorySpoilerTests.Models;

namespace StorySpoilerTests
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        // Application credential
        private const string UserName = "venchev70";
        private const string Password = "exam123";

        private static Random random = new Random();
        private static string lastCreatedStoryId = "";
        private const string nonExistedStoryId = "-1";

        private static string createdStoryTitle = "";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(UserName, Password);
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken),
            };
            client = new RestClient(options);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        // Get a new JWT Token from App
        private string GetJwtToken(string userName, string password)
        {
            RestClient tmpClient = new RestClient(BaseUrl);
            RestRequest? request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new { userName, password });
            RestResponse? response = tmpClient.Post(request);

            JsonElement content = JsonSerializer.Deserialize<JsonElement>(response.Content);
            string token = content.GetProperty("accessToken").ToString() ?? String.Empty;
            return token;
        }

        // Create a random string of specified length
        private static string GetRandomString(int length)
        {
            const string? chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        // Create a New Story Spoiler with the Required Fields
        [Test, Order(1)]
        public void Test1_CrateStory_ShouldReturnsCreated()
        {
            //•	Create a test to send a POST request to add a new story.
            createdStoryTitle = $"Story_{GetRandomString(6)}";
            var newStory = new StoryDTO
            {
                Title = createdStoryTitle,
                Description = $"Description {GetRandomString(16)}",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);
            var response = client.Execute(request);

            //•	Assert that the response status code is Created(201).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
 
            //•	Assert that the StoryId is returned in the response.
            Assert.That(jsonResponse?.StoryId, Is.Not.Null.And.Not.Empty, "Response does not contain a 'StoryId' property.");

            //•	Assert that the response message indicates the story was "Successfully created!".
            Assert.That(jsonResponse.Msg, Is.EqualTo("Successfully created!"));

            //•	Store the StoryId as a static member of the static member of the test class to maintain its value between test runs
            lastCreatedStoryId = jsonResponse.StoryId ?? "";
        }

        // Edit the Story Spoiler that you Created
        [Test, Order(2)]
        public void Test2_EditStoryTitle_ShouldReturnsOK()
        {
            // Create a test that sends a PUT request to edit the story using the StoryId from the story creation test as a path variable.
            var editedStory = new StoryDTO
            {
                Title = $"Edited_{GetRandomString(6)}",
                Description = $"Edited {GetRandomString(16)}"
            };

            var request = new RestRequest("/api/Story/Edit/{storyId}", Method.Put);
            request.AddUrlSegment("storyId", lastCreatedStoryId);
            request.AddJsonBody(editedStory);
            var response = client.Execute(request);

            // Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert that the response message indicates the story was "Successfully edited".
            Assert.That(jsonResponse?.Msg, Is.EqualTo("Successfully edited"));
        }

        // Get All Story Spoilers
        [Test, Order(3)]
        public void Test3_GetAllStories_ShoudShowNotEmpty()
        {
            // Create a test to send a GET request to list all stories.
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            // Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var jsonResponse = JsonSerializer.Deserialize<List<JsonElement>>(response.Content);

            // Assert that the response contains a non - empty array.
            Assert.That(jsonResponse, Is.InstanceOf<List<JsonElement>>());
            Assert.That(jsonResponse.Count, Is.GreaterThan(0));
            Assert.That(jsonResponse, Is.Not.Empty);
        }

        // Delete a Story Spoiler
        [Test, Order(4)]
        public void Test4_DeleteStory_ShouldDeleteEditedStory()
        {
            // Create test that sends a DELETE request using the StoryId from the created story.
            var request = new RestRequest("/api/Story/Delete/{storyId}", Method.Delete);
            request.AddUrlSegment("storyId", lastCreatedStoryId);
            var response = client.Execute(request);

            // Assert that the response status code is OK(200).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert that the response message is "Deleted successfully!".
            Assert.That(jsonResponse?.Msg, Is.EqualTo("Deleted successfully!"));
        }

        // Try to Create a Story Spoiler without the Required Fields
        [Test, Order(5)]
        public void Test5_CrateStory_WithoutRequiredFields_ShouldReturnsBadRequest()
        {
            // Write a test that attempts to create a story with missing required fields(Title, Description).
            var emptyStory = new StoryDTO
            {
                Title = "",
                Description = ""
            };

            // Send the POST request with the incomplete data.
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(emptyStory);
            var response = client.Execute(request);

            // Assert that the response status code is BadRequest (400).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("The Title field is required."));
            Assert.That(response.Content, Does.Contain("The Description field is required."));
        }

        // Edit a Non-existing Story Spoiler
        [Test, Order(6)]
        public void Test6_EditStoryTitle_OfNonExistingStory_ShouldReturnsNotFound()
        {
            // Write a test to send a PUT request to edit a story with a StoryId that does not exist.
            var nonExistedStory = new StoryDTO
            {
                Title = "Non existing story title",
                Description = "Non existing story description"
            };

            var request = new RestRequest("/api/Story/Edit/{storyId}", Method.Put);
            request.AddUrlSegment("storyId", nonExistedStoryId);
            request.AddJsonBody(nonExistedStory);
            var response = client.Execute(request);

            // Assert that the response status code is NotFound(404).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert that the response message indicates "No spoilers...".
            Assert.That(jsonResponse?.Msg, Is.EqualTo("No spoilers..."));
        }

        // Delete a Non-existing Story Spoiler
        [Test, Order(7)]
        public void Test7_DeleteStory_OfNonExistingStory_ShouldReturnsBadRequest()
        {
            // Write a test to send a DELETE request to edit a story with a StoryId that does not exist.
            var request = new RestRequest("/api/Story/Delete/{storyId}", Method.Delete);
            request.AddUrlSegment("storyId", nonExistedStoryId);
            var response = client.Execute(request);

            // Assert that the response status code is Bad request(400).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert that the response message indicates "Unable to delete this story spoiler!".
            Assert.That(jsonResponse?.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
    }
}