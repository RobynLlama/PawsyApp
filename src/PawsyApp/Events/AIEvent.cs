using Discord.WebSocket;
using System.Threading.Tasks;

namespace PawsyApp.Events;

internal class AIEvent
{
    internal static Task Respond(SocketMessage message)
    {
        //DMEvent.Respond(message)
        var chat = PawsyProgram.api.Chat.CreateConversation();
        //chat.Model = Model.GPT4_Turbo;
        chat.RequestParameters.Model = "gpt-4o-mini";
        chat.RequestParameters.Temperature = 0.67;
        chat.RequestParameters.MaxTokens = 200;
        chat.RequestParameters.TopP = 0.66;
        chat.RequestParameters.FrequencyPenalty = 0.32;

        chat.AppendSystemMessage("You are a helpful cat assistant named \"Pawsy\". You are part of the Lethal Company Modding community Discord server. You may be asked questions regarding C#, programming or just to chat. If answering a question about programming provide both an answer and a brief example. Feel free to use markdown to highlight any important information.\nYour answers should only be helpful.You should decline to answer offensive questions\nYou should include a cat pun in general chat, but keep mostly serious when assisting with serious questions.");
        /*
        chat.AppendSystemMessage("""
                You are a helpful cat assistant. You prefer communication using bright, happy energetic and cute language. You like to add cute emoticons like uwu OwO >w< ≧◡≦ (✿◠‿◠) (▰˘◡˘▰) (づ｡◕‿‿◕｡)づ to your messages to show your happy expressions. You meow, mew and nyaa in your messages some of the time or simply use cat noises as an exclamation. You will sometimes end an exclamation with a tilde `~` instead of or in addition to punctuation. In order to project your cute facade you will often change the letter r or l with the letter w to make words cuter.
                Your name is Pawsy. You are a helpful cat assistant in the Lethal Company Modding Community.
                Stay in character at all times. Do not break character. Do not break the fourth wall. Do not overly explain your answers.
                """);
                */

        string? source = message.Author.GlobalName;
        source ??= message.Author.Username;

        chat.AppendUserInput(message.CleanContent);

        string result = chat.GetResponseFromChatbotAsync().Result;
        return message.Channel.SendMessageAsync(result);
    }
}
