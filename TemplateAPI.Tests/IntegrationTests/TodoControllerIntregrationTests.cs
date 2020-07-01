using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using TemplateAPI;
using Newtonsoft.Json;
using System.Text;
using System;
using TemplateAPI.Models;


namespace TemplateApi.Tests
{
    public class TodoControllerIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly HttpClient _notAuthorizedClient;

        public TodoControllerIntegrationTests()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<TemplateAPI.Startup>());
            _client = server.CreateClient();
            _notAuthorizedClient = server.CreateClient();

            var token = GetBearerToken();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.Result);

            // Create an entry as setup
            //todo add setup method? Is that a thing with xunit?
            TodoItemUpsertDTO item1 = new TodoItemUpsertDTO();
            item1.Name = "test";
            item1.IsComplete = true;

            var json = JsonConvert.SerializeObject(item1);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            _client.PostAsync("/api/TodoItems", data);
        }

        [Fact]
        public async Task Authorization_User_Returns200()
        {
            AuthUser userDetails = new AuthUser();
            userDetails.Username = "test";
            userDetails.Password = "test";

            var json = JsonConvert.SerializeObject(userDetails);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/TodoItems/authenticate", data);
            var jsonString = response.Content.ReadAsStringAsync();
            var responseItem = JsonConvert.DeserializeObject<User>(jsonString.Result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsType<string>(responseItem.Token);
        }

        [Fact]
        public async Task Authorization_UserBadCreds_Returns400()
        {
            AuthUser userDetails = new AuthUser();
            userDetails.Username = "test";
            userDetails.Password = "bad_pw";

            var json = JsonConvert.SerializeObject(userDetails);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/TodoItems/authenticate", data);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_TodoList_Returns200()
        {
            var response = await _client.GetAsync("/api/TodoItems");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
        }

        [Fact]
        public async Task Get_TodoItemById_Returns200()
        {
            var response = await _client.GetAsync("/api/TodoItems/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jsonString = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TodoItemResponseDTO>(jsonString.Result);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Get_TodoItemByBadId_Returns404()
        {
            var response = await _client.GetAsync("/api/TodoItems/100");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_TodoItemNotAuthorized_Returns401()
        {
            var response = await _notAuthorizedClient.GetAsync("/api/TodoItems/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Add_TodoItem_Returns201()
        {
            TodoItemUpsertDTO newItem = new TodoItemUpsertDTO();
            newItem.Name = "new item test";
            newItem.IsComplete = false;

            var json = JsonConvert.SerializeObject(newItem);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/TodoItems", data);

            var jsonString = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TodoItemResponseDTO>(jsonString.Result);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("new item test", model.Name);
            Assert.False(model.IsComplete);

        }

        [Fact]
        public async Task Add_TodoItemNotAuthorized_Returns401()
        {
            TodoItemUpsertDTO updateItem = new TodoItemUpsertDTO();
            updateItem.Name = "add item  fail test";
            updateItem.IsComplete = false;

            var json = JsonConvert.SerializeObject(updateItem);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _notAuthorizedClient.PostAsync("/api/TodoItems", data);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Update_TodoItemById_Returns200()
        {
            TodoItemUpsertDTO updateItem = new TodoItemUpsertDTO();
            updateItem.Name = "update item test";
            updateItem.IsComplete = false;

            var json = JsonConvert.SerializeObject(updateItem);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync("/api/TodoItems/1", data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var jsonString = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TodoItemResponseDTO>(jsonString.Result);
            Assert.Equal("update item test", model.Name);

        }

        [Fact]
        public async Task Update_TodoItemNotAuthorized_Returns401()
        {
            TodoItemUpsertDTO updateItem = new TodoItemUpsertDTO();
            updateItem.Name = "update item  fail test";
            updateItem.IsComplete = false;

            var json = JsonConvert.SerializeObject(updateItem);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _notAuthorizedClient.PutAsync("/api/TodoItems/2", data);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Delete_TodoItemById_Returns204()
        {
            //we want to add before we delete to not have any side effects 
            TodoItemUpsertDTO newItem = new TodoItemUpsertDTO();
            newItem.Name = "delete me";
            newItem.IsComplete = false;

            var json = JsonConvert.SerializeObject(newItem);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("/api/TodoItems", data);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            var jsonString = postResponse.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TodoItemResponseDTO>(jsonString.Result);

            var deleteResponse = await _client.DeleteAsync(String.Concat("/api/TodoItems/", model.Id.ToString()));
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }

        [Fact]
        public async Task Delete_TodoItemById_Returns404()
        {
            var response = await _client.DeleteAsync("/api/TodoItems/100");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        }

        [Fact]
        public async Task Delete_TodoItemNotAuthorized_Returns401()
        {
            var response = await _notAuthorizedClient.DeleteAsync("/api/TodoItems/2");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<string> GetBearerToken()
        {
            AuthUser userDetails = new AuthUser();
            userDetails.Username = "test";
            userDetails.Password = "test";

            var json = JsonConvert.SerializeObject(userDetails);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/TodoItems/authenticate", data);
            var jsonString = response.Content.ReadAsStringAsync();
            var responseItem = JsonConvert.DeserializeObject<User>(jsonString.Result);
            return responseItem.Token;
        }

    }
}