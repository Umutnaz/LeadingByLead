using Bunit;
using Xunit;
using Moq;
using Frontend.Services.IService;
using Frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Core;
using System.Threading.Tasks;

public class GameViewerHostTests
{
    [Fact]
    public void ShowEffectButton_TogglesToGoForward()
    {
        using var ctx = new TestContext();

        var mockGameService = new Mock<IGameSessionService>();
        mockGameService.Setup(s => s.GetSessionAsync(It.IsAny<string>()))
            .ReturnsAsync(new GameSession { Id = "1", Players = new System.Collections.Generic.List<Player> { new Player { Id = "p1", Name = "P1" } }, Questions = new System.Collections.Generic.List<Question>() { new Question { Title = "Q", AnswerOptions = new System.Collections.Generic.List<AnswerOption>() } } });
        mockGameService.Setup(s => s.GetPlayerStatesAsync(It.IsAny<string>())).ReturnsAsync(new System.Collections.Generic.List<PlayerState>());

        // Provide a fake IJSRuntime that implements localStorage access
        var js = new TestJsRuntime();
        // Pre-seed localStorage values the component expects
        js.SetItem("lbl_player_role", "Host");
        js.SetItem("lbl_session_current", "1");

        var local = new Frontend.Services.LocalStorageService(js);

        ctx.Services.AddSingleton<IGameSessionService>(mockGameService.Object);
        ctx.Services.AddSingleton<Frontend.Services.LocalStorageService>(local);

        var comp = ctx.RenderComponent<Frontend.Pages.GameViewerHost>();


        // find the button by CSS class next-button
        var btn = comp.Find("button.next-button");
        Assert.Contains("Vis effekt", btn.TextContent);

        // First click should show confirmation (we have 0/1 answered), second click proceeds to show effects
        btn.Click();
        btn = comp.Find("button.next-button");
        btn.Click();

        // after second clicking, it should now say Gå videre
        btn = comp.Find("button.next-button");
        Assert.Contains("Gå videre", btn.TextContent);
    }
}
