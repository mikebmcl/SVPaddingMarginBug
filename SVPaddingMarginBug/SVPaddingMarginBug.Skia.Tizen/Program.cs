using Tizen.Applications;

using Uno.UI.Runtime.Skia;

namespace SVPaddingMarginBug.Skia.Tizen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new SVPaddingMarginBug.App(), args);
            host.Run();
        }
    }
}
