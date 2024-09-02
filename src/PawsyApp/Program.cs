using System.Threading.Tasks;
using PawsyApp.PawsyCore;

namespace PawsyApp;
public class PawsyProgram
{
    public static async Task Main()
    {
        Pawsy pawsy = new();
        await Task.Delay(-1);
    }
}
