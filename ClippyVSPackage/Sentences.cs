using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recoding.ClippyVSPackage
{
    public class Sentences
    {
        Clippy _clippy;
        Balloon _balloon;
        SpriteContainer _spriteContainer;

        public Sentences(Clippy clippy, Balloon balloon, SpriteContainer spriteContainer)
        {
            _clippy = clippy;
            _balloon = balloon;
            _spriteContainer = spriteContainer;
        }

        private static List<string> csharpSentences = new List<string>() { 
            "Ah! C#! I'll just sit here and look at you. Judging.",
            "C-sharp! Are we going to write a song, so?",
            "Do you now C# exists from around 2002? Feeling old, right?",
            "Nothing better than some good old OOP in the morning.",
            "Let's do some reflection!!!",
            "Don't forget to type all the things!",
            "Mhhhh, I see some dependency injection coming, am I right?",
            "Just remember: semicolons are used to denote the end of a statement. I feel so useful.",
            "Just remember: curly brackets are used to group statements.",
            "Just remember: variables are assigned using an equals sign, but compared using two consecutive equals signs. TWO, don't forget.",
            "Just remember: square brackets are used with arrays. I like arrays. And letters.",
            "Just remember: C# has explicit support for covariance and contravariance in generic types. Go on, write some of these... stuff."
        };

        public void OnFileOpened(string documentPath) 
        {
            string ext = Path.GetExtension(documentPath).ToLower();

            switch (ext)
            {
                case ".cs":
                    Random rnd = new Random();
                    int rndValue = rnd.Next(0, csharpSentences.Count - 1);

                    _balloon.ShowBalloon(csharpSentences[rndValue], _spriteContainer, new BalloonButton("Close", BalloonCommands.DoClose));
                    break;

                case ".txt":
                default:
                    break;
            }
        }
    }
}
