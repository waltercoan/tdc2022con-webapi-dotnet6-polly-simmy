namespace AppWebServer.Tests;

using System.Text.Json;
using AppWebServer.Models;
using Xunit;
using Xunit.Abstractions;

public class MonkeyCaosTest
{
    public MonkeyCaosTest(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    private ITestOutputHelper OutputHelper { get; }
    
    [Theory]
    [InlineData("Huguinho")]
    [InlineData("Zezinho")]
    [InlineData("Luizinho")]
    [InlineData("Mariazinha")]
    [InlineData("Pedrinho")]
    public async void RegistrationTest(string name)
    {
        await using var application = new APIApplication(OutputHelper);
        
        var client = application.CreateClient();
        var objData = new CourseRegistration() {
            Name = name,
            Date = new DateTime(),
            Course = "Bacharelado em Sistemas de Informação"
        };
        var response = await client.PostAsJsonAsync<CourseRegistration>("/CourseRegistration",objData);

        Assert.Equal(System.Net.HttpStatusCode.OK,response.StatusCode);
    }
    
}